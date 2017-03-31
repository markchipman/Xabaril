using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xabaril.Core;

namespace Xabaril.MVC
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple =true,Inherited =true)]
    public class FeatureFilter
        : Attribute,IFilterFactory
    {
        public string FeatureName { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var featuresService = scope.ServiceProvider.GetRequiredService<IFeaturesService>();

                return new FeatureResourceFilter(featuresService, FeatureName);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private class FeatureResourceFilter : IAsyncResourceFilter
        {
            private readonly IFeaturesService _featuresService;
            private readonly string _featureName;

            public FeatureResourceFilter(IFeaturesService featuresService,string featureName)
            {
                _featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
                
                if (String.IsNullOrWhiteSpace(featureName))
                {
                    throw new ArgumentNullException(nameof(featureName));
                }

                _featuresService = featuresService;
                _featureName = featureName;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                if (!await _featuresService.IsEnabledAsync(_featureName))
                {
                    context.Result = new NotFoundResult()
                    {

                    };
                }
                else
                {
                    await next();
                }
            }
        }
    }
}
