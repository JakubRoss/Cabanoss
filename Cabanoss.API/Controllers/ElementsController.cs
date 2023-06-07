using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.Element;
using Cabanoss.Core.Model.User;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(8)]
    public class ElementsController : ControllerBase
    {
        private IElementService _elementService;

        public ElementsController(IElementService elementService)
        {
            _elementService = elementService;
        }

        /// <summary>
        /// adds a new element to the task
        /// </summary>
        /// <param name="elementDto">Request's payload</param>
        /// <param name="taskId">task id</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/elements/tasks?taskId={id}
        /// </remarks>
        [HttpPost("elements/tasks")]
        public async Task AddElement([FromQuery] int taskId, [FromBody] ElementDto elementDto)
        {
            await _elementService.AddElement(taskId, elementDto);
        }

        /// <summary>
        /// updates a given task element 
        /// </summary>
        /// <param name="updateElementDto">Request's payload</param>
        /// <param name="elementId">element id</param>
        /// <remarks>
        /// PUT cabanoss.azurewebsites.net/elements?elementId={id}
        /// </remarks>
        [HttpPut("elements")]
        public async Task UpdateElement([FromQuery] int elementId, [FromBody] UpdateElementDto updateElementDto)
        {
            await _elementService.UpdateElement(elementId, updateElementDto);
        }

        /// <summary>
        /// downloads the elements of a given task 
        /// </summary>
        /// <param name="taskId">task id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/elements?taskId={id}
        /// </remarks>
        [HttpGet("elements/tasks")]
        public async Task<List<ResponseElementDto>> GetElements([FromQuery] int taskId)
        {
            var elements = await _elementService.GetElements(taskId);
            return elements;
        }

        /// <summary>
        /// downloads the indicated task element
        /// </summary>
        /// <param name="elementId">element id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/elements?elementId={id}
        /// </remarks>
        [HttpGet("elements")]
        public async Task<ResponseElementDto> GetElement([FromQuery] int elementId)
        {
            var element = await _elementService.GetElement(elementId);
            return element;
        }

        /// <summary>
        /// removes a given task element
        /// </summary>
        /// <param name="elementId">element id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/elements?elementId={id}
        /// </remarks>
        [HttpDelete("elements")]
        public async Task DeleteElement([FromQuery] int elementId)
        {
            await _elementService.DeleteElement(elementId);
        }

        /// <summary>
        /// adds a user to a specific task element
        /// </summary>
        /// <param name="elementId">element id</param>
        /// <param name="userId">user id</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/members/elements/{elementId}?userId={id}
        /// </remarks>
        [HttpPost("members/elements/{elementId}")]
        public async Task AddUserToElement([FromRoute] int elementId, [FromQuery] int userId)
        {
            await _elementService.AddUserToElement(elementId, userId);
        }

        /// <summary>
        /// checks the "checkbox" (true or false)
        /// </summary>
        /// <param name="elementId">element id</param>
        /// <remarks>
        /// PATCH cabanoss.azurewebsites.net/elements?elementId={id}
        /// </remarks>
        [HttpPatch("elements")]
        public async Task CheckElement([FromQuery] int elementId, ElementCheckDto updateElement)
        {
            await _elementService.CheckElement(elementId, updateElement);
        }
        //
        /// <summary>
        /// retrieves users assigned to a specific task element
        /// </summary>
        /// <param name="elementId">element id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/members/elements?elementId={id}
        /// </remarks>
        [HttpGet("members/elements")]
        public async Task<List<ResponseUserDto>> GetElementUsers([FromQuery] int elementId)
        {
            return await _elementService.GetElementUsers(elementId);
        }

        /// <summary>
        /// removes the user to the specified task element
        /// </summary>
        /// <param name="elementId">element id</param>
        /// <param name="userId">user id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/members/elements/{elementId}?userId={id}
        /// </remarks>
        [HttpDelete("members/elements/{elementId}")]
        public async Task DeleteUserFromElement([FromRoute] int elementId, [FromQuery] int userId)
        {
            await _elementService.DeleteUserFromElement(elementId, userId);
        }
    }
}
