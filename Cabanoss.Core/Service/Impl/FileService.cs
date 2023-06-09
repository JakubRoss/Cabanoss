﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cabanoss.Core.Common;
using Cabanoss.Core.Exceptions;
using Cabanoss.Core.Repositories;
using Microsoft.AspNetCore.Http;

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
        private IHttpUserContextService _httpUserContextService;

        public FileService(
            IUserRepository userRepository,
            IHttpUserContextService httpUserContextService)
        {
            _userRepository = userRepository;
            _httpUserContextService = httpUserContextService;

        }
        #region Utils
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
        public async Task<BlobClient> FindFile(string fileName, AzureProps azureProps)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(azureProps.AzureStorageConnection);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(azureProps.containerName);

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                if (blobItem.Name.Contains(fileName))
                {
                    return containerClient.GetBlobClient(blobItem.Name);
                }
            }

            return null;
        }
        private string GetContentType(string fileName)
        {
            var fileExtension = $".{fileName.Split('.').Last()}";
            if (fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                return "image/jpeg";
            }
            else if (fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                return "image/png";
            }
            else
            {
                // Domyślny typ zawartości dla innych plików
                return "application/octet-stream";
            }
        }
        #endregion

        public async Task<FileContResult> GetFile(AzureProps azureProps)
        {
            var id = _httpUserContextService.UserId;
            var login = _httpUserContextService.UserLogin;
            var name = $"{id}_{login}AV";

            var blobClient = await FindFile(name,azureProps);
            if (blobClient is null)
            {
                throw new ResourceNotFoundException("File doesn't exist");
            }

            byte[] file;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(memoryStream);
                 file = memoryStream.ToArray();
            }

            var contentType = GetContentType(blobClient.Name);
            if (contentType != null)
            {
                blobClient.SetHttpHeaders(new BlobHttpHeaders
                {
                    ContentType = contentType,
                });
            }

            string uri = blobClient.Uri.AbsoluteUri;
            string fileName = uri.Substring(uri.LastIndexOf('/') + 1);

            return new FileContResult(file, contentType, fileName);
        }

        public async Task UploadFile(AzureProps azureProps,IFormFile file)
        {
            var id = _httpUserContextService.UserId;
            var login = _httpUserContextService.UserLogin;

            var ext = string.Empty;
            var allowedExtension = GetFileExtension(file, out ext);
            if (file is null || file.Length > 1048576 || !allowedExtension)
            {
                throw new ConflictExceptions("incorrect file format or size");
            }

            var name = $"{id}_{login}AV{ext}";
            var blobFile = await FindFile(name.Split('.').First(), azureProps);
            if (blobFile != null)
            {
                blobFile.DeleteIfExists();
            }

            BlobServiceClient blobServiceClient = new BlobServiceClient(azureProps.AzureStorageConnection);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(azureProps.containerName);

            BlobClient blobClient = containerClient.GetBlobClient(name);

            var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);
            blobClient.SetHttpHeaders(new BlobHttpHeaders
            {
                ContentDisposition = "inline" // Ustaw nagłówek Content-Disposition, aby wskazać, że plik ma być wyświetlany w przeglądarce, a nie pobierany
            });
            var uri = blobClient.Uri;

            var user = await _userRepository.GetFirstAsync(x => x.Id == id);
            user.AvatarPath = uri.ToString();
            await _userRepository.UpdateAsync(user);

        }
    }
}
