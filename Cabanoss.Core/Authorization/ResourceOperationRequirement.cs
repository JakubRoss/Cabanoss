using Microsoft.AspNetCore.Authorization;

namespace Cabanoss.Core.Authorization
{
    public class ResourceOperationRequirement : IAuthorizationRequirement
    {
        public ResourceOperations resourceOperations { get; }

        public ResourceOperationRequirement(ResourceOperations resourceOperations) 
        {
            this.resourceOperations = resourceOperations;
        }
    }
}
