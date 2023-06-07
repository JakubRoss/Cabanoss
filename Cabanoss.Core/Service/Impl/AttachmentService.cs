using AutoMapper;
using Cabanoss.Core.Authorization;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.Attachments;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Cabanoss.Core.Service.Impl
{
    public class AttachmentService : IAttachmentService
    {
        private IAttachmentRepository _attachmentRepository;
        private IMapper _mapper;
        private IBoardRepository _boardRepository;
        private IAuthorizationService _authorizationService;
        private IHttpUserContextService _IHttpUserContextService;

        public AttachmentService(
            IAttachmentRepository attachmentRepository,
            IMapper mapper,
            IBoardRepository boardRepository,
            IAuthorizationService authorizationService,
            IHttpUserContextService httpUserContextService)
        {
            _attachmentRepository = attachmentRepository;
            _mapper = mapper;
            _boardRepository = boardRepository;
            _authorizationService = authorizationService;
            _IHttpUserContextService = httpUserContextService;
        }

        #region Utils
        private async Task<Board> GetBoardByCardId(int cardId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(list => list.Cards.Any(card => card.Id == cardId)), i => i.BoardUsers);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");
            return board;
        }
        private async Task<Board> GetBoardByAttachmentId(int attachmentId)
        {
            var board = await _boardRepository.GetFirstAsync(board => board.Lists.Any(card => card.Cards.Any(comm => comm.Attachments.Any(id => id.Id == attachmentId))), i => i.BoardUsers);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");
            return board;
        }
        private async Task CheckBoardMembership(Board board)
        {
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");

            var authorizationResult = await _authorizationService.AuthorizeAsync(_IHttpUserContextService.User, board, new MembershipRequirements());
            if (!authorizationResult.Succeeded)
                throw new ResourceNotFoundException("no access");
        }
        private async Task<AuthorizationResult> CheckAdminRole(Board board)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(_IHttpUserContextService.User, board, new AdminRoleRequirements());
            return authorizationResult;
        }
        #endregion

        public async Task<List<AttachmentResponseDto>> GetAttachments(int cardId)
        {
            var board = await GetBoardByCardId(cardId);
            await CheckBoardMembership(board);

            var cardAttachments = await _attachmentRepository.GetAllAsync(p => p.CardId == cardId);
            var cardAttachmentsDto = _mapper.Map<List<AttachmentResponseDto>>(cardAttachments);
            return cardAttachmentsDto;

        }
        public async Task<AttachmentResponseDto> GetAttachment(int attachmentId)
        {
            var board = await GetBoardByAttachmentId(attachmentId);
            await CheckBoardMembership(board);

            var attachment = _attachmentRepository.GetFirstAsync(p => p.Id == attachmentId);
            var attachmentDto = _mapper.Map<AttachmentResponseDto>(attachment);

            return attachmentDto;
        }
        public async Task AddAttachment(int cardId, AttachmentDto attachment)
        {
            var board = await GetBoardByCardId(cardId);
            await CheckBoardMembership(board);

            var userId = (int)_IHttpUserContextService.UserId;

            var newAttachment = new Attachment()
            {
                Name = attachment.Name,
                Path = attachment.Path,
                DateCreated = DateTime.UtcNow,
                CardId = cardId,
                UserId = userId
            };

            await _attachmentRepository.AddAsync(newAttachment);
        }
        public async Task UpdateAttachment(int attachmentId, AttachmentDto attachment)
        {
            var board = await GetBoardByAttachmentId(attachmentId);

            await CheckBoardMembership(board);
            var adminAuthorization = await CheckAdminRole(board);

            var attachmentUpdate = await _attachmentRepository.GetFirstAsync(p => p.Id == attachmentId);

            var userId = _IHttpUserContextService.UserId;
            if (userId == attachmentUpdate.UserId || adminAuthorization.Succeeded)
            {
                if (attachmentUpdate.Path != null)
                    attachmentUpdate.Path = attachment.Path;
                if (attachmentUpdate.Name != null)
                    attachmentUpdate.Name = attachmentUpdate.Name;
                await _attachmentRepository.UpdateAsync(attachmentUpdate);
            }
            else
                throw new ResourceNotFoundException("No Access");
        }
        public async Task DeleteAttachment(int attachmentId)
        {
            var board = await GetBoardByAttachmentId(attachmentId);

            await CheckBoardMembership(board);
            var adminAuthorization = await CheckAdminRole(board);

            var attachment = await _attachmentRepository.GetFirstAsync(p => p.Id == attachmentId);

            var userId = _IHttpUserContextService.UserId;
            if (userId == attachment.UserId || adminAuthorization.Succeeded)
                await _attachmentRepository.DeleteAsync(attachment);
            else
                throw new ResourceNotFoundException("No Access");
        }
    }
}
