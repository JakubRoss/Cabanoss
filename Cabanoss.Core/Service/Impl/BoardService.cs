using AutoMapper;
using Cabanoss.Core.Authorization;
using Cabanoss.Core.Common;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.Board;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Authorization;

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
        private async System.Threading.Tasks.Task<BoardUser> CheckBoardMembership(int boardId, int? userId)
        {
            var boardUser = await _boardUsersBaseRepository.GetFirstAsync(i => i.BoardId == boardId && i.UserId == userId);
            if (boardUser == null)
                throw new ResourceNotFoundException("User don't exists");
            return boardUser;
            
        }
        private async Task<Board> GetBoard(int boardId)
        {
            var board = await _boardRepository.GetFirstAsync(x => x.Id == boardId, i=>i.BoardUsers);
            if (board == null)
                throw new ResourceNotFoundException("Resource not found");
            return board;
        }
        #endregion

        public async Task<ResponseBoardDto> GetBoardAsync(int boardId)
        {
            var board = await GetBoard(boardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

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
            var board = await GetBoard(boardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Delete));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");
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
            var board = await GetBoard(boardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Read));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

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

            var board = await GetBoard(boardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Create));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var newBoardUser = new BoardUser { BoardId = boardId, UserId = userId };
            newBoardUser.Roles = Roles.User;
            await _boardUsersBaseRepository.AddAsync(newBoardUser);
        }

        public async Task RemoveUserAsync (int boardId, int userId)
        {
            var board = await GetBoard(boardId);
            var boardUserToRemove = await CheckBoardMembership(boardId, userId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Delete));
            if ((authorizationResult.Succeeded && boardUserToRemove.Roles !=Roles.Creator) || (_httpUserContextService.UserId == userId && boardUserToRemove.Roles !=Roles.Creator))
                await _boardUsersBaseRepository.DeleteAsync(boardUserToRemove);
            else
                throw new UnauthorizedException("Unauthorized");
        }

        public async Task UpdateBoardAsync(int boardId, UpdateBoardDto updateBoardDto)
        {
            var board = await GetBoard(boardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Update));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            board.Name = updateBoardDto.Name;
            board.UpdatedAt = DateTime.Now;
            await _boardRepository.UpdateAsync(board);
        }

        public async Task SetUserRole (int boardId,int userId, int roles)
        {
            var board = await GetBoard(boardId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpUserContextService.User, board, new ResourceOperationRequirement(ResourceOperations.Update));
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedException("Unauthorized");

            var boardUser = await CheckBoardMembership(boardId, userId);
            if (boardUser.Roles == Roles.Creator)
                throw new UnauthorizedException("Unauthorized");

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
                    throw new ConflictExceptions("Use 0 (Admin) or 1 (User) to set role");
            }
        }

    }
}

