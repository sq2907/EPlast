﻿using AutoMapper;
using EPlast.BLL.DTO.AnnualReport;
using EPlast.BLL.Interfaces.City;
using EPlast.BLL.Services.Interfaces;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPlast.BLL.Services
{
    public class AnnualReportService : IAnnualReportService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ICityAccessService _cityAccessService;
        private readonly IMapper _mapper;

        public AnnualReportService(IRepositoryWrapper repositoryWrapper, ICityAccessService cityAccessService, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _cityAccessService = cityAccessService;
            _mapper = mapper;
        }

        ///<inheritdoc/>
        public async Task<AnnualReportDTO> GetByIdAsync(User user, int id)
        {
            var annualReport = await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                    predicate: a => a.ID == id,
                    include: source => source
                        .Include(a => a.NewCityAdmin)
                        .Include(a => a.MembersStatistic)
                        .Include(a => a.City));
            return await _cityAccessService.HasAccessAsync(user, annualReport.CityId) ? _mapper.Map<AnnualReport, AnnualReportDTO>(annualReport)
                : throw new UnauthorizedAccessException();
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<AnnualReportDTO>> GetAllAsync(User user)
        {
            var annualReports = await _repositoryWrapper.AnnualReports.GetAllAsync(
                    include: source => source
                        .Include(ar => ar.Creator)
                        .Include(ar => ar.City)
                            .ThenInclude(c => c.Region));
            var citiesDTO = await _cityAccessService.GetCitiesAsync(user);
            var filteredAnnualReports = annualReports.Where(ar => citiesDTO.Any(c => c.ID == ar.CityId));
            return _mapper.Map<IEnumerable<AnnualReport>, IEnumerable<AnnualReportDTO>>(filteredAnnualReports);
        }

        ///<inheritdoc/>
        public async Task CreateAsync(User user, AnnualReportDTO annualReportDTO)
        {
            var city = await _repositoryWrapper.City.GetFirstOrDefaultAsync(
                predicate: a => a.ID == annualReportDTO.CityId);
            if (!await _cityAccessService.HasAccessAsync(user, city.ID))
            {
                throw new UnauthorizedAccessException();
            }
            if (await CheckCreated(city.ID))
            {
                throw new InvalidOperationException();
            }
            var annualReport = _mapper.Map<AnnualReportDTO, AnnualReport>(annualReportDTO);
            annualReport.CreatorId = user.Id;
            annualReport.Date = DateTime.Now;
            annualReport.Status = AnnualReportStatus.Unconfirmed;
            await _repositoryWrapper.AnnualReports.CreateAsync(annualReport);
            await _repositoryWrapper.SaveAsync();
        }

        ///<inheritdoc/>
        public async Task EditAsync(User user, AnnualReportDTO annualReportDTO)
        {
            var annualReport = await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                    predicate: a => a.ID == annualReportDTO.ID && a.CityId == annualReportDTO.CityId && a.CreatorId == annualReportDTO.CreatorId
                        && a.Date.Date == annualReportDTO.Date.Date && a.Status == AnnualReportStatus.Unconfirmed);
            if (annualReportDTO.Status != AnnualReportStatusDTO.Unconfirmed)
            {
                throw new InvalidOperationException();
            }
            if (!await _cityAccessService.HasAccessAsync(user, annualReport.CityId))
            {
                throw new UnauthorizedAccessException();
            }
            annualReport = _mapper.Map<AnnualReportDTO, AnnualReport>(annualReportDTO);
            _repositoryWrapper.AnnualReports.Update(annualReport);
            await _repositoryWrapper.SaveAsync();
        }

        ///<inheritdoc/>
        public async Task ConfirmAsync(User user, int id)
        {
            var annualReport = await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                    predicate: a => a.ID == id && a.Status == AnnualReportStatus.Unconfirmed);
            if (!await _cityAccessService.HasAccessAsync(user, annualReport.CityId))
            {
                throw new UnauthorizedAccessException();
            }
            annualReport.Status = AnnualReportStatus.Confirmed;
            _repositoryWrapper.AnnualReports.Update(annualReport);
            await SaveLastConfirmedAsync(annualReport.CityId);
            await _repositoryWrapper.SaveAsync();
        }

        ///<inheritdoc/>
        public async Task CancelAsync(User user, int id)
        {
            var annualReport = await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                    predicate: a => a.ID == id && a.Status == AnnualReportStatus.Confirmed);
            if (!await _cityAccessService.HasAccessAsync(user, annualReport.CityId))
            {
                throw new UnauthorizedAccessException();
            }
            annualReport.Status = AnnualReportStatus.Unconfirmed;
            _repositoryWrapper.AnnualReports.Update(annualReport);
            await _repositoryWrapper.SaveAsync();
        }

        ///<inheritdoc/>
        public async Task DeleteAsync(User user, int id)
        {
            var annualReport = await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                    predicate: a => a.ID == id && a.Status == AnnualReportStatus.Unconfirmed);
            if (!await _cityAccessService.HasAccessAsync(user, annualReport.CityId))
            {
                throw new UnauthorizedAccessException();
            }
            _repositoryWrapper.AnnualReports.Delete(annualReport);
            await _repositoryWrapper.SaveAsync();
        }

        ///<inheritdoc/>
        public async Task<bool> CheckCreated(User user, int cityId)
        {
            var city = await _repositoryWrapper.City.GetFirstOrDefaultAsync(
                predicate: a => a.ID == cityId);
            if (!await _cityAccessService.HasAccessAsync(user, city.ID))
            {
                throw new UnauthorizedAccessException();
            }
            return await CheckCreated(city.ID);
        }

        private async Task SaveLastConfirmedAsync(int cityId)
        {
            var annualReport = await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                predicate: a => a.CityId == cityId && a.Status == AnnualReportStatus.Confirmed);
            if (annualReport != null)
            {
                annualReport.Status = AnnualReportStatus.Saved;
                _repositoryWrapper.AnnualReports.Update(annualReport);
            }
        }

        private async Task<bool> CheckCreated(int cityId)
        {
            return await _repositoryWrapper.AnnualReports.GetFirstOrDefaultAsync(
                predicate: a => a.CityId == cityId && (a.Status == AnnualReportStatus.Unconfirmed || a.Date.Year == DateTime.Now.Year)) != null;
        }
    }
}