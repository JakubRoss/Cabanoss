using Cabanoss.Core.Model.Workspace;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cabanoss.API.Controllers
{
    [Route("workspaces")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceBussinessLogicService;

        public WorkspacesController(IWorkspaceService workspaceBussinessLogicService)
        {
            _workspaceBussinessLogicService = workspaceBussinessLogicService;
        }

        [HttpGet]
        public async Task<WorkspaceDto> GetUserWorkspace()
        {
            var workspaceDto = await _workspaceBussinessLogicService.GetUserWorkspaceAsync();
            return workspaceDto;
        }

        [HttpPut]
        public void Put([FromBody] UpdateWorkspaceDto updateWorkspaceDto)
        {
            _workspaceBussinessLogicService.UpdateWorkspaceAsync(updateWorkspaceDto);
        }
    }
}
