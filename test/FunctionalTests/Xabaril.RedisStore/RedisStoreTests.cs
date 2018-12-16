using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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
        [SkipOnAppVeyor]
        public async Task return_true_if_configuration_is_persisted_on_redis()
        {
            var store = new RedisStoreBuilder().Build();

            var configurer = new FeatureConfigurer(CreateFeature())
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

        [SkipOnAppVeyor]
        public async Task return_parameter_if_is_stored()
        {
            var date = DateTime.UtcNow.AddDays(1);
            var feature = CreateFeature();
            var configurer = new FeatureConfigurer(feature)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();

            var parameter = await store
                .FindParameterAsync("release-date", feature.Name, typeof(UTCActivator).FullName);

            parameter.Should().NotBeNull();
            parameter.Value.Should().Be(date.ToString());
        }

        [SkipOnAppVeyor]
        public async Task return_null_if_parameter_is_not_stored()
        {
            var date = DateTime.UtcNow.AddDays(1);
            var feature = CreateFeature();
            var configurer = new FeatureConfigurer(feature)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();

            var parameter = await store
                .FindParameterAsync("non_existing", feature.Name, typeof(UTCActivator).FullName);

            parameter.Should().BeNull();
        }

        [SkipOnAppVeyor]
        public async Task return_feature_if_exist()
        {
            var feature = CreateFeature();
            var date = DateTime.UtcNow.AddDays(1);
            var configurer = new FeatureConfigurer(feature)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });
            var store = new RedisStoreBuilder()
               .WithExistingData(new List<FeatureConfigurer>() { configurer })
               .Build();

            feature = await store.FindFeatureAsync(feature.Name);

            feature.Should().NotBeNull();
        }

        [SkipOnAppVeyor]
        public async Task return_null_feature_if_not_exist()
        {
            var store = new RedisStoreBuilder().Build();
            var feature = await store.FindFeatureAsync("non_existing_feature");

            feature.Should().BeNull();
        }

        [SkipOnAppVeyor]
        public async Task return_all_persisted_activators()
        {
            var feature = CreateFeature();
            var date = DateTime.UtcNow.AddDays(1);
            var configurer = new FeatureConfigurer(feature)
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", date);
                });

            var store = new RedisStoreBuilder()
                .WithExistingData(new List<FeatureConfigurer>() { configurer })
                .Build();


            var activators = await store.FindFeatureActivatorsTypesAsync(feature.Name);

            activators.Should().NotBeNull();
            activators.Any().Should().Be(true);
        }

        [SkipOnAppVeyor]
        public async Task return_empty_if_feature_not_exist()
        {
            var feature = CreateFeature();
            var date = DateTime.UtcNow.AddDays(1);
            var configurer = new FeatureConfigurer(feature)
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

                var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");

                var store =  new RedisFeaturesStore(logger, connectionMultiplexer, libraries);

                if (_configurer != null)
                {
                    store.PersistConfiguratioAsync(_configurer).Wait();
                }

                return store;
            }
        }

        Feature CreateFeature()
        {
            return new Feature
            {
                Name = "Test#1",
                Enabled = true,
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}
