using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.Board;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(2)]
    public class BoardsController : ControllerBase
    {
        private IBoardService _boardService;

        public BoardsController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        /// <summary>
        /// Create new board
        /// </summary>
        /// <param name="createBoardDto">Request's payload</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/boards
        /// </remarks>
        [HttpPost("boards")]
        public async Task PostBoard([FromBody] CreateBoardDto createBoardDto)
        {
            await _boardService.CreateBoardAsync(createBoardDto);
        }

        /// <summary>
        /// Renames the board
        /// </summary>
        /// <param name="updateBoard">Request's payload</param>
        /// <param name="boardId">Board id</param>
        /// <remarks>
        /// PUT cabanoss.azurewebsites.net/boards?boardId={id}
        /// </remarks>
        [HttpPut("boards")]
        public async Task UpdateBoardName([FromBody] UpdateBoardDto updateBoard, [FromQuery] int boardId)
        {
            await _boardService.UpdateBoardAsync(boardId, updateBoard);
        }

        /// <summary>
        /// Downloads list of boards
        /// </summary>
        /// <returns>
        /// Returns a list of boards 
        /// </returns>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/boards
        /// </remarks>
        [HttpGet("boards")]
        public async Task<List<ResponseBoardDto>> GetBoards()
        {
            var boards = await _boardService.GetBoardsAsync();
            return boards;
        }

        /// <summary>
        /// downloads a specified board
        /// </summary>
        /// <param name="boardId">Board id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/boards/{id}
        /// </remarks>
        [HttpGet("boards/{boardId}")]
        public async Task<ResponseBoardDto> GetBoard([FromRoute] int boardId)
        {
            var board = await _boardService.GetBoardAsync(boardId);
            return board;
        }

        /// <summary>
        /// Removes tables
        /// </summary>
        /// <param name="boardId">Board id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/boards?boardId={id}
        /// </remarks>
        [HttpDelete("boards")]
        public async Task DeleteBoard([FromQuery] int boardId)
        {
            await _boardService.DeleteBoardAsync(boardId);
        }

        /// <summary>
        /// adds the specified user to the given board
        /// </summary>
        /// <param name="boardId">board id</param>
        /// <param name="userId">user id</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/members/boards/{boardId}?userId={userId}
        /// </remarks>
        [HttpPost("members/boards/{boardId}")]
        public async Task AddBoardUsers([FromRoute] int boardId, [FromQuery] int userId)
        {
            await _boardService.AddUsersAsync(boardId, userId);
        }

        /// <summary>
        /// sets the user role in the given board
        /// </summary>
        /// <param name="boardId">board id</param>
        /// <param name="userId">user id</param>
        /// <param name="userRole">user role [0 - Admin, 1 - User]</param>
        /// <remarks>
        /// PATCH cabanoss.azurewebsites.net/members/boards/{boardId}?userId={userId}
        /// </remarks>
        [HttpPatch("members/boards/{boardId}")]
        public async Task SetUserRole([FromRoute] int boardId, [FromQuery] int userId, [FromBody] int userRole)
        {
            await _boardService.SetUserRole(boardId, userId, userRole);
        }

        /// <summary>
        /// downloads users of a certain board
        /// </summary>
        /// <param name="boardId">board id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/members/boards?boardId={id}
        /// </remarks>
        [HttpGet("members/boards")]
        public async Task<List<ResponseBoardUser>> GetBoardUsers([FromQuery] int boardId)
        {
            var users = await _boardService.GetBoardUsersAsync(boardId);
            return users;
        }


        /// <summary>
        /// removes the specified user to the given board
        /// </summary>
        /// <param name="boardId">board id</param>
        /// <param name="userId">user id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/members/{boardId}?userId={userId}
        /// </remarks>
        [HttpDelete("members/boards/{boardId}")]
        public async Task RemoveBoardUsers([FromRoute] int boardId, [FromQuery] int userId)
        {
            await _boardService.RemoveUserAsync(boardId, userId);
        }
    }
}
