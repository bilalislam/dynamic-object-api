using System.Text.Json;
using DynamicObjectAPI.Data;
using DynamicObjectApi.Models;
using DynamicObjectApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<BusinessLogicService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("objects/{id:int}", async (int id, ApplicationDbContext context) =>
    {
        var dynamicObject = await context.Objects.FindAsync(id);
        return dynamicObject;
    })
    .WithName("getObjectById")
    .WithOpenApi();

app.MapPost("objects",
        async (ApplicationDbContext context, BusinessLogicService businessLogicService, [FromBody] JsonElement data,
            [FromQuery] string objectType) =>
        {
            var jsonData = JObject.Parse(data.GetRawText());
            businessLogicService.ApplyBusinessRules(objectType, jsonData);

            var model = new DynamicObject()
            {
                ObjectType = objectType,
                Data = JsonDocument.Parse(data.GetRawText())
            };
            context.Objects.Add(model);
            await context.SaveChangesAsync();
        })
    .WithName("createObjectByType")
    .WithOpenApi();

app.MapPut("objects/{id:int}", async (int id, [FromBody] JsonElement data, ApplicationDbContext context) =>
    {
        var dynamicObject = await context.Objects.FindAsync(id);
        if (dynamicObject != null)
        {
            dynamicObject.Data = JsonDocument.Parse(data.GetRawText());
            dynamicObject.UpdatedAt = DateTime.UtcNow;
            context.Entry(dynamicObject).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    })
    .WithName("updateObjectById")
    .WithOpenApi();

app.MapDelete("objects/{id:int}", async (int id, ApplicationDbContext context) =>
    {
        var dynamicObject = await context.Objects.FindAsync(id);
        if (dynamicObject != null) context.Objects.Remove(dynamicObject);
        await context.SaveChangesAsync();
    })
    .WithName("deleteObjectById")
    .WithOpenApi();

app.Run();