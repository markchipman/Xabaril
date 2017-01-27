using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xabaril;
using Xabaril.Core.Activators;
using Xunit;

namespace UnitTests.Xabaril.Core.Activators
{
    public class utc_activator_should
    {
        [Fact]
        public async Task indicates_whether_activator_is_active()
        {
            var activator = new UTCActivatorBuilder()
                .WithReleaseDate(DateTime.UtcNow.AddDays(-1))
                .Build();

            (await activator.IsActiveAsync("some_feature")).Should().Be(true);
        }

        [Fact]
        public async Task indicates_whether_activator_is_not_active()
        {
            var activator = new UTCActivatorBuilder()
                .WithReleaseDate(DateTime.UtcNow.AddDays(1))
                .Build();

            (await activator.IsActiveAsync("some_feature")).Should().Be(false);
        }

        [Fact]
        public async Task use_dates_with_different_formats()
        {
            var activator = new UTCActivatorBuilder()
                .WithReleaseDate(DateTime.UtcNow.AddDays(-1), format: "yyyy/MM/dd")
                .Build();

            (await activator.IsActiveAsync("some_feature")).Should().Be(true);

            activator = new UTCActivatorBuilder()
                .WithReleaseDate(DateTime.UtcNow.AddDays(1), format: "yyyy/MM/dd")
                .Build();

            (await activator.IsActiveAsync("some_feature")).Should().Be(false);
        }


        [Fact]
        public void use_descriptor_with_activator_name_equals_to_activator_type_name()
        {
            var activator = new UTCActivatorBuilder()
             .WithReleaseDate(DateTime.UtcNow.AddDays(-1), format: "yyyy/MM/dd")
             .Build();

            var typeName = typeof(UTCActivator).Name;

            activator.Descriptors
                .ToList()
                .ForEach(d => d.ActivatorName.Should().BeEquivalentTo(typeName));
        }

        private class UTCActivatorBuilder
        {
            Dictionary<string, object> _parameters = new Dictionary<string, object>();

            public UTCActivator Build()
            {
                var loggerFactory = new LoggerFactory();
                var logger = loggerFactory.CreateLogger<XabarilModule>();

                var runtimeParameterAccessor = RuntimeParameterAccessorBuilder.Build(_parameters);

                return new UTCActivator(logger, runtimeParameterAccessor);
            }

            public UTCActivatorBuilder WithRuntimeParameters(Dictionary<string, object> parameters)
            {
                _parameters = parameters;

                return this;
            }

            public UTCActivatorBuilder WithReleaseDate(DateTime date, string format = "yyyy-MM-dd")
            {
                return WithRuntimeParameters(new Dictionary<string, object>()
                {
                    { "release-date", date.ToString(format) },
                    { "format-date", format },
                });
            }
        }
    }
}
