using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using TubaMiddleware.Logging;
using TubaMiddleware.Models;

namespace TubaMiddleware.Middlewares
{
    public class ExceptionMiddleware
    {
        private static readonly ILogger Logger = new LoggerConfiguration()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IMediator _mediator;

        public ExceptionMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationException)
            {
                ApiErrorDto apiErrorDto;
                var messageTranslate = "request_model_is_invalid";

                //TODO: Service veye Data katmanından gelen ValidationException ları bu condition ile yakalabiliriz
                if (validationException.Message == "ValidationBehavior") messageTranslate = "request_model_is_invalid";

                if (validationException.Errors.Any())
                {
                    var errors = new Dictionary<string, string>();
                    foreach (var error in validationException.Errors)
                    {
                        if (errors.ContainsKey(error.PropertyName))
                            continue;

                        var errorMessageTranslate = error.ErrorMessage.Split('|')[0];
                        errors.Add(error.PropertyName, errorMessageTranslate);
                    }

                    apiErrorDto = new ApiErrorDto { Message = messageTranslate, Error = errors };

                    var logMessage = string.Join('|',
                        validationException.Errors.Select(s => $"{s.PropertyName}:{s.ErrorMessage}").FirstOrDefault());
                    Logger.Warning("Validation failed. Errors: {@ValidationErrors}", logMessage);
                }
                else
                {
                    apiErrorDto = new ApiErrorDto { Message = "Validasyon Hatası" };
                    Logger.Warning("Validation failed with messages {@ValidationMessages}",
                        validationException.Message);
                }

                await WriteLog(context, validationException, string.Empty);
                await ClearResponseAndBuildErrorDto(context, apiErrorDto, StatusCodes.Status201Created)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await WriteLog(context, exception, null, StatusCodes.Status500InternalServerError);
                await ClearResponseAndBuildErrorDto(context,
                        new ApiErrorDto { Message = "Internal Server Error" },
                        StatusCodes.Status500InternalServerError)
                    .ConfigureAwait(false);
            }
        }

        private static Task ClearResponseAndBuildErrorDto(HttpContext context, ApiErrorDto errorDto,
            int statusCode = StatusCodes.Status400BadRequest)
        {
            var response = JsonConvert.SerializeObject(errorDto);
            context.Response.Clear();

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return context.Response.WriteAsync(response, Encoding.UTF8);
        }

        private static Task ClearResponseAndBuildApiResponseDto(HttpContext context,
            ApiResponseDto apiResponseDto)
        {
            var response = JsonConvert.SerializeObject(apiResponseDto);
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return context.Response.WriteAsync(response, Encoding.UTF8);
        }

        private async Task<bool> WriteLog(HttpContext context, Exception exception, string data,
            int statusCode = StatusCodes.Status400BadRequest)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
            var request = context.Request;
            var logDetail = new LogDetail(request.Host.Host, request.Protocol, request.Method, request.Path,
                request.GetEncodedPathAndQuery(), statusCode, "correlationId is null")
            {
                RequestHeaders = request.Headers.ToDictionary(h => h.Key, h => (object) h.Value.ToString()),
                RequestBody = string.Empty,
                UserId = "userıd000",
                ApiKey = "ApiKey 54544",
                AuthenticationType = "auth Type is admin",
                Exception = exception,
                Data = data
            };
            File.AppendAllText("./time.txt", DateTime.Now.TimeOfDay + Environment.NewLine);

            Log.Error(JsonConvert.SerializeObject(logDetail));

            return await Task.FromResult(true);
        }
    }
}