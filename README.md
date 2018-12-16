
![AppVeyor ](https://ci.appveyor.com/api/projects/status/86ovvsglf9x729d6/branch/master?svg=true)
# Xabaril


**Xabaril** is a [Feature Toggle](https://martinfowler.com/articles/feature-toggles.html) and A/B testing library for ASP.NET Core. Feature Toogle is a powerful technique that allows developers to deliver new functionality to users withouth changing code. Provides an alternative to to mantain multiples branches (aka feature branches), so any feature can be tested even before it is completed and ready for the release. We can release a version of our product with not production ready features. These non production ready features are hidden (toggled) for the broader set of users but can be enabled to any subset of testing or internal users we want them to try the features.We can use feature toogling to enable or disable features during run time.

## Features

A **Feature** is basically a name which allow us to define a particular feature. Each feature can be enabled based on particular activators, which depending on their configuration will tell us the state of it. Check if a feature is enabled or not can be done in several ways, and it will depend on the framework we are using or particular needs of each case.

### Getting Started

1. Install the standard Nuget package into your ASP.NET Core application.

	```
    Install-Package Xabaril
	```

2. Install the InMemoryStore Nuget package into your ASP.NET Core application.

	```
    Install-Package Xabaril.InMemoryStore
	```
2. In the _ConfigureServices_ method of _Startup.cs_, register Xabaril, defining one or more features.

	```csharp
        services.AddXabaril()
        .AddXabarilInMemoryStore(options =>
        {
            options.AddFeature(
                new Feature
                {
                    Name = _featureName,
                    Enabled = true,
                    CreatedOn = DateTime.UtcNow
                })
                .WithActivator<UTCActivator>(p =>
                {
                    foreach (var key in _parameters.Keys)
                    {
                        p.Add(key, _parameters[key]);
                    }
                });
        });
	```
4. In the _Configure_ method, insert middleware to expose the generated Xabaril as JSON endpoint.

	```csharp
	app.UseXabaril("xabaril/features")
	```
5. Now we can make calls to the Xabaril endpoint to check if our feature is active:

	```
	http://server:port/xabaril/features?featureName=MyFeature
	```
6. At this point, you can view the generated JSON

	```json
	{
	   "isEnabled" : true
	}
	```
You can use this approach in JavaScript or Http clients. We can see other ways to consume Xabaril in MVC and use another stores.

### ASP.NET MVC Core

Install the standard Nuget package into your ASP.NET Core application.

```
Install-Package Xabaril.MVC
```

With **ASP.NET MVC Core** we have different options to check if a feature is enabled, for example we can use the **Tag Helper** **feature** inside our Razor views.

```html
 
 <feature name="MyFeature">
        <div>This content is displayed only when feature <b>MyFeature</b> is <span>Active</span></div>
 </feature>

```

If we want to enable or disable the execution of a controller we can do it using the filter **FeatureFilter**. In this case, if the feature *MyFeature* is not enabled a NotFound is returned.

```csharp

    [FeatureFilter(FeatureName ="MyFeature")]
    public IActionResult FiltersActive()
    {
        //if the filter is active this is processed

        return View();
    }

```

If we want to define different behaviors, when the feature is active and when not you can use **FeatureToogle** instead of **FeatureFilter**.

```csharp

    [FeatureToogle(FeatureName ="MyFeature")]
    [ActionName("someaction")]
    public IActionResult SomeActionWhenFeatureIsActive()
    {
        return View("FeatureView");
    }

    [ActionName("someaction")]
    public IActionResult SomeActionWhenFeatureIsNotActive()
    {
        return View("DefaultView");
    }

```

For particular needs and customization of execution blocks we can use the service defined with the **IFeaturesService** interface directly in the code of any of our controllers.

```csharp

    public class HomeController : Controller
    {
        private readonly IFeaturesService _features;

        public HomeController(IFeaturesService features)
        {
            _features = features;
        }

        public async Task<IActionResult> Index()
        {
            if (await _features.IsEnabledAsync("MyFeature"))
            {
                ViewData["Message"] = "My Feature Is Active";
            }
            else
            {
                ViewData["Message"] = "My Feature Is NOT Active";
            } 

            return View();
        }
    }

```

## Activators


An **Activator** is code which defines when a feature is enabled or not. Each feature can use one or multiple activator at the same time. In Xabaril you have many different activators out of the box, and of course you can write your custom activators.

### Activators Rollout 

With *roll-out* activators we can set a value with which we can do a rool-out of the feature. In that case, the particular feature, will be only only enable to a defined  percentage of the users. This roll-out is done based on a hash function defined in [*Jenkins-Partitioner*](https://en.wikipedia.org/wiki/Jenkins_hash_function). For now there is only two *roll-out*: activators 

    1.- RolloutHeaderValueActivator: rolling out features depending the value of specified header.
    2.- RolloutUserNameActivator: rolling out features depending the authenticated user.


### UTCActivator

 With **UTCActivator** you can use any UTC date time to define when a feature is active or not. If the date when the feature is tested is greather than the date specified in *release-date*  parameters the activator is enabled.

```csharp

var configurer = new FeatureConfigurer(
    new Feature
    {
        Name = "test#1",
        Enabled = true,
        CreatedOn = DateTime.UtcNow
    })
    .WithActivator<UTCActivator>(parameters =>
    {
        parameters.Add("release-date", DateTime.UtcNow.AddDays(1));
    });

```

**UTCActivator** also support the parameter *format-date* in order to specify the date format.

### FromToActivator

With **FromToActivator** you can define the dates between the feature is active or not.

```csharp

var configurer = new FeatureConfigurer(
    new Feature
    {
        Name = "test#1",
        Enabled = true,
        CreatedOn = DateTime.UtcNow
    })
    .WithActivator<FromToActivator>(parameters =>
    {
        parameters.Add("release-from-date", DateTime.UtcNow.AddDays(1));
        parameters.Add("release-to-date", DateTime.UtcNow.AddDays(5));
    });

```

**FromToActivator** also support the parameter *format-date* in order to specify the date format.

### UserActivator

With **UserActivator** you can enable any feature for specified authenticated users. 

```csharp

var configurer = new FeatureConfigurer(
    new Feature
    {
        Name = "test#1",
        Enabled = true,
        CreatedOn = DateTime.UtcNow
    })
    .WithActivator<UserActivator>(parameters =>
    {
        parameters.Add("user","user1;user2;user3");
    });

``` 

### RoleActivator

Like **UserActivator** **RoleActivator** define the Role that the user need to have some feature enabled.

```csharp

var configurer = new FeatureConfigurer(
    new Feature
    {
        Name = "test#1",
        Enabled = true,
        CreatedOn = DateTime.UtcNow
    })
    .WithActivator<RoleActivator>(parameters =>
    {
        parameters.Add("role","admin");
    });

``` 

### LocationActivator

With location activator you can specify the IP request countries on some feature is active.

```csharp

var configurer = new FeatureConfigurer(
    new Feature
    {
        Name = "test#1",
        Enabled = true,
        CreatedOn = DateTime.UtcNow
    })
    .WithActivator<LocationActivator>(parameters =>
    {
        parameters.Add("locations","Spain;USA");
    });

```

By default the location of any IP is discovered by any implementation of **IPApiLocationProvider**, Xabaril out-of-box only provide a NO-service but is very easy create this implementation using some free services like IP-API.

```csharp

 public class IPApiLocationProvider
        : IGeoLocationProvider
    {
        static HttpClient _client = new HttpClient() { BaseAddress = new Uri("http://ip-api.com/json/") };

        public async Task<string> FindLocationAsync(string ipAddress)
        {
            var response = await _client.GetAsync(ipAddress);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic result = JObject.Parse(content);

                if (result.status.Equals("success"))
                {
                    return result.country;
                }
            }

            return null;
        }
    }

```

When you have a new implementation for any service in Xabaril you can override using the specified method in **XabarilBuilder**


```csharp

 services.AddXabaril()
        .AddXabarilOptions(options =>
        {
            options.FailureMode = Xabaril.FailureMode.LogAndDisable;
        })
        .AddXabarilInMemoryStore(options =>
        {
            options.AddFeature(
                new Feature
                {
                    Name = "LocationActivator",
                    Enabled = true,
                    CreatedOn = DateTime.UtcNow
                })
                .WithActivator<LocationActivator>(_ => _.Add("locations", "Spain;United States"));

        }).AddGeoLocationProvider<IPApiLocationProvider>();

```


## Stores

At this moment you can use two diferent stores, memory and Redis, to save your features configurations:


### InMemory

Install the InMemoryStore Nuget package into your ASP.NET Core application.

```
Install-Package Xabaril.InMemoryStore
```
*InMemoryFeaturesStore* depend on  ASP.NET Core *IMemoryCache* and save all features configuration in memory.


```csharp

 services.AddXabaril()
        .AddXabarilOptions(options =>
        {
            options.FailureMode = Xabaril.FailureMode.LogAndDisable;
        })
        .AddXabarilInMemoryStore(options =>
        {
            options.AddFeature(
                new Feature
                {
                    Name = "LocationActivator",
                    Enabled = true,
                    CreatedOn = DateTime.UtcNow
                })
                .WithActivator<LocationActivator>(_ => _.Add("locations", "Spain;United States"));

        }).AddGeoLocationProvider<IPApiLocationProvider>();

```

### Redis
Install the InMemoryStore Nuget package into your ASP.NET Core application.

```
Install-Package Xabaril.RedisStore
```
*RedisFeaturesStore*  depend on ServiceStack.Redis packages and save all the features configuration on Redis Server.


```csharp

services.AddXabaril()
        .AddXabarilOptions(options =>
        {
            options.FailureMode = Xabaril.FailureMode.LogAndDisable;
        })
        .AddRedisStore(options =>
        {
            options.RedisHost = "localhost:6379";

            options.AddFeature(
                new Feature
                {
                    Name = "LocationActivator",
                    Enabled = true,
                    CreatedOn = DateTime.UtcNow
                })
                .WithActivator<LocationActivator>(_ => _.Add("locations", "Spain;United States"));

        }).AddGeoLocationProvider<IPApiLocationProvider>();


```
## Contributing

Xabaril wouldn't be possible without the time and effort of its contributors. The team is made up of Unai Zorrilla Castro [@unaizorrilla](https://github.com/unaizorrilla), Luis Ruiz Pav�n [@lurumad](https://github.com/lurumad) and Carlos Landeras [@carloslanderas](https://github.com/carloslanderas).

*Our valued committers are*: Hugo Biarge @hbiarge, Luis Fraile @lfraile.

If you want to contribute to the project and make it better, your help is very welcome. You can contribute with helpful bug reports, features requests and also submitting new features with pull requests.

1. Read and follow the [Don't push your pull requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/)
2. Build.ps1 is working on local and AppVeyor.
3. Follow the code guidelines and conventions.
4. New features are not only code, tests and documentation are also mandatory.