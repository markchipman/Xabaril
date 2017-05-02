using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xabaril.Core.Activators;

namespace JavascriptMiddlewareConsumption
{
    public class Startup
    {
        
        public void ConfigureServices(IServiceCollection services)
        {           
            
            services.AddXabaril()
                    .AddXabarilOptions(options =>
            {
                options.FailureMode = Xabaril.FailureMode.LogAndDisable;
            })
                .AddXabarilInMemoryStore(opt =>
                {
                    opt.AddFeature("UtcFeature#1")
                    .WithActivator<UTCActivator>(parameters =>
                    {
                        parameters.Add("release-date", DateTime.UtcNow.AddDays(-1));
                    });

                    opt.AddFeature("FromToFeature#1").WithActivator<FromToActivator>(parameters =>
                   {
                       parameters.Add("release-from-date", DateTime.UtcNow.AddDays(-1));
                       parameters.Add("release-to-date", DateTime.UtcNow.AddDays(5));
                   });

                   opt.AddFeature("UserAuthenticatedFeature#1").WithActivator<UserActivator>(parameters =>
                   {
                       parameters.Add("user","user1;user2;user3");
                   });
                });


        }

       
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseXabaril("xabaril");
        }
    }
}
