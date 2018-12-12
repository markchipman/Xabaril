using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
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
        public async Task redirect_to_not_found_if_feature_is_active()
        {
            var response = await _fixture.Client.GetAsync("TestResourceFilter/WithValidFeature");

            response.IsSuccessStatusCode
                .Should().BeTrue();
        }

        [Fact]
        public async Task redirect_to_not_found_if_feature_not_exist()
        {
            var response = await _fixture.Client.GetAsync("TestResourceFilter/WithNonExistingFeature");

            response.StatusCode
                .Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task redirect_to_not_found_if_feature_is_not_enabled()
        {
            var response = await _fixture.Client.GetAsync("TestResourceFilterC/WithNonActiveFeature");

            response.StatusCode
                .Should().Be(HttpStatusCode.NotFound);
        }
    }

    
}
