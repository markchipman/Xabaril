using FluentAssertions;
using global::Xabaril.Core;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Xabaril.Core
{
    public class http_context_role_provider_should
    {
        [Fact]
        public async Task get_null_when_user_is_not_authenticated()
        {
            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();

            var provider = new HttpContextRoleProvider(httpContextAccessor);

            (await provider.GetRoleAsync()).Should().BeNull();
        }

        [Fact]
        public async Task get_null_when_claim_role_not_exist()
        {
            var context = new DefaultHttpContext();
            context.User = GetAuthenticatedIdentityWithoutRole();

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextRoleProvider(httpContextAccessor);

            (await provider.GetRoleAsync()).Should().BeNull();
        }

        [Fact]
        public async Task get_the_default_claim_role_value_of_authenticated_user()
        {
            string currentRole = "stuff";

            var context = new DefaultHttpContext();
            context.User = GetAuthenticatedIdentityWithDefaultRoleClaim(role: currentRole);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextRoleProvider(httpContextAccessor);

            (await provider.GetRoleAsync()).Should().Be(currentRole);
        }

        [Fact]
        public async Task get_null_for_non_authenticated_user_with_default_claim_role()
        {
            string currentRole = "stuff";

            var context = new DefaultHttpContext();
            context.User = GetNonAuthenticatedIdentityWithDefaultRoleClaim(role: currentRole);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextRoleProvider(httpContextAccessor);

            (await provider.GetRoleAsync()).Should().BeNull();
        }

        [Fact]
        public async Task get_the_custom_claim_role_value_of_authenticated_user()
        {
            string currentRole = "stuff";

            var context = new DefaultHttpContext();
            context.User = GetAuthenticatedIdentityWithCustomRoleClaim(role: currentRole);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = context;

            var provider = new HttpContextRoleProvider(httpContextAccessor);

            (await provider.GetRoleAsync()).Should().Be(currentRole);
        }

        private ClaimsPrincipal GetAuthenticatedIdentityWithDefaultRoleClaim(string role)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,"some_user"),
                    new Claim(ClaimTypes.Role,role),
                },
                authenticationType: "custom"));
        }

        private ClaimsPrincipal GetNonAuthenticatedIdentityWithDefaultRoleClaim(string role)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,"some_user"),
                    new Claim(ClaimTypes.Role,role),
                }));
        }

        private ClaimsPrincipal GetAuthenticatedIdentityWithCustomRoleClaim(string role)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,"some_user"),
                    new Claim("role",role),
                },
                authenticationType: "custom",
                nameType: "name",
                roleType: "role"));
        }

        private ClaimsPrincipal GetAuthenticatedIdentityWithoutRole()
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,"some_user"),
                },
                authenticationType: "custom"));
        }
    }
}
