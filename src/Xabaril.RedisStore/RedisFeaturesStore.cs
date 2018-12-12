using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xabaril.Store;

namespace Xabaril.RedisStore
{
    public class RedisFeaturesStore
        : IFeaturesStore
    {
        private readonly ILogger<XabarilModule> _logger;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IEnumerable<string> _assemblies;

        public RedisFeaturesStore(ILogger<XabarilModule> logger, ConnectionMultiplexer connectionMultiplexer, IEnumerable<string> assemblies)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _assemblies = assemblies;
        }

        public Task<IEnumerable<Type>> FindFeatureActivatorsTypesAsync(string featureName)
        {
            const string pattern = @"activator:([a-zA-Z0-9\.]+):parameter";

            var activators = new List<Type>();

            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var endpoint = _connectionMultiplexer.GetEndPoints().First();
            var server = _connectionMultiplexer.GetServer(endpoint);
            var keys = server.Keys(pattern: $"xabaril:features:{featureName}:activator:*");

            if (keys.Any())
            {
                foreach (var item in keys)
                {
                    var match = regex.Match(item);
                    if (match.Success)
                    {
                        if (match.Groups.Count == 2)
                        {
                            var activator = match.Groups[1]
                                .Value;

                            var type = FindType(activator);

                            if (type != null)
                            {
                                activators.Add(type);
                            }
                        }
                    }
                }
            }

            return Task.FromResult(activators.Any() ? activators : Enumerable.Empty<Type>());
        }

        public async Task<Feature> FindFeatureAsync(string featureName)
        {
            Feature feature = null;

            var featureKey = $"xabaril:features:{featureName}:*";

            var exist = await _connectionMultiplexer.GetDatabase().KeyExistsAsync(featureKey);

            if (exist)
            {
                feature = new Feature() { Name = featureName };
            }

            return feature;
        }

        public async Task<ActivatorParameter> FindParameterAsync(string name, string featureName, string activatorType)
        {
            ActivatorParameter parameter = null;

            var parametersKey = $"xabaril:features:{featureName}:activator:{activatorType}:parameter:{name}";

            var value = await _connectionMultiplexer.GetDatabase().StringGetAsync(parametersKey);

            if (value.HasValue)
            {
                parameter = new ActivatorParameter()
                {
                    Name = name,
                    ActivatorType = activatorType,
                    FeatureName = featureName,
                    Value = value
                };
            }

            return parameter;
        }

        public async Task<bool> PersistConfiguratioAsync(IEnumerable<FeatureConfigurer> features)
        {

            foreach (var item in features)
            {
                foreach (var activator in item.Configuration)
                {
                    foreach (var keyPair in activator.Value)
                    {
                        await _connectionMultiplexer.GetDatabase().StringSetAsync(
                            $"xabaril:features:{item.FeatureName}:activator:{activator.Key.FullName}:parameter:{keyPair.Key}",
                            keyPair.Value.ToString());
                    }
                }
            }

            return true;
        }

        Type FindType(string typeName)
        {
            foreach (var assembly in _assemblies)
            {
                try
                {
                    var type = Assembly.Load(new AssemblyName(assembly))
                        .GetType(typeName);

                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }
    }
}
