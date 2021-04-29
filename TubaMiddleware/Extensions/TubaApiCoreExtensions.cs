using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TubaMiddleware.Middlewares;
using TubaMiddleware.PipeLines;

namespace TubaMiddleware.Extensions
{
    public static class TubaApiCoreExtensions
    {
        public static void TubaApiMediaTrBehaviorConfigure(this IServiceCollection services, bool validationBehaviour,
            bool loggingBehaviour)
        {
            if (validationBehaviour) services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            if (loggingBehaviour) services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }

        public static void TubaApiMediaTrMiddlewareConfigure(this IApplicationBuilder app,
            bool loggingMiddleware,
            bool exceptionMiddleware
        )
        {
            if (exceptionMiddleware)
                app.UseMiddleware<ExceptionMiddleware>();


            if (loggingMiddleware)
                app.UseMiddleware<LoggingMiddleware>();
        }
    }
}