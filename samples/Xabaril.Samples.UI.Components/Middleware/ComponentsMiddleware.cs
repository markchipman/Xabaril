using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Reflection;
using Xabaril.Samples.UI.Components.ViewComponents;

namespace Xabaril.Samples.UI.Components.Middleware
{
    public static class ComponentsMiddleware
    {

        public static void RegisterSampleComponents(this IServiceCollection services)
        {
            var assemblyType = typeof(HeaderViewComponent).GetTypeInfo();

            var embeddedFileProvider = new EmbeddedFileProvider(
                 assemblyType.Assembly,
                 assemblyType.Assembly.GetName().Name
            );         

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });
        }
    }
}
