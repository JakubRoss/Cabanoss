using AutoMapper;
using Cabanoss.Core.Common;
using Cabanoss.Core.Data.Entities;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Model.User;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cabanoss.Core.Service.Impl
{
    public class UserService : IUserService
    {
        private IUserRepository _userBase;
        private IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IWorkspaceService _workspaceBussiness;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IBoardRepository _boardRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IHttpUserContextService _httpUserContextService;

        public UserService(
            IUserRepository userBase,
            IBoardRepository boardRepository,
            IWorkspaceRepository workspaceRepository,
            IMapper mapper,
            IWorkspaceService workspaceBussiness,
            IPasswordHasher<User>passwordHasher,
            AuthenticationSettings authenticationSettings,
            IHttpUserContextService httpUserContextService)
        {
            _userBase = userBase;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _workspaceBussiness = workspaceBussiness;
            _authenticationSettings = authenticationSettings;
            _boardRepository = boardRepository;
            _workspaceRepository = workspaceRepository;
            _httpUserContextService = httpUserContextService;
        }
        #region Utils
        private async System.Threading.Tasks.Task<User> GetUser()
        {
            var id = _httpUserContextService.UserId;
            var user = await _userBase.GetFirstAsync(p => p.Id == id);
            if (user == null)
                throw new ResourceNotFoundException("User don't exists");
            return user;
        }
        private string GenerateJwt(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new ResourceNotFoundException("Invalid User name or password");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpiredays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            var TokenHandler = new JwtSecurityTokenHandler();

            return TokenHandler.WriteToken(token);
        }
        #endregion

        public async Task AddUserAsync(CreateUserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            var hashedPassword = _passwordHasher.HashPassword(user, userDto.Password);
            user.PasswordHash = hashedPassword;
            user.CreatedAt = DateTime.Now;
            await _userBase.AddAsync(user);

            await _workspaceBussiness.AddWorkspaceAsync();
        }
        public async Task<UserDto> GetUserAsync()
        {
            var user = await GetUser();
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }
        public async Task<UserDto> UpdateUserAsync(UpdateUserDto userDto)
        {
            var user = await GetUser();
            #region updt_properties
            if (userDto.Login!=null)
                user.Login = userDto.Login;
            if (userDto.Password != null)
            {
                var hashedPassword = _passwordHasher.HashPassword(user,userDto.Password);
                user.PasswordHash = hashedPassword;
            }
            if (userDto.Email != null)
                user.Email = userDto.Email;
            user.UpdatedAt = DateTime.Now;
            #endregion

            var updated = await _userBase.UpdateAsync(user);
            var updatedDto = _mapper.Map<UserDto>(updated);
            return updatedDto;
        }
        public async Task RemoveUserAsync()
        {
            var user = await GetUser();
            var boards = await _boardRepository.GetAllAsync(b=>b.BoardUsers.Any(id=>id.UserId == user.Id));
            if(boards!=null)
                await _boardRepository.DeleteRangeAsync(boards);
            var workspace = await _workspaceRepository.GetFirstAsync(x=>x.UserId == user.Id);
            await _workspaceRepository.DeleteAsync(workspace);
            await _userBase.DeleteAsync(user);
        }
        public async Task<List<ResponseUserDto>> GetUsersAsync(string searchingPhrase)
        {
            var users = await _userBase.GetUsersAsync(searchingPhrase);
            if(users is null)
                throw new ResourceNotFoundException("The application has no users");
            var usersDto = new List<ResponseUserDto>();
            foreach (var user in users)
            {
                var userDto = _mapper.Map<ResponseUserDto>(user);
                usersDto.Add(userDto);
            }
            return usersDto;
        }
        public async Task<LoginResult> LogIn(UserLoginDto userLoginDto)
        {
            var user = await _userBase.GetFirstAsync(u => u.Login.ToLower() == userLoginDto.Login.ToLower());
            if (user == null)
                throw new UnauthorizedException("Invalid User name or password");

            string tokenText = GenerateJwt(user, userLoginDto.Password);

            var userDto = _mapper.Map<ResponseUserDto>(user);
            var loginResult = new LoginResult()
            {
                Token = tokenText,
                User = userDto
            };
            return loginResult;
        }
    }
}
