using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Xabaril.Core
{
    public class HttpContextRoleProvider
        : IRoleProvider
    {
        private readonly IHttpContextAccessor _httpContextAccesor;

        public HttpContextRoleProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccesor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task<string> GetRoleAsync()
        {
            var httpContext = _httpContextAccesor.HttpContext;
            var identity = httpContext?.User?.Identities.First();

            if (identity == null || identity.IsAuthenticated == false)
            {
                return Task.FromResult<string>(null);
            }

            var role = identity.Claims
                    .FirstOrDefault(c => c.Type == identity.RoleClaimType)?.Value;

            return Task.FromResult<string>(role);
        }
    }
}
