using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xabaril;
using Xabaril.Store;
using Xunit;

namespace UnitTests.Xabaril.Core
{
    public class feature_service_should
    {
        [Fact]
        public void throws_an_argument_null_exception_if_the_feature_does_not_exists()
        {
            var featureService = new FeatureServiceBuilder()
                                    .WithInMemoryStore()
                                    .Build();

            Func<Task<bool>> act = async () => await featureService.IsEnabledAsync("feature_name_does_not_exists");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task indicate_disable_if_failure_mode_is_log_and_disable()
        {
            var featureService = new FeatureServiceBuilder()
                                    .WithInMemoryStore()
                                    .WithOptions(new XabarilOptions()
                                    {
                                        FailureMode = FailureMode.LogAndDisable
                                    }).Build();

            (await featureService.IsEnabledAsync("feature_name_does_not_exists")).Should().Be(false);

        }

        [Fact]
        public async Task indicates_whether_a_feature_is_enabled()
        {
            var feature = Feature.EnabledFeature("Test#1");
            var featureService = new FeatureServiceBuilder()
                                    .WithInMemoryStore()
                                    .WithEnabledFeature(feature)
                                    .Build();

            (await featureService.IsEnabledAsync(feature.Name)).Should().Be(true);
        }

        [Fact]
        public async Task indicates_whether_a_feature_is_disabled()
        {
            var feature = Feature.DisabledFeature("Test#1");
            var featureService = new FeatureServiceBuilder()
                                    .WithInMemoryStore()
                                    .WithDisabledFeature(feature)
                                    .Build();

            (await featureService.IsEnabledAsync(feature.Name)).Should().Be(false);
        }
    }
}
