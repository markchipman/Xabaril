using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Xabaril.Store;

namespace Xabaril.Core
{
    public sealed class StoreRuntimeParameterAccesor
        : IRuntimeParameterAccessor
    {
        private readonly ILogger<XabarilModule> _logger;
        private readonly IFeaturesStore _featuresStore;

        public StoreRuntimeParameterAccesor(
            ILogger<XabarilModule> logger,
            IFeaturesStore featuresStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _featuresStore = featuresStore ?? throw new ArgumentNullException(nameof(featuresStore));
        }

        public async Task<TType> GetValueAsync<TType>(
            string featureName,
            ActivatorParameterDescriptor parameterDescriptor)
        {
            var feature = await _featuresStore.FindFeatureAsync(featureName);

            if (feature != null)
            {
                var parameter = await _featuresStore.FindParameterAsync(parameterDescriptor.Name, featureName, parameterDescriptor.ActivatorName);

                if (parameter != null)
                {
                    try
                    {
                        return (TType)TypeDescriptor.GetConverter(typeof(TType))
                            .ConvertFromString(null, CultureInfo.InvariantCulture, parameter.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"The parameter with name {parameterDescriptor.Name} on feature {featureName} can't be converted to type {typeof(TType).Name}. The internal exception is {ex}");
                    }
                }
                else
                {
                    _logger.LogWarning($"The parameter with name {parameterDescriptor.Name} is not configured on feature {featureName}.");
                }
            }
            else
            {
                _logger.LogWarning($"The feature with name {featureName} not exist on configured store.");
            }

            return default(TType);
        }
    }
}
