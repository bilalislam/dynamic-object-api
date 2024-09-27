using System.Net;
using DynamicObjectApi.Domain;

namespace DynamicObjectApi.Application;

public class ExceptionHandlingMiddleware(RequestDelegate next){
    public async Task InvokeAsync(HttpContext context){
        try{
            await next(context);
        }
        catch (Exception ex){
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception){
        context.Response.ContentType = "application/json";
        if (exception is ValidationException){
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else{
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        var response = new{
            context.Response.StatusCode,
            Message = "An unexpected error occurred.",
            Detailed = exception.Message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}