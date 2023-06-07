using Cabanoss.Core.Model.Comment;

namespace Cabanoss.Core.Service
{
    public interface ICommentServices
    {
        Task AddComment(int cardId, string text);
        Task DeleteComment(int commentId);
        Task<ResponseCommentDto> GetComment(int commentId);
        Task<List<ResponseCommentDto>> GetComments(int cardId);
        Task UpdateComment(int commentId, string text);
    }
}