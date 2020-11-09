﻿using AutoMapper;
using EPlast.BLL.DTO.City;
using EPlast.BLL.Interfaces.AzureStorage;
using EPlast.BLL.Interfaces.City;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EPlast.BLL.Interfaces;

namespace EPlast.BLL.Services.City
{
    public class CityDocumentsService : ICityDocumentsService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ICityFilesBlobStorageRepository _cityFilesBlobStorage;
        private readonly IUniqueIdService _uniqueId;

        public CityDocumentsService(IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ICityFilesBlobStorageRepository cityFilesBlobStorage,
            IUniqueIdService uniqueId)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _cityFilesBlobStorage = cityFilesBlobStorage;
            _uniqueId = uniqueId;
        }

        private async Task<IEnumerable<CityDocumentType>> GetAllCityDocumentTypeEntities()
        {
            var documentTypes = await _repositoryWrapper.CityDocumentType.GetAllAsync();

            return documentTypes;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CityDocumentTypeDTO>> GetAllCityDocumentTypesAsync()
        {
            var documentTypes = await GetAllCityDocumentTypeEntities();

            return _mapper.Map<IEnumerable<CityDocumentType>, IEnumerable<CityDocumentTypeDTO>>(documentTypes);
        }

        /// <inheritdoc />
        public async Task<CityDocumentsDTO> AddDocumentAsync(CityDocumentsDTO documentDTO)
        {
            var fileBase64 = documentDTO.BlobName.Split(',')[1];
            var extension = $".{documentDTO.FileName.Split('.').LastOrDefault()}";
            var fileName = $"{_uniqueId.GetUniqueId()}{extension}";

            await _cityFilesBlobStorage.UploadBlobForBase64Async(fileBase64, fileName);
            documentDTO.BlobName = fileName;

            var documentTypes = await GetAllCityDocumentTypesAsync();
            documentDTO.CityDocumentType = documentTypes
                .FirstOrDefault(dt => dt.Name == documentDTO.CityDocumentType.Name);
            documentDTO.CityDocumentTypeId = documentDTO.CityDocumentType.ID;

            var document = _mapper.Map<CityDocumentsDTO, CityDocuments>(documentDTO);
            _repositoryWrapper.CityDocuments.Attach(document);
            await _repositoryWrapper.CityDocuments.CreateAsync(document);
            await _repositoryWrapper.SaveAsync();

            return documentDTO;
        }

        public async Task<string> DownloadFileAsync(string fileName)
        {
            var fileBase64 = await _cityFilesBlobStorage.GetBlobBase64Async(fileName);

            return fileBase64;
        }

        public async Task DeleteFileAsync(int documentId)
        {
            var document = await _repositoryWrapper.CityDocuments
                .GetFirstOrDefaultAsync(d => d.ID == documentId);

            await _cityFilesBlobStorage.DeleteBlobAsync(document.BlobName);

            _repositoryWrapper.CityDocuments.Delete(document);
            await _repositoryWrapper.SaveAsync();
        }
    }
}
