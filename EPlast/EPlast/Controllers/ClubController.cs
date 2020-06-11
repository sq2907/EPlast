﻿using AutoMapper;
using EPlast.BussinessLayer.DTO;
using EPlast.BussinessLayer.DTO.Club;
using EPlast.BussinessLayer.Interfaces.Club;
using EPlast.BussinessLayer.Services.Interfaces;
using EPlast.ViewModels;
using EPlast.ViewModels.Club;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPlast.Controllers
{
    public class ClubController : Controller
    {
        private readonly IClubService _clubService;
        private readonly IClubAdministrationService _clubAdministrationService;
        private readonly IClubMembersService _clubMembersService;
        private readonly IMapper _mapper;
        private readonly ILoggerService<ClubController> _logger;
        private readonly IUserManagerService _userManagerService;

        public ClubController(IClubService clubService, IClubAdministrationService clubAdministrationService, IClubMembersService clubMembersService, IMapper mapper, ILoggerService<ClubController> logger, IUserManagerService userManagerService)
        {
            _clubService = clubService;
            _clubAdministrationService = clubAdministrationService;
            _clubMembersService = clubMembersService;
            _mapper = mapper;
            _logger = logger;
            _userManagerService = userManagerService;
        }

        private void CheckCurrentUserRoles(ref ClubProfileViewModel viewModel)
        {
            viewModel.IsCurrentUserClubAdmin = _userManagerService.GetUserId(User) == viewModel.ClubAdmin?.Id;
            viewModel.IsCurrentUserAdmin = User.IsInRole("Admin");
        }
        
        public IActionResult Index()
        {
            var clubs = _clubService.GetAllClubs();
            var viewModels = _mapper.Map<IEnumerable<ClubDTO>, IEnumerable<ClubViewModel>>(clubs);

            return View(viewModels);
        }

        public IActionResult Club(int index)
        {
            try
            {
                var viewModel = _mapper.Map<ClubProfileDTO, ClubProfileViewModel>(_clubService.GetClubProfile(index));
                CheckCurrentUserRoles(ref viewModel);

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        public async Task<IActionResult> ClubAdmins(int index)
        {
            try
            {
                var viewModel = _mapper.Map<ClubProfileDTO, ClubProfileViewModel>(await _clubAdministrationService
                    .GetCurrentClubAdministrationByIDAsync(index));

                CheckCurrentUserRoles(ref viewModel);

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        public IActionResult ClubMembers(int index)
        {
            try
            {
                var viewModel = _mapper.Map<ClubProfileDTO, ClubProfileViewModel>(_clubService.GetClubMembersOrFollowers(index, true));
                CheckCurrentUserRoles(ref viewModel);

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        public IActionResult ClubFollowers(int index)
        {
            try
            {
                var viewModel = _mapper.Map<ClubProfileDTO, ClubProfileViewModel>(_clubService.GetClubMembersOrFollowers(index, false));
                CheckCurrentUserRoles(ref viewModel);

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        public IActionResult ClubDescription(int index)
        {
            try
            {
                var viewModel = _mapper.Map<ClubDTO, ClubViewModel>(_clubService.GetClubInfoById(index));

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        [HttpGet]
        public IActionResult EditClub(int index)
        {
            try
            {
                var viewModel = _mapper.Map<ClubDTO, ClubViewModel>(_clubService.GetClubInfoById(index));

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        [HttpPost]
        public IActionResult EditClub(ClubViewModel model, IFormFile file)
        {
            try
            {
                _clubService.Update(_mapper.Map<ClubViewModel, ClubDTO>(model), file);

                return RedirectToAction("Club", new { index = model.ID });
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeIsApprovedStatus(int index, int clubIndex)
        {
            try
            {
                await _clubMembersService.ToggleIsApprovedInClubMembersAsync(index, clubIndex);

                return RedirectToAction("ClubMembers", new { index = clubIndex });
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeIsApprovedStatusFollowers(int index, int clubIndex)
        {
            try
            {
                await _clubMembersService.ToggleIsApprovedInClubMembersAsync(index, clubIndex);

                return RedirectToAction("ClubFollowers", new { index = clubIndex });
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");

                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeIsApprovedStatusClub(int index, int clubIndex)
        {
            try
            {
                await _clubMembersService.ToggleIsApprovedInClubMembersAsync(index, clubIndex);

                return RedirectToAction("Club", new { index = clubIndex });
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> DeleteFromAdmins(int adminId, int clubIndex)
        {
            bool isSuccessfull = await _clubAdministrationService.DeleteClubAdminAsync(adminId);

            if (isSuccessfull)
            {
                return RedirectToAction("ClubAdmins", new { index = clubIndex });
            }

            return RedirectToAction("HandleError", "Error", new { code = 505 });
        }

        [HttpPost]
        public async Task<int> AddEndDate([FromBody] AdminEndDateDTO adminEndDate)
        {
            try
            {
                await _clubAdministrationService.SetAdminEndDateAsync(adminEndDate);

                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToClubAdministration([FromBody] ClubAdministrationDTO createdAdmin)
        {
            try
            {
                await _clubAdministrationService.AddClubAdminAsync(createdAdmin);

                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public IActionResult ChooseAClub(string userId)
        {
            var clubs = _mapper.Map<IEnumerable<ClubDTO>, IEnumerable<ClubViewModel>>(_clubService.GetAllClubs());
            var model = new ClubChooseAClubViewModel
            {
                Clubs = clubs,
                UserId = userId
            };
            return View(model);
        }

        public async Task<IActionResult> AddAsClubFollower(int clubIndex, string userId)
        {
            userId = User.IsInRole("Admin") ? userId : _userManagerService.GetUserId(User);

            await _clubMembersService.AddFollowerAsync(clubIndex, userId);

            return RedirectToAction("UserProfile", "Account", userId);
        }

        [HttpGet]
        public IActionResult CreateClub()
        {
            try
            {
                return View(new ClubViewModel());
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }

        [HttpPost]
        public IActionResult CreateClub(ClubViewModel model, IFormFile file)
        {
            try
            {
                var club = _clubService.Create(_mapper.Map<ClubViewModel, ClubDTO>(model), file);

                return RedirectToAction("Club", new { index = club.ID });
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception :{e.Message}");
                return RedirectToAction("HandleError", "Error", new { code = 505 });
            }
        }
    }
}