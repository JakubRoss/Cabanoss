using Cabanoss.Core.Authorization;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.Comment;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Cabanoss.Core.Service.Impl
{
    public class CommentServices : ICommentServices
    {
        private ICommentRepository _commentRepository;
        private IBoardRepository _boardRepository;
        private IAuthorizationService _authorizationService;
        private IUserRepository _userRepository;
        private IHttpUserContextService _httpUserContextService;

        public CommentServices(
            ICommentRepository commentRepository,
            IBoardRepository boardRepository,
            IAuthorizationService authorizationService,
            IUserRepository userRepository,
            IHttpUserContextService httpUserContextService)
        {
            _commentRepository = commentRepository;
            _boardRepository = boardRepository;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _httpUserContextService = httpUserContextService;
        }

        #region Utils
        private async Task<Board> GetBoardByCardId(int cardId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(list => list.Cards.Any(card => card.Id == cardId)), i => i.BoardUsers);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");
            return board;
        }
        private async Task<Board> GetBoardByCommentId(int commentId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(card => card.Cards.Any(comm => comm.Comments.Any(id => id.Id == commentId))), i => i.BoardUsers);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");
            return board;
        }
        #endregion

        public async Task<List<ResponseCommentDto>> GetComments(int cardId)
        {
            var board = await GetBoardByCardId(cardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var cardComments = await _commentRepository.GetAllAsync(p => p.CardId == cardId);
            var cardDtoComments = new List<ResponseCommentDto>();
            foreach (var comment in cardComments)
            {
                var user = await _userRepository.GetFirstAsync(x => x.Id == comment.UserId);
                cardDtoComments.Add(new ResponseCommentDto
                {
                    Id = comment.Id,
                    UserId = user.Id,
                    Author = user.Login,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                });
            }
            return cardDtoComments;

        }
        public async Task<ResponseCommentDto> GetComment(int commentId)
        {
            var board = await GetBoardByCommentId(commentId);
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var comment = await _commentRepository.GetFirstAsync(p => p.Id == commentId);
            if (comment is null)
                throw new ResourceNotFoundException("comment does not exist");

            return new ResponseCommentDto 
            { 
                Id = comment.Id,
                UserId = comment.UserId,
                Author = _userRepository.GetFirstAsync(i=>i.Id==comment.UserId).Result.Login,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };
        }
        public async Task AddComment(int cardId, string text)
        {
            var board = await GetBoardByCardId(cardId);
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var userId = (int)_httpUserContextService.UserId;

            var comment = new Comment()
            {
                Text = text,
                CreatedAt = DateTime.Now,
                UserId = userId,
                CardId = cardId
            };

            await _commentRepository.AddAsync(comment);
        }
        public async Task UpdateComment(int commentId, string text)
        {
            var board = await GetBoardByCommentId(commentId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Update));

            var comment = await _commentRepository.GetFirstAsync(p => p.Id == commentId);
            if (comment is null)
                throw new ResourceNotFoundException("Reosurce not found");

            if ((int)_httpUserContextService.UserId == comment.UserId || authorizationResult.Succeeded)
            {
                comment.Text = text;
                await _commentRepository.UpdateAsync(comment);
            }
            else
                throw new UnauthorizedException("Unauthorized");
        }
        public async Task DeleteComment(int commentId)
        {
            var board = await GetBoardByCommentId(commentId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Delete));

            var comment = await _commentRepository.GetFirstAsync(p => p.Id == commentId);

            if ((int)_httpUserContextService.UserId == comment.UserId || authorizationResult.Succeeded)
                await _commentRepository.DeleteAsync(comment);
            else
                throw new UnauthorizedException("Unauthorized");
        }
    }
}
