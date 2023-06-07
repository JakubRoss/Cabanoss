using Cabanoss.Core.Model.List;
using System.Security.Claims;

namespace Cabanoss.Core.Service
{
    public interface IListService
    {
        Task<List<ListDto>> GetAllAsync(int boardId);
        Task CreateListAsync(int boardId, string name);
        Task<ListDto> GetListAsync(int listId);
        Task UpdateList(int listId, string name);
        Task SetDeadline(int listid, DateOnly date);
        Task DeleteList(int listId);
    }
}