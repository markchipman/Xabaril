using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xabaril.Core;

namespace Xabaril.MVC
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FeatureToogleAttribute
        : Attribute, IActionConstraintFactory
    {
        public string FeatureName { get; set; }

        public int Order { get; set; }

        public bool IsReusable => false;

        public IActionConstraint CreateInstance(IServiceProvider serviceProvider)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var featuresService = scope.ServiceProvider.GetRequiredService<IFeaturesService>();

                return new FeatureConstraintFilter(featuresService, FeatureName, Order);
            }
        }

        private class FeatureConstraintFilter
            : IActionConstraint
        {

            private readonly IFeaturesService _featureService;

            private string _featureName;
            private int _order;

            public FeatureConstraintFilter(IFeaturesService featureService, string featureName, int order)
            {
                _featureService = featureService ?? throw new ArgumentNullException(nameof(featureService));
                _featureName = featureName;
                _order = order;
            }

            public int Order => _order;

            public bool Accept(ActionConstraintContext context)
            { 
                var isEnabled = _featureService.IsEnabledAsync(_featureName).Result;

                return isEnabled;
            }
        }
    }
}
