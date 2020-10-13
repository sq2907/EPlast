﻿using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPlast.BLL.Interfaces.AzureStorage
{
   public interface IBlankFilesBlobStorageRepository
    {
        Task<CloudBlockBlob> GetBlobAsync(string blobName);
        Task DeleteBlobAsync(string blobName);
        Task UploadBlobAsync(IFormFile blobfile, string fileName);
        Task UploadBlobForBase64Async(string base64, string fileName);
        Task<string> GetBlobBase64Async(string blobName);
    }
}
