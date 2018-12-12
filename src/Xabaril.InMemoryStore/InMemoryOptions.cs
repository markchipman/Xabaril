using System.Collections.Generic;
using Xabaril.Store;

namespace Xabaril.InMemoryStore
{
    public class InMemoryOptions
    {
        private List<FeatureConfigurer> _featuresConfiguration;

        public List<FeatureConfigurer> FeatureConfiguration => _featuresConfiguration;

        public FeatureConfigurer AddFeature(Feature feature)
        {
            if (_featuresConfiguration == null)
            {
                _featuresConfiguration = new List<FeatureConfigurer>();
            }

            var featureConfiguration = new FeatureConfigurer(feature);

            _featuresConfiguration.Add(featureConfiguration);

            return featureConfiguration;
        }
    }
}
