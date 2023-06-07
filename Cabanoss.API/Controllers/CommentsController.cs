using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.Comment;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [Route("comments")]
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(5)]
    public class CommentsController : ControllerBase
    {
        private ICommentServices _commentServices;

        public CommentsController(ICommentServices commentServices)
        {
            _commentServices = commentServices;
        }

        /// <summary>
        /// adds commentary to a given card
        /// </summary>
        /// <param name="cardId">card id</param>
        /// <param name="commentDto">Request's payload</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/comments/cards?cardId={id}
        /// </remarks>
        [HttpPost("cards")]
        public async Task AddComment([FromQuery] int cardId, [FromBody] CommentDto commentDto)
        {
            await _commentServices.AddComment(cardId, commentDto.Text);
        }

        /// <summary>
        /// updates a given comment
        /// </summary>
        /// <param name="commentId">comment id</param>
        /// <param name="commentDto">Request's payload</param>
        /// <remarks>
        /// PUT cabanoss.azurewebsites.net/comments?commentId={id}
        /// </remarks>
        [HttpPut]
        public async Task UpdateComment([FromQuery] int commentId, [FromBody] CommentDto commentDto)
        {
            await _commentServices.UpdateComment(commentId, commentDto.Text);
        }

        /// <summary>
        /// downloads comments of a given card
        /// </summary>
        /// <param name="cardId">Card id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/comments/cards?cardId={id}
        /// </remarks>
        [HttpGet("cards")]
        public async Task<List<ResponseCommentDto>> GetComments([FromQuery] int cardId)
        {
            var comments = await _commentServices.GetComments(cardId);
            return comments;
        }

        /// <summary>
        /// downloads a given comment
        /// </summary>
        /// <param name="commentId">comment id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/comments?commentId={id}
        /// </remarks>
        [HttpGet]
        public async Task<ResponseCommentDto> GetComment([FromQuery] int commentId)
        {
            var comment = await _commentServices.GetComment(commentId);
            return comment;
        }

        /// <summary>
        /// deletes a given comment
        /// </summary>
        /// <param name="commentId">comment id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/comments?commentId={id}
        /// </remarks>
        [HttpDelete]
        public async Task DeleteCard([FromQuery] int commentId)
        {
            await _commentServices.DeleteComment(commentId);
        }
    }
}
