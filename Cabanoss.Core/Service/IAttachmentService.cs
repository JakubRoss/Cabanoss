using Cabanoss.Core.Model.Attachments;
using System.Security.Claims;

namespace Cabanoss.Core.Service
{
    public interface IAttachmentService
    {
        Task AddAttachment(int cardId, AttachmentDto attachment);
        Task DeleteAttachment(int attachmentId);
        Task<AttachmentResponseDto> GetAttachment(int attachmentId);
        Task<List<AttachmentResponseDto>> GetAttachments(int cardId);
        Task UpdateAttachment(int attachmentId, AttachmentDto attachment);
    }
}