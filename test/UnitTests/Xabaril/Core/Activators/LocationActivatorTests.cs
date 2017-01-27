using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xabaril;
using Xabaril.Core;
using Xabaril.Core.Activators;
using Xunit;

namespace UnitTests.Xabaril.Core.Activators
{
    public class location_activator_should
    {
        [Fact]
        public async Task indicates_whether_activator_is_active()
        {
            var activator = new LocationActivatorBuilder()
                .WithRuntimeParameters(new Dictionary<string, object>()
                {
                    {"locations","SPAIN;PORTUGAL;FRANCE" }
                })
                .WithCurrentActualLocation("SPAIN")
                .Build();

            (await activator.IsActiveAsync("featureName")).Should().Be(true);
        }


        [Fact]
        public async Task indicates_whether_activator_is_not_active()
        {
            var activator = new LocationActivatorBuilder()
                .WithRuntimeParameters(new Dictionary<string, object>()
                {
                    {"locations","PORTUGAL;FRANCE" }
                })
                .WithCurrentActualLocation("SPAIN")
                .Build();

            (await activator.IsActiveAsync("featureName")).Should().Be(false);
        }


        [Fact]
        public async Task not_be_case_sensitive()
        {
            var activator = new LocationActivatorBuilder()
               .WithRuntimeParameters(new Dictionary<string, object>()
               {
                    {"locations","sPaIn;FRANCE" }
               })
               .WithCurrentActualLocation("SPAIN")
               .Build();

            (await activator.IsActiveAsync("featureName")).Should().Be(true);
        }

        [Fact]
        public void use_descriptor_with_activator_name_equals_to_activator_type_name()
        {
            var activator = new LocationActivatorBuilder()
              .WithRuntimeParameters(new Dictionary<string, object>()
              {
                    {"locations","Spain;France" }
              })
              .Build();

            var typeName = typeof(LocationActivator).Name;

            activator.Descriptors
                .ToList()
                .ForEach(d => d.ActivatorName.Should().BeEquivalentTo(typeName));
        }


        private class LocationActivatorBuilder
        {
            IGeoLocationProvider _geoLocationProvider;

            Dictionary<string, object> _parameters = new Dictionary<string, object>();

            public LocationActivator Build()
            {
                var loggerFactory = new LoggerFactory();
                var logger = loggerFactory.CreateLogger<XabarilModule>();

                var runtimeParameterAccessor = RuntimeParameterAccessorBuilder.Build(_parameters);

                var httpContextAccesor = new HttpContextAccessor();
                httpContextAccesor.HttpContext = new DefaultHttpContext();
                httpContextAccesor.HttpContext.Connection.RemoteIpAddress = new IPAddress(0x2414188f);

                return new LocationActivator(logger, runtimeParameterAccessor, httpContextAccesor,_geoLocationProvider);
            }

            public LocationActivatorBuilder WithRuntimeParameters(Dictionary<string, object> parameters)
            {
                _parameters = parameters;

                return this;
            }


            public LocationActivatorBuilder WithCurrentActualLocation(string currentLocation)
            {
                _geoLocationProvider = new SimpleGeoLocationProvider(currentLocation);

                return this;
            }
        }

        private class SimpleGeoLocationProvider
           : IGeoLocationProvider
        {
            string _currentLocation;

            public SimpleGeoLocationProvider(string currentLocation)
            {
                _currentLocation = currentLocation;
            }

            public Task<string> FindLocationAsync(string ipAddress)
            {
                return Task.FromResult<string>(_currentLocation);
            }
        }
    }
}
