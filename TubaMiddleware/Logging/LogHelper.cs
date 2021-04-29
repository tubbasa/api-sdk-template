using System.Linq;
using Serilog;

namespace TubaMiddleware.Logging
{
    public class LogHelper
    {
        public static readonly string LogTemplate =
            $"HTTP {"{" + nameof(LogDetail.RequestMethod) + "}"} {"{" + nameof(LogDetail.RequestPathAndQuery) + "}"} responded {"{" + nameof(LogDetail.ResponseStatusCode) + "}"}";

        public static readonly string LogTemplateWithElapsed =
            $"HTTP {"{" + nameof(LogDetail.RequestMethod) + "}"} {"{" + nameof(LogDetail.RequestPathAndQuery) + "}"} responded {"{" + nameof(LogDetail.ResponseStatusCode) + "}"} in {"{" + nameof(LogDetail.ElapsedMilliseconds) + "}"} ms";

        public static readonly string LogTemplateError =
            $"ERROR : HTTP {"{" + nameof(LogDetail.RequestMethod) + "}"} {"{" + nameof(LogDetail.RequestPathAndQuery) + "}"} responded {"{" + nameof(LogDetail.ResponseStatusCode) + "}"}";

        public static readonly string LogTemplateRequest =
            $"ERROR : HTTP {"{" + nameof(LogDetail.RequestMethod) + "}"} {"{" + nameof(LogDetail.RequestPathAndQuery) + "}"} responded {"{" + nameof(LogDetail.ResponseStatusCode) + "}"}";


        public static ILogger GetLogDetail(ILogger logger, LogDetail logDetail)
        {
            if (logDetail == null)
                return logger;

            logger = logger
                .ForContext(nameof(logDetail.MachineName), logDetail.MachineName)
                .ForContext(nameof(logDetail.RequestHost), logDetail.RequestHost)
                .ForContext(nameof(logDetail.RequestProtocol), logDetail.RequestProtocol)
                .ForContext(nameof(logDetail.RequestMethod), logDetail.RequestMethod)
                .ForContext(nameof(logDetail.ResponseStatusCode), logDetail.ResponseStatusCode)
                .ForContext(nameof(logDetail.RequestPath), logDetail.RequestPath)
                .ForContext(nameof(logDetail.RequestPathAndQuery), logDetail.RequestPathAndQuery)
                .ForContext(nameof(logDetail.CorrelationId), logDetail.CorrelationId);

            if (logDetail.RequestHeaders != null && logDetail.RequestHeaders.Any())
                logger = logger.ForContext(nameof(logDetail.RequestHeaders), logDetail.RequestHeaders, true);

            if (logDetail.ElapsedMilliseconds != null)
                logger = logger.ForContext(nameof(logDetail.ElapsedMilliseconds), logDetail.ElapsedMilliseconds);

            if (!string.IsNullOrEmpty(logDetail.RequestBody))
                logger = logger.ForContext(nameof(logDetail.RequestBody), logDetail.RequestBody);

            if (!string.IsNullOrEmpty(logDetail.AuthenticationType))
                logger = logger.ForContext(nameof(logDetail.UserId), logDetail.UserId)
                    .ForContext(nameof(logDetail.ApiKey), logDetail.ApiKey)
                    .ForContext(nameof(logDetail.AuthenticationType), logDetail.AuthenticationType);

            if (logDetail.Exception != null)
                logger = logger.ForContext(nameof(logDetail.Exception), logDetail.Exception, true);

            if (!string.IsNullOrEmpty(logDetail.Data))
                logger = logger.ForContext(nameof(logDetail.Data), logDetail.Data);

            return logger;
        }
    }
}