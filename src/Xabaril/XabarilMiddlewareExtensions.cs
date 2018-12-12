using Microsoft.AspNetCore.Builder;
using Xabaril;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class XabarilMiddlewareExtensions
    {
        public static IApplicationBuilder UseXabaril(this IApplicationBuilder app, string path)
        {
            app.UseMiddleware<XabarilMiddleware>(path);

            return app;
        }
    }
}
