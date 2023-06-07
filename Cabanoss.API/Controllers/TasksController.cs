using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.Task;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [Route("tasks")]
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(7)]
    public class TasksController : ControllerBase
    {
        private ITasksService _tasksService;

        public TasksController(ITasksService tasksService)
        {
            _tasksService = tasksService;
        }

        /// <summary>
        /// adds a specific task to a given card
        /// </summary>
        /// <param name="cardId">card id</param>
        /// <param name="createTaskDto">Request's payload id</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/tasks/cards?cardId={id}
        /// </remarks>
        [HttpPost("cards")]
        public async Task AddNewTask([FromQuery]int cardId,[FromBody]TaskDto createTaskDto)
        {
            await _tasksService.AddTask(cardId, createTaskDto);
        }

        /// <summary>
        /// updates the specified task
        /// </summary>
        /// <param name="taskId">   task id</param>
        /// <remarks>
        /// PUT cabanoss.azurewebsites.net/tasks?taskId={id}
        /// </remarks>
        [HttpPut]
        public async Task UpdateTask([FromQuery] int taskId, [FromBody] TaskDto updateTask)
        {
            await _tasksService.UpdateTask(taskId, updateTask);
        }

        /// <summary>
        /// downloads certain tasks from a given card
        /// </summary>
        /// <param name="cardId">card id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/tasks/cards?cardId={id}
        /// </remarks>
        [HttpGet("cards")]
        public async Task<List<ResponseTaskDto>> GetCardTasks([FromQuery] int cardId)
        {
            var tasks = await _tasksService.GetCardTasks(cardId);
            return tasks;
        }

        /// <summary>
        /// downloads a specific task
        /// </summary>
        /// <param name="taskId">   task id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/tasks?taskId={id}
        /// </remarks>
        [HttpGet]
        public async Task<ResponseTaskDto> GetTask([FromQuery] int taskId)
        {
           var task = await _tasksService.GetTask(taskId);
           return task;
        }

        /// <summary>
        /// deletes the specified task
        /// </summary>
        /// <param name="taskId">   task id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/tasks?taskId={id}
        /// </remarks>
        [HttpDelete]
        public async Task DeleteTask([FromQuery] int taskId)
        {
            await _tasksService.DeleteTask(taskId);
        }

    }
}
