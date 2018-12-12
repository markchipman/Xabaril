using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Xabaril.Core;

namespace Xabaril
{
    public class XabarilMiddleware
    {
        private TemplateMatcher _requestPathMatcher;
        private readonly RequestDelegate _next;
        private readonly IFeaturesService _feturesService;
        private readonly string _path;

        public XabarilMiddleware(
            RequestDelegate next,
            IFeaturesService featuresService,
            string path)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _feturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
            _path = path;

            _requestPathMatcher = new TemplateMatcher(
                TemplateParser.Parse(path),
                new RouteValueDictionary());
        }

        public async Task Invoke(HttpContext context)
        {
            if (!IsXabarilRequest(context.Request))
            {
                await _next(context);

                return;
            }
            else
            {
                
            }

            var statusCode = HttpStatusCode.OK;
            var featureName = context.Request.Query["featureName"];
            var json = String.Empty;

            try
            {
                var isEnabled = await _feturesService.IsEnabledAsync(featureName);
                var data = new { isEnabled };
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

        private bool IsXabarilRequest(HttpRequest request)
        {
            return request.Method == HttpMethods.Get &&
                   _requestPathMatcher.TryMatch(request.Path, new RouteValueDictionary()) &&
                   request.Query.ContainsKey("featureName");
        }

        private Task WriteResponseAsync(
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
