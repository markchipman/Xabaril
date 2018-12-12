using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using Xabaril;
using Xabaril.Core.Activators;
using Xabaril.MVC;
using Xabaril.Store;

namespace UnitTests.MVC
{
    public class ServerFixture
    {
        public HttpClient Client { get; private set; }

        public ServerFixture()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<TestStartup>()
                .UseEnvironment("Testing");

            var server = new TestServer(builder);

            Client = server.CreateClient();
        }
    }

    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .Services
                .AddXabaril()
                .AddXabarilOptions(opt => opt.FailureMode = FailureMode.LogAndDisable)
                .AddXabarilInMemoryStore(opt =>
                {
                    opt.AddFeature(Feature.EnabledFeature("MyFeature"))
                    .WithActivator<UTCActivator>(_ =>
                    {
                        _.Add("release-date", DateTime.UtcNow.AddDays(-1));
                    });

                    opt.AddFeature(Feature.DisabledFeature("NonActiveFeature"))
                    .WithActivator<UTCActivator>(_ =>
                    {
                        _.Add("release-date", DateTime.UtcNow.AddDays(+1));
                    });
                });
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvcWithDefaultRoute();
        }
    }

    public class TestResourceFilterController : Controller
    {
        [FeatureFilter(FeatureName = "MyFeature")]
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

    public class TestConstraintFilterController : Controller
    {
        [ActionName("someaction1")]
        [FeatureToogle(FeatureName = "MyFeature")]
        public IActionResult WithActiveFeature1()
        {
            return Content("ActionWhenFeatureIsActive");
        }


        [ActionName("someaction1")]
        public IActionResult DefaultAction1()
        {
            return Content("DefaultAction");
        }


        [ActionName("someaction2")]
        [FeatureToogle(FeatureName = "NonActiveFeature")]
        public IActionResult WithActiveFeature2()
        {
            return Content("ActionWhenFeatureIsActive");
        }


        [ActionName("someaction2")]
        public IActionResult DefaultAction2()
        {
            return Content("DefaultAction");
        }
    }
}
