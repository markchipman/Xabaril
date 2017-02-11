using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Xabaril.Core
{
    public class HttpContextRoleProvider
        : IRoleProvider
    {
        private readonly IHttpContextAccessor _httpContextAccesor;

        public HttpContextRoleProvider(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            _httpContextAccesor = httpContextAccessor;
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
