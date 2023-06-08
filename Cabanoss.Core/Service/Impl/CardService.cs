using AutoMapper;
using Cabanoss.Core.Authorization;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.Card;
using Cabanoss.Core.Model.User;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Cabanoss.Core.Service.Impl
{
    public class CardService : ICardService
    {
        private ICardRepository _cardRepository;
        private IMapper _mapper;
        private IBoardRepository _boardRepository;
        private IAuthorizationService _authorizationService;
        private IUserRepository _userRepository;
        private ICardUserRepository _cardUserRepository;
        private IHttpUserContextService _httpUserContextService;

        public CardService(
            ICardRepository cardRepository,
            IMapper mapper,
            IBoardRepository boardRepository,
            IAuthorizationService authorizationService,
            IUserRepository userRepository,
            ICardUserRepository cardUserRepository,
            IHttpUserContextService httpUserContextService)
        {
            _cardRepository = cardRepository;
            _mapper = mapper;
            _boardRepository = boardRepository;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _cardUserRepository = cardUserRepository;
            _httpUserContextService = httpUserContextService;
        }
        #region Utils
        private async Task<Card> GetCardById(int cardId)
        {
            var card = await _cardRepository.GetFirstAsync(p => p.Id == cardId, i=>i.CardUsers);
            if (card == null)
                throw new ResourceNotFoundException("ResourceNotFound");
            return card;
        }
        private async Task<Board> GetBoard(int cardId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(c=>c.Cards.Any(c=>c.Id == cardId)), i=>i.BoardUsers);
            if (board == null)
                throw new ResourceNotFoundException("Resource not found");
            return board;
        }
        private async System.Threading.Tasks.Task CheckBoardMembership(int cardId, int userId)
        {
            var board = await GetBoard(cardId);
            var user = board.BoardUsers.FirstOrDefault(x => x.UserId == userId);
            if (user == null)
                throw new UnauthorizedException("the user you want to add does not belong to the table or does not exist");
        }
        #endregion

        public async Task<List<CardDto>> GetCards(int listId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(list => list.Id == listId), i => i.BoardUsers);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var lists = await _cardRepository.GetAllAsync(p => p.ListId == listId);
            var listsDto = _mapper.Map<List<CardDto>>(lists);
            return listsDto;
        }

        public async Task<CardDto> GetCard(int cardId)
        {
            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var card = await GetCardById(cardId);
            var cardDto = _mapper.Map<CardDto>(card);
            return cardDto;
        }

        public async Task AddCard(int listId, CreateCardDto createCard, int? cardId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(list => list.Id == listId), i => i.BoardUsers);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Create));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            if (cardId == null)
            {
                Card card = new Card
                {
                    Name = createCard.Name,
                    Description = createCard.Description,
                    CreatedAt = DateTime.UtcNow,
                    ListId = listId

                };
                await _cardRepository.AddAsync(card);
            }
            if(cardId != null)
            {
                var exist = await _boardRepository.GetFirstAsync(b => b.Lists.Any(l => l.Cards.Any(c => c.Id == cardId)));
                var transferCard = await _cardRepository.GetFirstAsync(c => c.Id == cardId);
                var card = exist != null ? _cardRepository.GetFirstAsync(c => c.Id == cardId).Result : throw new ConflictExceptions("card does not belong to the board or does not exist");
                card.ListId = listId;
                await _cardRepository.UpdateAsync(card);
            }
        }

        public async Task DeleteCard(int cardId)
        {
            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Create));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var card = await GetCardById(cardId);
            await _cardRepository.DeleteAsync(card);
        }

        public async Task UpdateCard(int cardId, UpdateCardDto createCard)
        {
            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Update));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var card = await GetCardById(cardId);

            #region updt_properties
            if (createCard.Name != null)
                card.Name = createCard.Name;
            if (createCard.Description != null)
                card.Description = createCard.Description;
            #endregion
            card.UpdateAt = DateTime.UtcNow;
            await _cardRepository.UpdateAsync(card);
        }

        public async Task SetDeadline(int cardId, DateOnly date)
        {
            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Update));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var card = await GetCardById(cardId);

            card.Deadline = date.ToDateTime(new TimeOnly());
            await _cardRepository.UpdateAsync(card);
        }

        //For members
        public async Task<List<ResponseUserDto>> GetCardUsers(int cardId)
        {
            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var cardUsers = await _userRepository.GetAllAsync(cu=>cu.CardUsers.Any(cu=>cu.CardId == cardId));
            if (cardUsers == null)
                throw new ResourceNotFoundException("Resource not Found");

            var usersDto = _mapper.Map<List<ResponseUserDto>>(cardUsers);
            return usersDto;

        }
        public async Task AddUserToCard(int cardId, int userId)
        {
            var card = await GetCardById(cardId);
            var user = card.CardUsers.FirstOrDefault(i=>i.UserId == userId);
            if (user != null)
                throw new ConflictExceptions("the object is in the resource");

            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Update));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            await CheckBoardMembership(cardId, userId);

            var userCard = new CardUser()
            {
                UserId = userId,
                CardId = cardId
            };

            await _cardUserRepository.AddAsync(userCard);
        }
        public async Task DeleteUserFromCard(int cardId, int userId)
        {
            var board = await GetBoard(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Delete));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            await CheckBoardMembership(cardId, userId);

            var card = await GetCardById(cardId);
            var cardUser = card.CardUsers.FirstOrDefault(id=>id.UserId == userId);
            if (cardUser == null)
                throw new ResourceNotFoundException("the user you want to add does not belong to the card or does not exist");
            await _cardUserRepository.DeleteAsync(cardUser);
        }
    }
}
