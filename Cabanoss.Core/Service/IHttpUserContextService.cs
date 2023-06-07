using System.Security.Claims;

namespace Cabanoss.Core.Service
{
    public interface IHttpUserContextService
    {
        ClaimsPrincipal User { get; }
        int? UserId { get; }
        string UserLogin { get; }
    }
}