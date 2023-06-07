using Cabanoss.Core.Model.Workspace;

namespace Cabanoss.Core.Service
{
    public interface IWorkspaceService
    {
        Task<WorkspaceDto> GetUserWorkspaceAsync();
        Task<WorkspaceDto> UpdateWorkspaceAsync(UpdateWorkspaceDto updateWorkspaceDto);
        Task AddWorkspaceAsync();
    }
}