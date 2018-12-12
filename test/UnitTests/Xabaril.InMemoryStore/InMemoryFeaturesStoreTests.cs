using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xabaril;
using Xabaril.Core.Activators;
using Xabaril.InMemoryStore;
using Xabaril.Store;
using Xunit;

namespace UnitTests.Xabaril.InMemoryStore
{
    public class inmemory_feature_store_should
    {
        [Fact]
        public async Task find_feature_if_exist()
        {
            var feature = CreateFeature();
            var store = new FeatureStoreBuilder()
                .WithFeatureConfigurer(new FeatureConfigurer(feature)
                    .WithActivator<UserActivator>(@params =>
                    {
                        @params.Add("USER", "uzorrilla");
                    })).Build();

            (await store.FindFeatureAsync(feature.Name)).Name.Should().Be(feature.Name);
        }

        [Fact]
        public async Task return_null_feature_if_not_exist()
        {
            var store = new FeatureStoreBuilder()
                .Build();

            var feature = await store.FindFeatureAsync("non_existing_feature");

            (await store.FindFeatureAsync("non_existing_feature")).Should().BeNull();
        }

        [Fact]
        public async Task find_parameter_if_exist()
        {
            const string Key = "USER";
            const string Value = "uzorrilla";
            var feature = CreateFeature();
            var store = new FeatureStoreBuilder()
                .WithFeatureConfigurer(new FeatureConfigurer(feature)
                    .WithActivator<UserActivator>(@params =>
                    {
                        @params.Add(Key, Value);
                    })).Build();

            var parameter = await store.FindParameterAsync(Key, feature.Name, typeof(UserActivator).Name);
                
            parameter.Should().NotBeNull();
            parameter.Name.Should().Be(Key);
            parameter.FeatureName.Should().Be(feature.Name);
            parameter.ActivatorType.Should().Be(typeof(UserActivator).Name);
            parameter.Value.Should().Be(Value);
        }

        [Fact]
        public async Task return_null_if_use_fullname_type_value()
        {
            var feature = CreateFeature();
            var store = new FeatureStoreBuilder()
                .WithFeatureConfigurer(new FeatureConfigurer(feature)
                    .WithActivator<UserActivator>(@params =>
                    {
                        @params.Add("USER", "uzorrilla");
                    })).Build();

            var parameter = await store.FindParameterAsync("USER", feature.Name, typeof(UserActivator).FullName);
            
            parameter.Should().BeNull();
        }

        [Fact]
        public async Task return_null_parameter_if_not_exist()
        {
            var feature = CreateFeature();
            var store = new FeatureStoreBuilder()
                .WithFeatureConfigurer(new FeatureConfigurer(CreateFeature())
                    .WithActivator<UserActivator>(@params =>
                    {
                        @params.Add("USER", "uzorrilla");
                    })).Build();

            var parameter = await store.FindParameterAsync("non_existing_parameter", feature.Name, typeof(UserActivator).Name);

            parameter.Should().BeNull();
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

        private class FeatureStoreBuilder
        {
            private FeatureConfigurer _configurer;

            public IFeaturesStore Build()
            {
                var loggerFactory = new LoggerFactory();
                var logger = loggerFactory.CreateLogger<XabarilModule>();

                var store = new InMemoryFeaturesStore(logger,
                    new MemoryCache(new MemoryCacheOptions()));

                if (_configurer != null)
                {
                    store.PersistConfiguratioAsync(new List<FeatureConfigurer>() { _configurer });
                }

                return store;
            }

            public FeatureStoreBuilder WithFeatureConfigurer(FeatureConfigurer configurer)
            {
                _configurer = configurer;

                return this;
            }

        }
    }
}
