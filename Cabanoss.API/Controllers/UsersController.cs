using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.User;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [Route("users")]
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(1)]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// updates account details
        /// </summary>
        /// <param name="user">Request's payload</param>
        /// <remarks>
        /// PUT cabanoss.azurewebsites.net/users
        /// </remarks>
        [HttpPut]
        public async System.Threading.Tasks.Task<UserDto> PutUser([FromBody] UpdateUserDto user)
        {
            var updatedUser = await _userService.UpdateUserAsync(user);
            return updatedUser;
        }

        /// <summary>
        /// downloads account details
        /// </summary>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/users
        /// </remarks>
        [HttpGet]
        public async Task<UserDto> GetUser()
        {
            var userDto =await _userService.GetUserAsync();
            return userDto;
        }

        /// <summary>
        /// downloads all users of the application
        /// </summary>
        /// /// <param name="searchingPhrase">phrase search in login or email if null returns all users</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/users/all?searchingPhrase={phrase}
        /// </remarks>
        [HttpGet("all")]
        public async Task<List<ResponseUserDto>> GetUsersAsync([FromQuery]string? searchingPhrase)
        {
            return await _userService.GetUsersAsync(searchingPhrase);
        }

        /// <summary>
        /// deletes account
        /// </summary>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/users
        /// </remarks>
        [HttpDelete]
        public async System.Threading.Tasks.Task DeleteUser()
        {
            await _userService.RemoveUserAsync();
        }
    }
}
