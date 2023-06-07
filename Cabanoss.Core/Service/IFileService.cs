using Cabanoss.Core.Service.Impl;
using Microsoft.AspNetCore.Http;

namespace Cabanoss.Core.Service
{
    public interface IFileService
    {
        Task<FileContResult> GetFile();
        Task UploadFile(IFormFile file);
    }
}