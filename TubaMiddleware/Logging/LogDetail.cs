using System;
using System.Collections.Generic;

namespace TubaMiddleware.Logging
{
    public class LogDetail
    {
        public LogDetail(string requestHost, string requestProtocol, string requestMethod, string requestPath,
            string requestPathAndQuery,
            int responseStatusCode, string correlationId)
        {
            Timestamp = DateTime.Now;
            RequestHost = requestHost;
            RequestProtocol = requestProtocol;
            RequestMethod = requestMethod;
            RequestPath = requestPath;
            RequestPathAndQuery = requestPathAndQuery;
            ResponseStatusCode = responseStatusCode;
            CorrelationId = correlationId;
        }

        private DateTime Timestamp { get; }
        public string MachineName => Environment.MachineName;
        public string Message { get; set; }

        public string RequestHost { get; set; }
        public string RequestProtocol { get; set; }
        public string RequestMethod { get; set; }
        public string RequestPath { get; set; }
        public string RequestPathAndQuery { get; set; }
        public int ResponseStatusCode { get; set; }
        public string CorrelationId { get; set; }
        public Dictionary<string, object> RequestHeaders { get; set; }
        public long? ElapsedMilliseconds { get; set; }
        public string RequestBody { get; set; }
        public string UserId { get; set; }
        public string ApiKey { get; set; }
        public string AuthenticationType { get; set; }
        public Exception Exception { get; set; }
        public string Data { get; set; }
    }
}