
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using NewDotnet.Models;
using Microsoft.EntityFrameworkCore;
using NewDotnet.Context;
namespace NewDotnet.Middleware
{

    // Middleware to log exceptions to a database
    public class ExceptionLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        // Assuming DatabaseService is a service to handle database operations
        private readonly IServiceScopeFactory _scopeFactory;
        public ExceptionLoggerMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory= scopeFactory;

        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e);
                throw;
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string excMessage = BuildExceptionMessage(exception);
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OODBModelContext>();
                // use dbContext here
                var db = new DataLayer.Database(dbContext);
                // Here you would log to your database or other logging mechanism
                db.LogAuditEvent("exc", "An exception occurred: {ExceptionMessage}", excMessage);

            }


            return Task.CompletedTask;
        }
        private string BuildExceptionMessage(Exception exception)
        {
            string message = "";
            while (exception != null)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += "\n----------------------------------------\n";
                }
                message += exception.ToString();
                exception = exception.InnerException;
            }
            return message;
        }
    }
    public static class ExceptionLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionLoggerMiddleware>();
        }
    }
} 
    


    
