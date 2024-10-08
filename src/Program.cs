using System.Text.Json;
using DynamicObjectApi.Application;
using DynamicObjectApi.Domain;
using DynamicObjectApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IRuleEngineStrategy, RuleEngineStrategy>();
builder.Services.AddScoped<IBusinessLogicService, BusinessLogicService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope()){
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.MapGet("objects/{id:int}/type/{type}",
        async (int id, string type,
            ApplicationDbContext context) => {
            return await context.Objects.FirstOrDefaultAsync(x => x.Id == id && x.ObjectType == type);
        })
    .WithName("getObjectById")
    .WithOpenApi();

app.MapPost("objects",
        async ([FromBody] JsonElement data, [FromQuery] string objectType,
            ApplicationDbContext context,
            IBusinessLogicService businessLogicService) => {
            var jsonData = JObject.Parse(data.GetRawText());
            await businessLogicService.ApplyBusinessRules(objectType, jsonData);

            var model = new DynamicObject(){
                ObjectType = objectType,
                Data = JsonDocument.Parse(data.GetRawText())
            };
            context.Objects.Add(model);
            await context.SaveChangesAsync();
        })
    .WithName("createObjectByType")
    .WithOpenApi();

app.MapPut("objects/{id:int}/type/{{type}}", async (int id, string type, [FromBody] JsonElement data,
        ApplicationDbContext context,
        IBusinessLogicService businessLogicService) => {
        var jsonData = JObject.Parse(data.GetRawText());
        await businessLogicService.ApplyBusinessRules(type, jsonData);

        var dynamicObject = await context.Objects.FindAsync(id);
        if (dynamicObject != null){
            dynamicObject.Data = JsonDocument.Parse(data.GetRawText());
            dynamicObject.UpdatedAt = DateTime.UtcNow;
            context.Entry(dynamicObject).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    })
    .WithName("updateObjectById")
    .WithOpenApi();

app.MapDelete("objects/{id:int}", async (int id,
        ApplicationDbContext context) => {
        var dynamicObject = await context.Objects.FindAsync(id);
        if (dynamicObject != null) context.Objects.Remove(dynamicObject);
        await context.SaveChangesAsync();
    })
    .WithName("deleteObjectById")
    .WithOpenApi();

app.Run();