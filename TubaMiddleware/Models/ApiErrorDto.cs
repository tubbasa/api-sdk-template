using System.Collections.Generic;

namespace TubaMiddleware.Models
{
    public class ApiErrorDto
    {
        public string Message { get; set; }
        public Dictionary<string, string> Error { get; set; }
    }
}