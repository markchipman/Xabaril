using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xabaril.Core.Activators
{
    public sealed class LocationActivator
        : IFeatureActivator, IDiscoverableActivatorParameters
    {
        private readonly ILogger<XabarilModule> _logger;
        private readonly IRuntimeParameterAccessor _runtimeParameterAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeoLocationProvider _geoLocationProvider;

        List<ActivatorParameterDescriptor> _descriptors = new List<ActivatorParameterDescriptor>()
        {
            new ActivatorParameterDescriptor() {Name = "locations", ClrType=typeof(Double).Name , IsOptional = false,ActivatorName = typeof(LocationActivator).Name}
        };

        public IEnumerable<ActivatorParameterDescriptor> Descriptors
        {
            get
            {
                return _descriptors;
            }
        }

        public LocationActivator(ILogger<XabarilModule> logger,
            IRuntimeParameterAccessor runtimeParameterAccessor,
            IHttpContextAccessor httpContextAccessor,
            IGeoLocationProvider geoLocationProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeParameterAccessor = runtimeParameterAccessor ?? throw new ArgumentNullException(nameof(runtimeParameterAccessor));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _geoLocationProvider = geoLocationProvider ?? throw new ArgumentNullException(nameof(geoLocationProvider));
        }

        public async Task<bool> IsActiveAsync(string featureName)
        {
            var active = false;

            var locations = await _runtimeParameterAccessor
                .GetValueAsync<string>(featureName, _descriptors[0]);

            if (!String.IsNullOrWhiteSpace(locations))
            {
                var splittedLocations = locations.Split(';');

                var requestIp = _httpContextAccessor.HttpContext
                    .Connection
                    .RemoteIpAddress
                    .MapToIPv4()
                    .ToString();

                var location = await _geoLocationProvider.FindLocationAsync(requestIp);

                if (location != null)
                {
                    active = splittedLocations
                        .Any(item => item.Equals(location, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            return active;
        }
    }
}
