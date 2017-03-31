using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;
using Xabaril.Core;

namespace Xabaril.MVC
{
    [HtmlTargetElement("feature",Attributes = FeatureNameAttribute)]
    public class FeatureTagHelper
        :TagHelper
    {
        private IFeaturesService _featuresService;

        private const string FeatureNameAttribute = "name";

        [HtmlAttributeName(FeatureNameAttribute)]
        public string FeatureName { get; set; }


        public FeatureTagHelper(IFeaturesService featuresService)
        {
            _featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));

            _featuresService = featuresService;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync();

            if (!childContent.IsEmptyOrWhiteSpace)
            {
                var isActive = await _featuresService.IsEnabledAsync(FeatureName);

                if (!isActive)
                {
                    output.Content.SetContent(string.Empty);
                }
            }
        }
    }
}
