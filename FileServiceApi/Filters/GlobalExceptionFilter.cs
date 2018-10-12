using System;
using System.Net;
using System.Threading.Tasks;
using FileServiceApi.Models;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace FileServiceApi.Filters
{
    public class GlobalExceptionFilter
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionFilter(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "An error occured.");
                await HandleExceptionAsync(httpContext, exception);
            }
        }

        private static Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return httpContext.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = "Internal Server Error occurred."
            }.ToString());
        }
    }
}