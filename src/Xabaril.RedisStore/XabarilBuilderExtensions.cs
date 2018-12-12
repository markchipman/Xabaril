using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using Xabaril.Store;

namespace Xabaril.RedisStore
{
    public static class XabarilBuilderExtensions
    {
        public static IXabarilBuilder AddRedisStore(this IXabarilBuilder builder)
        {
            return AddRedisStore(builder, options => { });
        }

        public static IXabarilBuilder AddRedisStore(this IXabarilBuilder builder, Action<RedisOptions> configurer)
        {
            var options = new RedisOptions();
            var setup = configurer ?? (opts => { });

            setup(options);

            builder.Services.AddSingleton(ConnectionMultiplexer.Connect(options.RedisHost));
            builder.Services.AddSingleton<IFeaturesStore, RedisFeaturesStore>(provider =>
            {
                var logger = provider.GetService<ILogger<XabarilModule>>();
                var redisClientManager = provider.GetService<ConnectionMultiplexer>();
                var libraries = DependencyContext
                    .Default
                    .CompileLibraries
                    .Select(c=>c.Name);

                var store =  new RedisFeaturesStore(logger,redisClientManager,libraries);

                if (options.FeatureConfiguration != null)
                {
                    store.PersistConfiguratioAsync(options.FeatureConfiguration).Wait();
                }

                return store;
            });

            return builder;
        }
    }
}
