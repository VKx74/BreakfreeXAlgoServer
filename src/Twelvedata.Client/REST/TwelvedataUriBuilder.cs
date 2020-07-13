using System;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Algoserver.Client.REST
{
    public static class TwelvedataUriBuilder
    {
        public static string GetUri(string uriTemplate, object request)
        {
            var queryProps = request.GetType().GetProperties().Where(p => p.GetValue(request) != null && p.GetCustomAttributes<QueryParameterAttribute>().Any())
                .Select(a =>
                new
                {
                    Name = a.GetCustomAttributes<QueryParameterAttribute>().First().Name,
                    Prop = a
                });

            var queryParams = queryProps.Select(a => $"{a.Name}={GetPropValue(a.Prop, request)}");
            var query = $"{string.Join("&", queryParams)}";

            uriTemplate = string.IsNullOrEmpty(query) ? uriTemplate : $"{uriTemplate}&{query}";
            return uriTemplate;
        }

        private static string GetPropValue(PropertyInfo prop, object request)
        {
            if (prop.PropertyType == typeof(DateTime?))
            {
                return ((DateTime?)prop.GetValue(request))?.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (prop.PropertyType == typeof(bool?) || prop.PropertyType == typeof(bool))
            {
                return ((bool?)prop.GetValue(request))?.ToString().ToLowerInvariant();
            }

            return HttpUtility.UrlEncode(prop.GetValue(request)?.ToString());
        }
    }
}
