using Cabanoss.Core.Model.Card;
using Cabanoss.Core.Model.User;
using System.Security.Claims;

namespace Cabanoss.Core.Service
{
    public interface ICardService
    {
        Task AddCard(int listId, CreateCardDto createCard, int? cardId);
        Task DeleteCard(int cardId);
        Task<CardDto> GetCard(int cardId);
        Task<List<CardDto>> GetCards(int listId);
        Task UpdateCard(int cardId, UpdateCardDto createCard);
        Task SetDeadline(int cardId, DateOnly date);
        Task<List<ResponseUserDto>> GetCardUsers(int cardId);
        Task AddUserToCard(int cardId, int userId);
        Task DeleteUserFromCard(int cardId, int userId);
    }
}