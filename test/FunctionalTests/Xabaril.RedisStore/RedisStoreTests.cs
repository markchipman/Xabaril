using FluentAssertions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xabaril;
using Xabaril.Core.Activators;
using Xabaril.RedisStore;
using Xabaril.Store;
using Xunit;

namespace FunctionalTests.Xabaril.RedisStore
{
    public class redis_store_should
    {
        [Fact]
        public async Task return_true_if_configuration_is_persisted_on_redis()
        {
            var store = new RedisStoreBuilder().Build();

            var configurer = new FeatureConfigurer("Test#1")
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", DateTime.UtcNow.AddDays(1));
                });

            var result = await store.PersistConfiguratioAsync(new List<FeatureConfigurer>()
            {
                configurer
            });

            result.Should().Be(true);
        }

        [Fact]
        public async Task return_parameter_if_is_stored()
        {
            var featureName = "Test#1";
            var date = DateTime.UtcNow.AddDays(1);

            var configurer = new FeatureConfigurer(featureName)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();

            var parameter = await store
                .FindParameterAsync("release-date", featureName, typeof(UTCActivator).FullName);

            parameter.Should().NotBeNull();
            parameter.Value.Should().Be(date.ToString());
        }

        [Fact]
        public async Task return_null_if_parameter__is_not_stored()
        {
            var featureName = "Test#1";
            var date = DateTime.UtcNow.AddDays(1);

            var configurer = new FeatureConfigurer(featureName)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();

            var parameter = await store
                .FindParameterAsync("non_existing", featureName, typeof(UTCActivator).FullName);

            parameter.Should().BeNull();
        }

        [Fact]
        public async Task return_feature_if_exist()
        {
            var featureName = "Test#1";
            var date = DateTime.UtcNow.AddDays(1);

            var configurer = new FeatureConfigurer(featureName)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });


            var store = new RedisStoreBuilder()
               .WithExistingData(new List<FeatureConfigurer>() { configurer })
               .Build();

            var feature = await store.FindFeatureAsync("Test#1");

            feature.Should().BeNull();
        }

        [Fact]
        public async Task return_null_feature_if_not_exist()
        {
            var store = new RedisStoreBuilder()
             .Build();

            var feature = await store.FindFeatureAsync("non_existing_feature");

            feature.Should().BeNull();
        }

        [Fact]
        public async Task return_all_persisted_activators()
        {
            var featureName = "Test#1";
            var date = DateTime.UtcNow.AddDays(1);

            var configurer = new FeatureConfigurer(featureName)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();


            var activators = await store.FindFeatureActivatorsTypesAsync("Test#1");

            activators.Should().NotBeNull();
            activators.Any().Should().Be(true);
        }

        [Fact]
        public async Task return_empty_if_feature_not_exist()
        {
            var featureName = "Test#1";
            var date = DateTime.UtcNow.AddDays(1);

            var configurer = new FeatureConfigurer(featureName)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();


            var activators = await store.FindFeatureActivatorsTypesAsync("non_existing_feature");

            activators.Should().NotBeNull();
            activators.Any().Should().Be(false);
        }


        class RedisStoreBuilder
        {
            IEnumerable<FeatureConfigurer> _configurer;

            public RedisStoreBuilder WithExistingData(IEnumerable<FeatureConfigurer> configurer)
            {
                _configurer = configurer;

                return this;
            }

            public RedisFeaturesStore Build()
            {
                var loggerFactory = new LoggerFactory();
                var logger = loggerFactory.CreateLogger<XabarilModule>();
                var libraries = DependencyContext.Default.CompileLibraries
                    .Select(c => c.Name);

                var redisManagerClient = new RedisManagerPool("localhost:6379");

                var store =  new RedisFeaturesStore(logger, redisManagerClient, libraries);

                if (_configurer != null)
                {
                    store.PersistConfiguratioAsync(_configurer).Wait();
                }

                return store;
            }
        }
    }
}
