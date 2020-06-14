﻿using AutoMapper;
using EPlast.BussinessLayer.DTO.UserProfiles;
using EPlast.BussinessLayer.Services.Interfaces;
using EPlast.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPlast.BussinessLayer.Services
{
    public class UserManagerService : IUserManagerService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserManagerService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<bool> IsInRoleAsync(UserDTO user, params string[] roles)
        {

            var userFirst = _mapper.Map<UserDTO, User>(user);

            foreach (var i in roles)
            {
                if (await _userManager.IsInRoleAsync(userFirst, i))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> IsInRoleAsync(ClaimsPrincipal user, params string[] roles)
        {

            var userFirst = await _userManager.GetUserAsync(user);

            foreach (var i in roles)
            {
                if (await _userManager.IsInRoleAsync(userFirst, i))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<string> GetUserIdAsync(ClaimsPrincipal user)
        {
            var id = await _userManager.GetUserIdAsync(await _userManager.GetUserAsync(user));
            return id;
        }

        public async Task<UserDTO> FindByIdAsync(string userId)
        {
            var result = _mapper.Map<User, UserDTO>(await _userManager.FindByIdAsync(userId));
            return result;
        }
        public async Task<IEnumerable<string>> GetRolesAsync(UserDTO user)
        {
            var result = await _userManager.GetRolesAsync(_mapper.Map<UserDTO, User>(user));
            return result;
        }
    }
}
