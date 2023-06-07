using Cabanoss.API.Swagger;
using Cabanoss.Core.Model.Attachments;
using Cabanoss.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cabanoss.API.Controllers
{
    [Route("attachments")]
    [ApiController]
    [Authorize]
    [SwaggerControllerOrder(6)]
    public class AttachmentsController : ControllerBase
    {
        private IAttachmentService _attachmentService;

        public AttachmentsController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        /// <summary>
        /// Adds an attachment to a given card
        /// </summary>
        /// <param name="cardId">Card id</param>
        /// <param name="attachmentDto">Request's payload</param>
        /// <remarks>
        /// POST cabanoss.azurewebsites.net/attachments/cards?cardId={id}
        /// </remarks>
        [HttpPost("cards")]
        public async Task AddAttachment([FromQuery] int cardId, [FromBody] AttachmentDto attachmentDto)
        {
            await _attachmentService.AddAttachment(cardId, attachmentDto);
        }

        /// <summary>
        /// updates the indicated attachment
        /// </summary>
        /// <param name="attachmentId">attachment id</param>
        /// <param name="attachmentDto">Request's payload</param>
        /// <remarks>
        /// PUT cabanoss.azurewebsites.net/attachments?attachmentId={id}
        /// </remarks>
        [HttpPut]
        public async Task UpdateAttachment([FromQuery] int attachmentId, [FromBody] AttachmentDto attachmentDto)
        {
            await _attachmentService.UpdateAttachment(attachmentId, attachmentDto);
        }

        /// <summary>
        /// Downloads attachments from a given card
        /// </summary>
        /// <param name="cardId">Card id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/attachments/cards?cardId={id}
        /// </remarks>
        [HttpGet("cards")]
        public async Task<List<AttachmentResponseDto>> GetAttachments([FromQuery] int cardId)
        {
            var attachments = await _attachmentService.GetAttachments(cardId);
            return attachments;
        }

        /// <summary>
        /// downloads the indicated attachment
        /// </summary>
        /// <param name="attachmentId">attachment id</param>
        /// <remarks>
        /// GET cabanoss.azurewebsites.net/attachments?attachmentId={id}
        /// </remarks>
        [HttpGet]
        public async Task<AttachmentResponseDto> GetAttachment([FromQuery] int attachmentId)
        {
            var attachment = await _attachmentService.GetAttachment(attachmentId);
            return attachment;
        }

        /// <summary>
        /// deletes the indicated attachment
        /// </summary>
        /// <param name="attachmentId">attachment id</param>
        /// <remarks>
        /// DELETE cabanoss.azurewebsites.net/attachments?attachmentId={id}
        /// </remarks>
        [HttpDelete]
        public async Task DeleteAttachment([FromQuery] int attachmentId)
        {
            await _attachmentService.DeleteAttachment(attachmentId);
        }
    }
}
