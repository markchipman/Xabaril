using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xabaril;
using Xabaril.Core.Activators;
using Xabaril.MVC;
using Xunit;

namespace UnitTests.MVC
{
    public class feature_filter_should
        :IClassFixture<ServerFixture>
    {
        private readonly ServerFixture _fixture;

        public feature_filter_should(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task redirect_to_not_found_if_feature_is_activet()
        {
            var response = await _fixture.Client.GetAsync("TestFilter/WithValidFeature");

            response.IsSuccessStatusCode
                .Should().BeTrue();
        }

        [Fact]
        public async Task redirect_to_not_found_if_feature_not_exist()
        {
            var response = await _fixture.Client.GetAsync("TestFilter/WithNonExistingFeature");

            response.StatusCode
                .Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task redirect_to_not_found_if_feature_is_not_enabled()
        {
            var response = await _fixture.Client.GetAsync("TestFilter/WithNonActiveFeature");

            response.StatusCode
                .Should().Be(HttpStatusCode.NotFound);
        }
    }

    public class ServerFixture
    {
        public HttpClient Client { get; private set; }

        public ServerFixture()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<TestStartup>()
                .UseIISIntegration()
                .UseEnvironment("Testing");

            var server = new TestServer(builder);

            Client = server.CreateClient();
        }

       
    }


    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddXabaril()
                .AddXabarilOptions(opt => opt.FailureMode = FailureMode.LogAndDisable)
                .AddXabarilInMemoryStore(opt =>
                {
                    opt.AddFeature("MyFeature")
                    .WithActivator<UTCActivator>(_ =>
                    {
                        _.Add("release-date", DateTime.UtcNow.AddDays(-1));
                    });

                    opt.AddFeature("NonActiveFeature")
                    .WithActivator<UTCActivator>(_ =>
                    {
                        _.Add("release-date", DateTime.UtcNow.AddDays(+1));
                    });
                });

            services.AddMvc();
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }

    public class TestFilterController : Controller
    {
        [FeatureFilter(FeatureName ="MyFeature")]
        public IActionResult WithValidFeature()
        {
            return Content("Active");
        }

        [FeatureFilter(FeatureName = "NonExistingFeature")]
        public IActionResult WithNonExistingFeature()
        {
            return Content("NonExistingFeature");
        }

        [FeatureFilter(FeatureName = "NonActiveFeature")]
        public IActionResult WithNonActiveFeature()
        {
            return Content("NonActive");
        }
    }
}
