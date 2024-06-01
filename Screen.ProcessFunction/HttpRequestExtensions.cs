using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Screen.ProcessFunction
{
    public static class HttpRequestExtensions
    {
        public static IDictionary<string, string> GetQueryParameterDictionary(this HttpRequest request)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var pair in request.Query)
            {
                dictionary[pair.Key] = pair.Value;
            }
            return dictionary;
        }
    }
}
