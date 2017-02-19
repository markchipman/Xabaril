using FluentAssertions;
using global::Xabaril.Core;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Xabaril.Core
{
    public class http_context_user_provider_should
    {
        [Fact]
        public async Task get_null_if_user_is_not_authenticated()
        {
            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();

            var provider = new HttpContextUserProvider(httpContextAccessor);

            (await provider.GetUserNameAsync()).Should().BeNull();
        }

        [Fact]
        public async Task get_the_claim_name_of_authenticated_user_with_default_name_claim()
        {
            var username = "some_user";

            var context = new DefaultHttpContext();
            context.User = GetAuthenticatedIdentityWithDefaultName(username);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextUserProvider(httpContextAccessor);

            (await provider.GetUserNameAsync()).Should().Be(username);
        }

        [Fact]
        public async Task get_the_claim_name_of_authenticated_user_with_custom_name_claim()
        {
            var username = "some_user";

            var context = new DefaultHttpContext();
            context.User = GetAuthenticatedIdentityWithDefaultName(username);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextUserProvider(httpContextAccessor);

            (await provider.GetUserNameAsync()).Should().Be(username);
        }

        [Fact]
        public async Task get_null_for_non_authenticated_user_with_default_name_claim()
        {
            var username = "some_user";

            var context = new DefaultHttpContext();
            context.User = GetNonAuthenticatedIdentityWithDefaultName(username);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextUserProvider(httpContextAccessor);

            (await provider.GetUserNameAsync()).Should().BeNull();
        }

        private ClaimsPrincipal GetAuthenticatedIdentityWithDefaultName(string user)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,user)
                },
                authenticationType: "custom"));
        }

        private ClaimsPrincipal GetNonAuthenticatedIdentityWithDefaultName(string user)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,user)
                }));
        }

        private ClaimsPrincipal GetAuthenticatedIdentityWithCustomName(string user)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim("name",user)
                },
                authenticationType: "custom",
                nameType: "name",
                roleType: "role"));
        }
    }
}
