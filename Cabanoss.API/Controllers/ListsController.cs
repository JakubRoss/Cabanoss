using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.List;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [Route("lists")]
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(3)]
    public class ListsController : ControllerBase
    {
        private IListService _listService;

        public ListsController(IListService listService)
        {
            _listService = listService;
        }

        /// <summary>
        /// Creating list for a given board
        /// </summary>
        /// <param name="boardId">board Id</param>
        /// <param name="createList">Request's payload</param>
        /// <remarks>POST cabanoss.azurewebsites.net/lists?boardId={id}
        /// </remarks>
        [HttpPost]
        public async Task CreateList([FromQuery] int boardId, [FromBody] CreateListDto createList)
        {
            await _listService.CreateListAsync(boardId, createList.Name);
        }

        /// <summary>
        /// Retrieves all lists for a given board
        /// </summary>
        /// <param name="boardId">board Id</param>
        /// <returns>List of board task lists</returns>
        /// <remarks>GET cabanoss.azurewebsites.net/lists/boards?boardId={id}
        /// </remarks>
        [HttpGet("boards")]
        public async Task<List<ListDto>> GetLists([FromQuery] int boardId)
        {
            var lists = await _listService.GetAllAsync(boardId);
            return lists;
        }

        /// <summary>
        /// Retrieves a given list
        /// </summary>
        /// <param name="listId">list Id</param>
        /// <returns>task list</returns>
        /// <remarks>GET cabanoss.azurewebsites.net/lists?listId={id}
        /// </remarks>
        [HttpGet]
        public async Task<ListDto> GetList([FromQuery]int listId)
        {
            return await _listService.GetListAsync(listId);
        }

        /// <summary>
        /// Updating a given list
        /// </summary>
        /// <param name="listId">list Id</param>
        /// <param name="createList">Request's payload</param>
        /// <remarks>PUT cabanoss.azurewebsites.net/lists?listId={id}
        /// </remarks>
        [HttpPut]
        public async Task UpdateList([FromQuery] int listId , [FromBody] CreateListDto createList)
        {
            await _listService.UpdateList(listId, createList.Name);
        }

        /// <summary>
        /// Seting task list deadline
        /// </summary>
        /// <param name="listId">list Id</param>
        /// <param name="date">deadline time</param>
        /// <remarks>PATCH cabanoss.azurewebsites.net/lists?listId={id}
        /// </remarks>
        [HttpPatch]
        public async Task SetDeadline([FromQuery] int listId, [FromBody] DateOnly date)
        {
            await _listService.SetDeadline(listId , date );
        }

        /// <summary>
        /// Deleting a given list
        /// </summary>
        /// <param name="listId">list Id</param>
        /// <remarks>DELETE cabanoss.azurewebsites.net/lists?listId={id}
        /// </remarks>
        [HttpDelete]
        public async Task DeleteList([FromQuery] int listId)
        {
            await _listService.DeleteList(listId );
        }
    }
}
