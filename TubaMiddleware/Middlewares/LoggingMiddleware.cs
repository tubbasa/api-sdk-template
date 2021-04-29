using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;
using TubaMiddleware.Logging;

namespace TubaMiddleware.Middlewares
{
    public class LoggingMiddleware
    {
        private static readonly ILogger Logger = new LoggerConfiguration()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

        private readonly RequestDelegate _next;
        private Stopwatch _sw;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var bodyAsText = string.Empty;
            _sw = Stopwatch.StartNew();
            await _next(context);
            _sw.Stop();

            await WriteLog(context, bodyAsText);
        }

        private static async Task<string> GetBodyAsText(HttpContext context)
        {
            using (var bodyReader = new StreamReader(context.Request.Body))
            {
                var injectedRequestStream = new MemoryStream();
                var bodyAsText = bodyReader.ReadToEnd();
                var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);
                injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                injectedRequestStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = injectedRequestStream;

                return bodyAsText;
            }
        }

        private async Task<bool> WriteLog(HttpContext context, string requestBody)
        {
            var request = context.Request;
            string apiKey = string.Empty, authenticationType = string.Empty, userId = string.Empty;
            var logDetail = new LogDetail(request.Host.Host, request.Protocol, request.Method, request.Path,
                request.GetEncodedPathAndQuery(), context.Response.StatusCode, "correlationId")
            {
                ElapsedMilliseconds = _sw?.ElapsedMilliseconds ?? 0,
                RequestHeaders = request.Headers.ToDictionary(h => h.Key, h => (object) h.Value.ToString()),
                RequestBody = requestBody,
                UserId = userId,
                ApiKey = apiKey,
                AuthenticationType = authenticationType
            };

            LogHelper.GetLogDetail(Logger, logDetail).Information(LogHelper.LogTemplateWithElapsed);

            return await Task.FromResult(true);
        }
    }
}