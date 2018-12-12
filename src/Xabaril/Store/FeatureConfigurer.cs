using System;
using System.Collections.Generic;
using Xabaril.Core;

namespace Xabaril.Store
{
    public class FeatureConfigurer
    {
        public Feature Feature { get; }

        public Dictionary<Type, Dictionary<string, object>> Configuration { get; }

        public FeatureConfigurer(Feature feature)
        {
            Feature = feature ?? throw new ArgumentNullException("featureName");
            Configuration = new Dictionary<Type, Dictionary<string, object>>();
        }

        public FeatureConfigurer WithActivator<TFeatureActivator>(Action<Dictionary<string, object>> configureParameters)
            where TFeatureActivator : IFeatureActivator
        {
            var parameters = new Dictionary<string, object>();

            configureParameters(parameters);

            Configuration.Add(typeof(TFeatureActivator), parameters);

            return this;
        }
    }
}
