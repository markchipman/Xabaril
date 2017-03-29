# Xabaril

**Xabaril** is a [Feature Toggle](https://martinfowler.com/articles/feature-toggles.html) and A/B testing library for ASP.NET Core. Feature Toogle is a powerful technique that allows developers to deliver new functionality to users withouth changing code. Provides an alternative to to mantain multiples branches (aka feature branches), so any feature can be tested even before it is completed and ready for the release. We can release a version of our product with not production ready features. These non production ready features are hidden (toggled) for the broader set of users but can be enabled to any subset of testing or internal users we want them to try the features.We can use feature toogling to enable or disable features during run time.

## Features

A **Feature** is basically a name which allow us to define a particular feature. Each feature can be enabled based on particular activators, which depending on their configuration will tell us the state of it. Check if a feature is enabled or not can be done in several ways, and it will depend on the framework we are using or particular needs of each case.

### ASP.NET MVC Core

With **ASP.NET MVC Core** we have different options to check if a feature is enabled, for example we can use the **Tag Helper** **feature** inside our Razor views.

```html
 
 <feature name="MyFeature">
        <div>This content is displayed only when feature <b>MyFeature</b> is <span>Active</span></div>
 </feature>

```

If we want to enable or disable the execution of a controller we can do it using the filter **FeatureFilter**.

```csharp

    [FeatureFilter(FeatureName ="MyFeature")]
    public IActionResult FiltersActive()
    {
        //if the filter is active this is processed

        return View();
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

    1.- RolloutHeaderValueActivator: Dónde podemos seleccionar el nombre de la cabecera de cada request sobre la que se usará la función de partición.
    2.- RolloutUserNameActivator: El roll-out se realizará en base a la identididad del usuario autenticado.


### UTCActivator

 With **UTCActivator** you can use any UTC date time to define when a feature is active or not. If the date when the feature is tested is greather than the date specified in *release-date*  parameters the activator is enabled.

```csharp

 var configurer = new FeatureConfigurer("Test#1")
                .WithActivator<UTCActivator>(parameters =>
                {
                    parameters.Add("release-date", DateTime.UtcNow.AddDays(1));
                });


```

**UTCActivator** also support the parameter *format-date* in order to specify the date format.

