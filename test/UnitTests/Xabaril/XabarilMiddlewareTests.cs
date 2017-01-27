using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xabaril.Core.Activators;
using Xunit;

namespace UnitTests.Xabaril
{
    public class xabaril_middleware_should
    {
        private const string FeatureName = "test";
        private const string EndPointPath = "/xabaril/features";

        [Fact]
        public async Task returns_not_found_http_status_code_if_the_feature_does_not_exists()
        {
            var server = new TestServerBuilder()
                .WithEndPoint(EndPointPath)
                .WithFeature(FeatureName)
                .WithFeatureParameters(
                    new Dictionary<string, object> {
                        {
                            "release-date", DateTime.UtcNow.AddDays(-1)
                        }
                    })
                .Build();

            var response = await server.CreateClient().GetAsync($"{EndPointPath}?featureName=bad-feature");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task returns_is_active__if_the_feature_exists_and_is_active()
        {
            var server = new TestServerBuilder()
                .WithEndPoint(EndPointPath)
                .WithFeature(FeatureName)
                .WithFeatureParameters(
                    new Dictionary<string, object> {
                        {
                            "release-date", DateTime.UtcNow.AddDays(-1)
                        }
                    })
                .Build();

            var response = await server.CreateClient().GetAsync($"{EndPointPath}?featureName={FeatureName}");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Result>(json);

            data.IsEnabled.Should().Be(true);
        }
        [Fact]
        public async Task returns_is_active__if_the_feature_exists_and_is_inactive()
        {
            var server = new TestServerBuilder()
                .WithEndPoint(EndPointPath)
                .WithFeature(FeatureName)
                .WithFeatureParameters(
                    new Dictionary<string, object> {
                        {
                            "release-date", DateTime.UtcNow.AddDays(+1)
                        }
                    })
                .Build();

            var response = await server.CreateClient().GetAsync($"{EndPointPath}?featureName={FeatureName}");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Result>(json);

            data.IsEnabled.Should().Be(false);
        }

        internal class TestServerBuilder
        {
            private string _endPointPath;
            private string _featureName;
            private Dictionary<string, object> _parameters;

            public TestServer Build()
            {
                var builder = new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddXabaril()
                            .AddXabarilInMemoryStore(options =>
                            {
                                options.AddFeature(_featureName)
                                    .WithActivator<UTCActivator>(p =>
                                    {
                                        foreach (var key in _parameters.Keys)
                                        {
                                            p.Add(key, _parameters[key]);
                                        }
                                    });
                            });
                    })
                    .Configure(app => app.UseXabaril(_endPointPath));

                return new TestServer(builder);
            }

            public TestServerBuilder WithEndPoint(string endPointPath)
            {
                _endPointPath = endPointPath;

                return this;
            }

            public TestServerBuilder WithFeature(string featureName)
            {
                _featureName = featureName;

                return this;
            }

            public TestServerBuilder WithFeatureParameters(Dictionary<string, object> parameters)
            {
                _parameters = parameters;

                return this;
            }
        }

        class Result
        {
            public bool IsEnabled { get; set; }
        }
    }
}
