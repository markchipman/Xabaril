using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xabaril;
using Xabaril.RedisStore;
using Xabaril.Store;
using Xunit;

namespace UnitTests.Xabaril.RedisStore
{
    public class redistore_should
    {

        [Fact]
        public void configure_required_services()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            var builder = new XabarilBuilder(serviceCollection);

            builder.AddRedisStore();

            var serviceProvider = builder.Services.BuildServiceProvider();

            serviceProvider
                .GetRequiredService<IFeaturesStore>()
                .Should().NotBeNull();

            serviceProvider
                .GetRequiredService<IFeaturesStore>()
                .Should().BeOfType<RedisFeaturesStore>(); 
        }
    }
}
