# Xabaril

**Xabaril** is a features toogles and A/B testing library for ASP.NET Core. Feature Toogles is a powerful technique that allows developers to deliver new functionality to users withouth changing code. Provides an alternative to to mantain multiples branches (aka feature branches), such that feature can be tested even before it is completed and ready for the release. We can release a version with of our products with unfinished features. These unfinished features are hidden (toggled) for the users.We can use feature toogling to enable or disable features during run time.

## Features

A **Feature**  es básicamente un nombre que permite definier una funcionalidad concreta. Cada funcionalidad o característica estará activada o no en base a un conjunto de activadores, que dependiendo de cuales hayamos seleccionado nos dirán el estado de la misma. Chequear que una funcionalidad en concreto esté activada o no se puede realizar de diferentes formas, dependiendo de que framework estemos utilizando o de las necesidades de cada caso.

### ASP.NET MVC Core

En **ASP.NET MVC Core** tenemos diferentes opciones para comprobar si nuestras funcionalidades están activas o no, asi por ejemplo podríamos utilizar el **Tag Helper** **feature** dentro de nuestras vistas Razor.

```html
 
 <feature name="MyFeature">
        <div>This content is displayed only when feature <b>MyFeature</b> is <span>Active</span></div>
 </feature>

```

Si queremos habilitar o deshabilitar la ejecución de controladores MVC dependiendo de las características podemos utilizar el filtro **FeatureFilter**.

```csharp

    [FeatureFilter(FeatureName ="MyFeature")]
    public IActionResult FiltersActive()
    {
        //if the filter is active this is processed

        return View();
    }

```

Si necesitamos algo menos concreto y personalizar los bloques de ejecución donde hacer la comprobación  de nuestras características siempre podemos recurrir a utilizar el servicio **IFeaturesService** directamente en el codigo de nuestros controladores.

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

**Activator** is code that define when a feature is active or not. Each feature can use one or multiple activator at the same time. In Xabaril you have many different activators out-of-box, and of course you can write your custom activators.

### Activadores Rollout 

Con los activadores de tipo *roll-out* podemos establecer en base a que valor se puede hacer un roll-out de una característica. De tal forma que la misma solamente este activa para un portectage a definir de los usuarios. Este roll-out se hace en base a una funcion hash no criptográfica basada en  [*Jenkins-Partitioner*](https://en.wikipedia.org/wiki/Jenkins_hash_function). Hasta el momento hay dos activadores de tipo *roll-out*:

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

