using System.Collections.Generic;
using Xabaril.Store;

namespace Xabaril.RedisStore
{
    public class RedisOptions
    {
        private List<FeatureConfigurer> _featuresConfiguration;
        private string _defaultRedisHost = "localhost:6379";

        public List<FeatureConfigurer> FeatureConfiguration => _featuresConfiguration;

        public string RedisHost
        {
            get
            {
                return _defaultRedisHost;
            }
            set
            {
                _defaultRedisHost = value;
            }
        }

        public FeatureConfigurer AddFeature(string featureName)
        {
            if (_featuresConfiguration == null)
            {
                _featuresConfiguration = new List<FeatureConfigurer>();
            }

            var featureConfiguration = new FeatureConfigurer(featureName);

            _featuresConfiguration.Add(featureConfiguration);

            return featureConfiguration;
        }
    }
}
