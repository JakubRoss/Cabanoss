using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Cabanoss.Core.Service.Impl
{
    public class HttpUserContextService : IHttpUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpUserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

        public int? UserId => User is null ? null : int.Parse(User.FindFirst(t => t.Type == ClaimTypes.NameIdentifier).Value);
        public string UserLogin => User?.FindFirst(t => t.Type == ClaimTypes.Name)?.Value ?? string.Empty;
    }
}
