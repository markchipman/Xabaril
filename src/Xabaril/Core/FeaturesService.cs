using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xabaril.Store;

namespace Xabaril.Core
{
    public class FeaturesService : IFeaturesService
    {
        private readonly XabarilOptions _xabarilOptions;
        private readonly ILogger<XabarilModule> _logger;
        private readonly IFeaturesStore _featuresStore;
        private readonly SingleInstanceFactory _singleInstanceFactory;

        public FeaturesService(
            IOptions<XabarilOptions> options,
            ILogger<XabarilModule> logger,
            IFeaturesStore featuresStore,
            SingleInstanceFactory singleInstanceFactory)
        {
            _featuresStore = featuresStore ?? throw new ArgumentNullException(nameof(featuresStore));
            _singleInstanceFactory = singleInstanceFactory ?? throw new ArgumentNullException(nameof(singleInstanceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xabarilOptions = options?.Value ?? new XabarilOptions();
          
        }

        public async Task<bool> IsEnabledAsync(string featureName)
        {
            try
            {
                var feature = await _featuresStore.FindFeatureAsync(featureName);

                if (feature == null)
                {
                    throw new ArgumentException(nameof(featureName));
                }

                var types = await _featuresStore.FindFeatureActivatorsTypesAsync(featureName);

                foreach (var type in types)
                {
                    var activator = (IFeatureActivator)_singleInstanceFactory(type);

                    if (!await activator.IsActiveAsync(featureName))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (_xabarilOptions.FailureMode == FailureMode.Throw)
                {
                    throw;
                }
                else
                {
                    _logger.LogError($"Error executing IsEnabled({featureName}), the inner exception is {ex.ToString()}");

                    return false;
                }
            }

        }

        public async Task<bool> IsEnabledAsync<TFeature>() where TFeature : class
        {
            return await IsEnabledAsync(typeof(TFeature).Name);
        }
    }
}