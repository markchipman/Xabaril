using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Xabaril.Core;
using Xabaril.MVC;
using Xabaril.Store;
using Xunit;

namespace UnitTests.MVC
{
    public class feature_tag_helper_should
    {
        [Fact]
        public async Task clean_content_when_feature_is_not_active()
        {
            var feature = Feature.DisabledFeature("Test#1");
            var featureService = new FeatureServiceBuilder()
                .WithInMemoryStore()
                .WithDisabledFeature(feature)
                .Build();
            var tagHelper = new FeatureTagHelper(featureService)
            {
                FeatureName = feature.Name
            };
            var context = GetTagHelperContext();
            var output = GetTagHelperOutput(tag: "feature", innerContent: "<p>some content</p>");

            await tagHelper.ProcessAsync(context, output);

            output.Content.IsEmptyOrWhiteSpace
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task preserve_content_when_feature_is_active()
        {
            var feature = Feature.EnabledFeature("Test#1");
            var featureService = new FeatureServiceBuilder()
                .WithInMemoryStore()
                .WithEnabledFeature(feature)
                .Build();
            var tagHelper = new FeatureTagHelper(featureService)
            {
                FeatureName = feature.Name
            };
            var context = GetTagHelperContext();
            var output = GetTagHelperOutput(tag: "feature", innerContent: "<p>some content</p>");

            await tagHelper.ProcessAsync(context, output);

            output.Content.IsEmptyOrWhiteSpace
                .Should()
                .BeFalse();
        }


        TagHelperContext GetTagHelperContext()
        {
            return new TagHelperContext(
                new TagHelperAttributeList(Enumerable.Empty<TagHelperAttribute>()),
                new Dictionary<object, object>(),
                "test");
        }

        TagHelperOutput GetTagHelperOutput(string tag, string innerContent)
        {
            var output =   new TagHelperOutput(tag,
               new TagHelperAttributeList(),
               (useCachedResult_,encoder) =>
               {
                   var tagHelperContent = new DefaultTagHelperContent();

                   tagHelperContent.SetContent(innerContent);

                   return Task.FromResult<TagHelperContent>(tagHelperContent);
               });

            output.PreContent.SetContent("precontent");
            output.Content.SetContent(innerContent);
            output.PostContent.SetContent("postcontent");

            return output;
        }
    }
}
