using Microsoft.Extensions.Logging;
using ServiceStack.Redis;
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
        private readonly IRedisClientsManager _clientManager;
        private readonly IEnumerable<string> _assemblies;

        public RedisFeaturesStore(ILogger<XabarilModule> logger, IRedisClientsManager clientManager, IEnumerable<string> assemblies)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _assemblies = assemblies;
        }

        public Task<IEnumerable<Type>> FindFeatureActivatorsTypesAsync(string featureName)
        {
            const string pattern = @"activator:([a-zA-Z0-9\.]+):parameter";

            var activators = new List<Type>();

            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            using (var client = _clientManager.GetClient())
            {
                var keys = client.GetKeysByPattern($"xabaril:features:{featureName}:activator:*");

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
        }

        public Task<Feature> FindFeatureAsync(string featureName)
        {
            Feature feature = null;

            using (var client = _clientManager.GetClient())
            {
                var featureKey = $"xabaril:features:{featureName}:*";

                var exist = client.ContainsKey(featureKey);

                if (exist)
                {
                    feature = new Feature() { Name = featureName };
                }
            }

            return Task.FromResult(feature);
        }

        public Task<ActivatorParameter> FindParameterAsync(string name, string featureName, string activatorType)
        {

            ActivatorParameter parameter = null;

            using (var client = _clientManager.GetReadOnlyClient())
            {
                var parametersKey = $"xabaril:features:{featureName}:activator:{activatorType}:parameter:{name}";

                var value = client.Get<string>(parametersKey);

                if (value != null)
                {
                    parameter = new ActivatorParameter()
                    {
                        Name = name,
                        ActivatorType = activatorType,
                        FeatureName = featureName,
                        Value = value
                    };
                }
            }

            return Task.FromResult(parameter);
        }

        public Task<bool> PersistConfiguratioAsync(IEnumerable<FeatureConfigurer> features)
        {
            using (var client = _clientManager.GetClient())
            {
                foreach (var item in features)
                {
                    foreach (var activator in item.Configuration)
                    {
                        foreach (var keyPair in activator.Value)
                        {
                            client.Set($"xabaril:features:{item.FeatureName}:activator:{activator.Key.FullName}:parameter:{keyPair.Key}", keyPair.Value.ToString());
                        }

                    }
                }
            }

            return Task.FromResult(true);
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
