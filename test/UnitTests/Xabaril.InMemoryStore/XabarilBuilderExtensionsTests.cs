using FluentAssertions;
using global::Xabaril;
using global::Xabaril.InMemoryStore;
using global::Xabaril.Store;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.Xabaril.InMemoryStore
{
    public class xabaril_builder_extension_should
    {
        [Fact]
        public void configure_required_services()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            IXabarilBuilder xabarilBuilder = new XabarilBuilder(serviceCollection);

            xabarilBuilder = xabarilBuilder.AddXabarilInMemoryStore();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider
             .GetRequiredService<IFeaturesStore>()
             .Should().NotBeNull();

            serviceProvider
                .GetRequiredService<IFeaturesStore>()
                .Should().BeOfType<InMemoryFeaturesStore>();
        }
    }
}
