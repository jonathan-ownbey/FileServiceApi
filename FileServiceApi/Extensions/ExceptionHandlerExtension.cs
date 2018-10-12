using FileServiceApi.Filters;
using Microsoft.AspNetCore.Builder;

namespace FileServiceApi.Extensions
{
    public static class ExceptionHandlerExtension
    {
        public static void ConfigureGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionFilter>();
        }
    }
}