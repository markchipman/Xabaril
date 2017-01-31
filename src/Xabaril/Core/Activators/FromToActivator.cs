using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Xabaril.Core.Activators
{
    public class FromToActivator
        : IFeatureActivator, IDiscoverableActivatorParameters
    {
        private readonly ILogger<XabarilModule> _logger;
        private readonly IRuntimeParameterAccessor _runtimeParameterAccessor;

        List<ActivatorParameterDescriptor> _descriptors = new List<ActivatorParameterDescriptor>()
        {
             new ActivatorParameterDescriptor() {Name = "release-from-date", ClrType=typeof(String).Name , IsOptional = false,ActivatorName = typeof(FromToActivator).Name},
             new ActivatorParameterDescriptor() {Name = "release-to-date", ClrType=typeof(String).Name ,IsOptional = false,ActivatorName = typeof(FromToActivator).Name},
             new ActivatorParameterDescriptor() {Name = "format-date",ClrType=typeof(String).Name,IsOptional = true,ActivatorName = typeof(FromToActivator).Name}
        };

        public IEnumerable<ActivatorParameterDescriptor> Descriptors
        {
            get
            {
                return _descriptors;
            }
        }

        public FromToActivator(ILogger<XabarilModule> logger, IRuntimeParameterAccessor runtimeParameterAccessor)
        {
            if (runtimeParameterAccessor == null)
            {
                throw new ArgumentNullException(nameof(runtimeParameterAccessor));
            }

            _logger = logger;
            _runtimeParameterAccessor = runtimeParameterAccessor;
        }

        public async Task<bool> IsActiveAsync(string featureName)
        {
            DateTime releaseFromDate, releaseToDate;

            var from = await _runtimeParameterAccessor.GetValueAsync<string>(featureName, _descriptors[0]);
            var to = await _runtimeParameterAccessor.GetValueAsync<string>(featureName, _descriptors[1]);
            var format = await _runtimeParameterAccessor.GetValueAsync<string>(featureName, _descriptors[2]);
            var now = DateTime.UtcNow;

            if (from != null && to != null)
            {
                if (format != null)
                {
                    releaseFromDate = DateTime.ParseExact(from,
                        format,
                        null,
                        DateTimeStyles.AssumeUniversal);

                    releaseToDate = DateTime.ParseExact(to,
                        format,
                        null,
                        DateTimeStyles.AssumeUniversal);
                }
                else
                {
                    releaseFromDate = DateTime.Parse(from, null,
                        DateTimeStyles.AssumeUniversal);

                    releaseToDate = DateTime.Parse(to, null,
                        DateTimeStyles.AssumeUniversal);
                }

                return now >= releaseFromDate && now <= releaseToDate;
            }

            return false;
        }
    }
}
