using AutoMapper;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.Workspace;
using Cabanoss.Core.Repositories;

namespace Cabanoss.Core.Service.Impl
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceBaserepository;
        private readonly IMapper _mapper;
        private readonly IHttpUserContextService _httpUserContextService;

        public WorkspaceService(
            IWorkspaceRepository workspaceBaserepository,
            IMapper mapper,
            IHttpUserContextService httpUserContextService)
        {
            _workspaceBaserepository = workspaceBaserepository;
            _mapper = mapper;
            _httpUserContextService = httpUserContextService;
        }
        #region Utils
        private async Task<Workspace> GetWorkspaceAsync()
        {
            var workspace = await _workspaceBaserepository.GetFirstAsync(id => id.UserId == (int)_httpUserContextService.UserId);
            if (workspace == null)
                throw new ResourceNotFoundException("Uzytkownik nie posiada przestrzeni roboczej");
            return workspace;
        }
        #endregion

        public async Task<WorkspaceDto> GetUserWorkspaceAsync()
        {
            var workspace = await GetWorkspaceAsync();
            var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);
            return workspaceDto;
        }
        public async Task<WorkspaceDto> UpdateWorkspaceAsync(UpdateWorkspaceDto updateWorkspaceDto)
        {
            var workspace = await GetWorkspaceAsync();
            workspace.Name = updateWorkspaceDto.Name;
            workspace.UpdatedAt = DateTime.Now;
            var updatedWorkspace = await _workspaceBaserepository.UpdateAsync(workspace);
            var updatedWorkspaceDto = _mapper.Map<WorkspaceDto>(updatedWorkspace);
            return updatedWorkspaceDto;
        }

        #region nieuzywane
        public async Task AddWorkspaceAsync(User user)
        {
            var workspaceDto = new WorkspaceDto
            {
                Name = $"{user.Login} Workspace",
                CreatedAt = DateTime.Now,
                UserId = user.Id,
            };
            var workspace = _mapper.Map<Workspace>(workspaceDto);
            await _workspaceBaserepository.AddAsync(workspace);
        }
        #endregion
    }
}
