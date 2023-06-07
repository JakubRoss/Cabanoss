using Cabanoss.Core.Model.Element;
using Cabanoss.Core.Model.User;

namespace Cabanoss.Core.Service
{
    public interface IElementService
    {
        Task AddElement(int taskId, ElementDto elementDto);
        Task CheckElement(int elementId, ElementCheckDto updateElementDto);
        Task DeleteElement(int elementId);
        Task<ResponseElementDto> GetElement(int elementId);
        Task<List<ResponseElementDto>> GetElements(int taskId);
        Task UpdateElement(int elementId, UpdateElementDto updateElementDto);
        Task<List<ResponseUserDto>> GetElementUsers(int elementId);
        Task AddUserToElement(int elementId, int userId);
        Task DeleteUserFromElement(int elementId, int userId);
    }
}