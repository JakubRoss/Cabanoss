using AutoMapper;
using Cabanoss.Core.Authorization;
using Cabanoss.Core.Common;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.Board;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Cabanoss.Core.Service.Impl
{
    public class BoardService : IBoardService
    {
        private IBoardRepository _boardRepository;
        private IMapper _mapper;
        private IBoardUsersRepository _boardUsersBaseRepository;
        private IUserRepository _userBase;
        private IAuthorizationService _authorizationService;
        private IUserRepository _userRepository;
        private IHttpUserContextService _httpUserContextService;

        public BoardService(IBoardRepository boardBaseRepository,
            IMapper mapper,
            IBoardUsersRepository boardUsersBaseRepository,
            IUserRepository userBase,
            IAuthorizationService authorizationService,
            IUserRepository userRepository,
            IHttpUserContextService httpUserContextService)
        {
            _boardRepository = boardBaseRepository;
            _mapper = mapper;
            _boardUsersBaseRepository = boardUsersBaseRepository;
            _userBase = userBase;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _httpUserContextService = httpUserContextService;
        }
        #region utils
        /// <summary>
        /// Sprawdza przynaleznosc uzytkownika(pytajacego) do danej tablicy
        /// </summary>
        /// <param name="boardId">Id tablicy.</param>
        /// <param name="user">Claims principal.</param>
        /// <returns>Authorization result</returns>
        private async System.Threading.Tasks.Task<AuthorizationResult> CheckBoardMembership(int boardId)
        {
            var board = await _boardRepository.GetFirstAsync(i => i.Id == boardId, i => i.BoardUsers);
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new MembershipRequirements());
            return authorizationResult;
        }
        private async System.Threading.Tasks.Task<BoardUser> CheckBoardMembership(int boardId, int userId)
        {
            var boardUser = await _boardUsersBaseRepository.GetFirstAsync(i => i.BoardId == boardId && i.UserId == userId);
            return boardUser;
            
        }
        private async Task<AuthorizationResult> CheckCreatorRole(int boardId)
        {
            var board = await _boardRepository.GetFirstAsync(i => i.Id == boardId, i => i.BoardUsers);
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new CreatorRoleRequirements());
            return authorizationResult;
        }
        private async Task<AuthorizationResult> CheckAdminRole(int boardId)
        {
            var board = await _boardRepository.GetFirstAsync(i => i.Id == boardId, i => i.BoardUsers);
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new AdminRoleRequirements());
            return authorizationResult;
        }
        #endregion

        public async Task<ResponseBoardDto> GetBoardAsync(int boardId)
        {
            var authorizationResult = await CheckBoardMembership(boardId);
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");
            var board = await _boardRepository.GetFirstAsync(x => x.Id == boardId);

            return _mapper.Map<ResponseBoardDto>(board); 
        }
        public async Task CreateBoardAsync(CreateBoardDto createBoardDto)
        {
            var userDb = await _userBase.GetFirstAsync(p => p.Id == _httpUserContextService.UserId, i => i.Workspace);

            var board = _mapper.Map<Board>(createBoardDto);
            board.CreatedAt = DateTime.Now;
            board.WorkspaceId = userDb.Workspace.Id;
            var sboard = await _boardRepository.AddAsync(board);

            var newBoardUser = new BoardUser { BoardId = sboard.Id, UserId = userDb.Id };
            newBoardUser.Roles = Roles.Creator;
            await _boardUsersBaseRepository.AddAsync(newBoardUser);

        }

        public async Task DeleteBoardAsync(int boardId)
        {
            var authorizationRoleResult = await CheckCreatorRole(boardId);
            if (!authorizationRoleResult.Succeeded)
                throw new ResourceNotFoundException("No Access");
            var board = await _boardRepository.GetFirstAsync(p => p.Id == boardId);
            if (board is null)
                throw new ResourceNotFoundException("Resource Not Found");
            await _boardRepository.DeleteAsync(board);
        }

        public async Task<List<ResponseBoardDto>> GetBoardsAsync()
        {
            var userboards = await _boardUsersBaseRepository.GetAllAsync(p=>p.UserId == _httpUserContextService.UserId);
            var boards = new List<Board>();
            foreach (var board in userboards)
            {
                var cos = await _boardRepository.GetFirstAsync(p=>p.Id == board.BoardId);
                boards.Add(cos);
            }
            var responseBoards = _mapper.Map<List<ResponseBoardDto>>(boards);
            return responseBoards;
        }

        public async Task<List<ResponseBoardUser>> GetBoardUsersAsync(int boardId)
        {
            var authorizationMembershipResult = await CheckBoardMembership(boardId);
            if (!authorizationMembershipResult.Succeeded)
                throw new ResourceNotFoundException("no access");

            var users = await _userBase.GetAllAsync(u => u.BoardUsers.Any(bu => bu.BoardId == boardId));

            var mapedUsers = new List<ResponseBoardUser>();
            if(users != null)
            {
                foreach (var user in users)
                {
                    var role = user.BoardUsers.Where(id=>id.BoardId == boardId).Select(r=>r.Roles).First();
                    mapedUsers.Add(new ResponseBoardUser
                    {
                        Id = user.Id,
                        Login = user.Login,
                        Email = user.Email,
                        IsAdmin = (role != Roles.User) ? true : false
                    });
                }
            }
            return mapedUsers;
        }

        public async Task AddUsersAsync(int boardId , int userId)
        {
            var isUserExist = await _userRepository.GetFirstAsync(i=>i.Id == userId);
            if (isUserExist == null)
                throw new ResourceNotFoundException("User don't exists");

            var authorizationRoleResult = await CheckAdminRole(boardId);
            if (!authorizationRoleResult.Succeeded)
                throw new ResourceNotFoundException("No Access");

            var newBoardUser = new BoardUser { BoardId = boardId, UserId = userId };
            newBoardUser.Roles = Roles.User;
            await _boardUsersBaseRepository.AddAsync(newBoardUser);
        }

        public async Task RemoveUserAsync (int boardId, int userId)
        {
            var authorizationRoleResult = await CheckAdminRole(boardId);
            if (!authorizationRoleResult.Succeeded)
                throw new ResourceNotFoundException("No Access");

            var boardUser = await _boardUsersBaseRepository.GetFirstAsync(i=>i.BoardId == boardId&& i.UserId ==userId);
            if (boardUser != null)
                await _boardUsersBaseRepository.DeleteAsync(boardUser);
        }

        public async Task UpdateBoardAsync(int boardId, UpdateBoardDto updateBoardDto)
        {
            var board = await _boardRepository.GetFirstAsync(i=>i.Id == boardId);
            if(board is null)
                throw new ResourceNotFoundException("ResourceNotFound");

            var authorizationRoleResult = await CheckCreatorRole(boardId);
            if (!authorizationRoleResult.Succeeded)
                throw new ResourceNotFoundException("No Access");

            board.Name = updateBoardDto.Name;
            board.UpdatedAt = DateTime.Now;
            await _boardRepository.UpdateAsync(board);
        }

        public async Task SetUserRole (int boardId,int userId, int roles)
        {
            var authorizationRoleResult = await CheckAdminRole(boardId);
            if (!authorizationRoleResult.Succeeded)
                throw new ResourceNotFoundException("No Access");
            var boardUser = await CheckBoardMembership(boardId, userId);
            if (boardUser == null || boardUser.Roles == Roles.Creator)
                throw new ResourceNotFoundException("ResourceNotFound");

            switch (roles)
            {
                case 0:
                    boardUser.Roles = Roles.Admin;
                    await _boardUsersBaseRepository.UpdateAsync(boardUser);
                    break;
                case 1:
                    boardUser.Roles = Roles.User;
                    await _boardUsersBaseRepository.UpdateAsync(boardUser);
                    break;
                default:
                    throw new ResourceNotFoundException("ResourceNotFound");
            }
        }

    }
}

