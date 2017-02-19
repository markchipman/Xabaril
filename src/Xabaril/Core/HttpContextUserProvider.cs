using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Xabaril.Core
{
    public class HttpContextUserProvider
        : IUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccesor;

        public HttpContextUserProvider(IHttpContextAccessor httpContextAccesor)
        {
            _httpContextAccesor = httpContextAccesor;
        }

        public Task<string> GetUserNameAsync()
        {
            var httpContext = _httpContextAccesor.HttpContext;
            var identity = httpContext?.User?.Identities.First();

            if (identity == null || identity.IsAuthenticated == false)
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult<string>(
                identity.Name);
        }
    }
}
