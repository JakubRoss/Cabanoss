using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;


namespace Cabanoss.Core.Service.Impl
{
    public class FileContResult
    {
        public byte[] fileContents { get; set; }
        public string contentType { get; set; }
        public string fileName { get; set; }

        public FileContResult(byte[] fileContents, string contentType, string fileName)
        {
            this.fileContents = fileContents;
            this.contentType = contentType;
            this.fileName = fileName;
        }
    }

    public class FileService : IFileService
    {
        private IUserRepository _userRepository;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IHttpUserContextService _httpUserContextService;

        public FileService(
            IUserRepository userRepository,
            IHttpContextAccessor IHttpContextAccessor,
            IHttpUserContextService httpUserContextService)
        {
            _userRepository = userRepository;
            _IHttpContextAccessor = IHttpContextAccessor;
            _httpUserContextService = httpUserContextService;
        }
        private bool GetFileExtension(IFormFile file, out string ext)
        {
            string[] allowedExtensions = {".jpeg",".jpg",".png"}; 
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ext = extension;
                return false;
            }
            ext = extension;
            return true;
        }
        public async Task<FileContResult> GetFile()
        {
            var id = _httpUserContextService.UserId;

            var rootPath = Directory.GetParent(Directory.GetCurrentDirectory());
            var folderPath = $"{rootPath}\\Cabanoss.Core\\Files\\{id}";

            if (!Directory.Exists(folderPath))
            {
                throw new ResourceNotFoundException("File doesn't exist");
            }

            var file = Directory.GetFiles(folderPath).First();
            if(file == null)
            {
                throw new ResourceNotFoundException("File doesn't exist");
            }

            var fileName = Path.GetFileName(file);

            var contentProvider = new FileExtensionContentTypeProvider();
            contentProvider.TryGetContentType(file, out var contentType);

            var fileContents = System.IO.File.ReadAllBytes(file);

            return new FileContResult(fileContents, contentType, fileName);
        }

        public async Task UploadFile(IFormFile file)
        {
            var id = (int)_httpUserContextService.UserId;
            var login = _httpUserContextService.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            var ext = string.Empty;
            var allowedExtension = GetFileExtension(file, out ext);
            if (file is null || file.Length > 1048576 || !allowedExtension)
            {
                throw new ResourceNotFoundException("incorrect file format or size");
            }

            var rootPath = Directory.GetParent(Directory.GetCurrentDirectory());
            var name = $"{id}_{login}AV{ext}";
            var directoryPath = $"{rootPath}\\Cabanoss.Core\\Files\\{id}";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var files = Directory.GetFiles(directoryPath);
            if(files.Length > 0)
            {
                foreach(var obj in files)
                    File.Delete(obj);
            }

            var filePath = $"{rootPath}\\Cabanoss.Core\\Files\\{id}\\{name}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Konstruowanie pełnej ścieżki URL na platformie
            var baseUrl = $"{_IHttpContextAccessor.HttpContext.Request.Scheme}://{_IHttpContextAccessor.HttpContext.Request.Host}";

            // Tworzenie ścieżki URL do pliku
            var fileUrl = $"{baseUrl}/Cabanoss.Core/Files/{id}/{name}";
            var user = await _userRepository.GetFirstAsync(x => x.Id == id);
            user.AvatarPath = fileUrl;
            await _userRepository.UpdateAsync(user);
        }
    }
}
