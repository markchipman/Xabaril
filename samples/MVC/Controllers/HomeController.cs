using Microsoft.AspNetCore.Mvc;
using Xabaril.MVC;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //the view contains feature tag-helper to enable or not content
            return View();
        }

        [FeatureFilterAttribute(FeatureName ="MyFeature")]
        public IActionResult FiltersActive()
        {
            //if the filter is active this is processed

            return View();
        }

        [FeatureFilterAttribute(FeatureName = "NonExistingFeature")]
        public IActionResult FiltersNonActive()
        {
            //the feature is not active,  404 is returned

            return View();
        }
    }
}
