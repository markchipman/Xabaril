using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace UnitTests.MVC
{
    public class feature_toogle_should
        : IClassFixture<ServerFixture>
    {
        private readonly ServerFixture _fixture;

        public feature_toogle_should(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task use_specific_action_when_feature_is_active()
        {
            var response = await _fixture.Client.GetAsync("TestConstraintFilter/someaction1");

            response.IsSuccessStatusCode
                .Should().BeTrue();

            (await response.Content.ReadAsStringAsync())
                .Should().Contain("ActionWhenFeatureIsActive");
        }

        [Fact]
        public async Task use_default_action_when_feature_is_active()
        {
            var response = await _fixture.Client.GetAsync("TestConstraintFilter/someaction2");

            response.IsSuccessStatusCode
                .Should().BeTrue();

            (await response.Content.ReadAsStringAsync())
                .Should().Contain("DefaultAction");
        }
    }
}
