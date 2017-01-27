using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Xabaril.Core;

namespace Xabaril
{
    public class XabarilMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFeaturesService _feturesService;
        private readonly PathString _path;

        public XabarilMiddleware(
            RequestDelegate next,
            IFeaturesService featuresService,
            PathString path)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (featuresService == null)
            {
                throw new ArgumentNullException(nameof(featuresService));
            }

            _next = next;
            _feturesService = featuresService;
            _path = path;
        }

        public async Task Invoke(HttpContext context)
        {
            if (string.Equals(context.Request.Path, _path, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
                context.Request.Query.ContainsKey("featureName"))
            {
                var statusCode = HttpStatusCode.OK;
                var featureName = context.Request.Query["featureName"];
                var json = String.Empty;

                try
                {
                    var isEnabled = await _feturesService.IsEnabledAsync(featureName).ConfigureAwait(false);
                    var data = new { isEnabled = isEnabled };
                    json = JsonConvert.SerializeObject(data);
                }
                catch (ArgumentException)
                {
                    statusCode = HttpStatusCode.NotFound;
                }
                
                await WriteResponseAsync(
                    context,
                    json,
                    "application/json",
                    statusCode);
            }
            else
            {
                await _next(context);
            }
        }

        protected Task WriteResponseAsync(
           HttpContext context,
           string content,
           string contentType,
           HttpStatusCode statusCode)
        {
            context.Response.Headers["Content-Type"] = new[] { contentType };
            context.Response.Headers["Cache-Control"] = new[] { "no-cache, no-store, must-revalidate" };
            context.Response.Headers["Pragma"] = new[] { "no-cache" };
            context.Response.Headers["Expires"] = new[] { "0" };
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(content);
        }
    }
}
