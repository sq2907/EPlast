﻿using AutoMapper;
using EPlast.BLL.DTO.Admin;
using EPlast.BLL.DTO.Club;
using EPlast.BLL.Interfaces.Admin;
using EPlast.BLL.Services.Club;
using EPlast.DataAccess.Entities;
using EPlast.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EPlast.Tests.Services.Club
{
    [TestFixture]
    public class ClubParticipantsServiceTests
    {
        private ClubParticipantsService _clubParticipantsService;
        private Mock<IRepositoryWrapper> _repoWrapper;
        private Mock<IMapper> _mapper;
        private Mock<IAdminTypeService> _adminTypeService;
        private Mock<IUserStore<User>> _user;
        private Mock<UserManager<User>> _userManager;

        [SetUp]
        public void SetUp()
        {
            _repoWrapper = new Mock<IRepositoryWrapper>();
            _mapper = new Mock<IMapper>();
            _adminTypeService = new Mock<IAdminTypeService>();
            _user = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(_user.Object, null, null, null, null, null, null, null, null);
            _clubParticipantsService = new ClubParticipantsService(_repoWrapper.Object, _mapper.Object, _adminTypeService.Object, _userManager.Object);
        }

        [Test]
        public async Task GetAdministrationByIdAsync_ReturnsAdministrations()
        {
            // Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId } });
            _mapper
                .Setup(m => m.Map<IEnumerable<ClubAdministration>, IEnumerable<ClubAdministrationDTO>>(It.IsAny<IEnumerable<ClubAdministration>>()))
                .Returns(new List<ClubAdministrationDTO> { new ClubAdministrationDTO { ID = fakeId } });

            // Act
            var result = await _clubParticipantsService.GetAdministrationByIdAsync(It.IsAny<int>());

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.FirstOrDefault().ID, fakeId);
        }

        [Test]
        public async Task AddAdministratorAsync_ReturnsAdministrator()
        {
            //Arrange
            _repoWrapper
                .Setup(s => s.ClubAdministration.CreateAsync(clubAdm));
            _adminTypeService
                .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new AdminTypeDTO());

            //Act
            var result = await _clubParticipantsService.AddAdministratorAsync(clubAdmDTO);

            //Assert
            Assert.IsInstanceOf<ClubAdministrationDTO>(result);
        }

        [Test]
        public async Task AddAdministratorAsync_WhereStartDateIsNull_ReturnsAdministrator()
        {
            //Arrange
            _repoWrapper
                .Setup(s => s.ClubAdministration.CreateAsync(clubAdm));
            _adminTypeService
                .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new AdminTypeDTO());
            clubAdmDTO.StartDate = null;

            //Act
            var result = await _clubParticipantsService.AddAdministratorAsync(clubAdmDTO);

            //Assert
            Assert.IsInstanceOf<ClubAdministrationDTO>(result);
        }

        [Test]
        public async Task EditAdministratorAsync_ReturnsEditedAdministratorWithSameId()
        {
            //Arrange
            _adminTypeService
                .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new AdminTypeDTO());
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>,
                    IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new ClubAdministration());
            _repoWrapper
                .Setup(r => r.ClubAdministration.Update(It.IsAny<ClubAdministration>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());

            //Act
            var result = await _clubParticipantsService.EditAdministratorAsync(clubAdmDTO);

            //Assert
            _repoWrapper.Verify();
            Assert.IsInstanceOf<ClubAdministrationDTO>(result);
        }

        [Test]
        public async Task EditAdministratorAsync_WhereStartTimeIsNull_ReturnsEditedAdministratorWithSameId()
        {
            //Arrange
            _adminTypeService
                .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new AdminTypeDTO());
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>,
                    IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new ClubAdministration());
            _repoWrapper
                .Setup(r => r.ClubAdministration.Update(It.IsAny<ClubAdministration>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());
            clubAdmDTO.StartDate = null;

            //Act
            var result = await _clubParticipantsService.EditAdministratorAsync(clubAdmDTO);

            //Assert
            _repoWrapper.Verify();
            Assert.IsInstanceOf<ClubAdministrationDTO>(result);
        }

        [Test]
        public async Task EditAdministratorAsync_ReturnsEditedAdministratorWithDifferentId()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>,
                    IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new ClubAdministration());
            _adminTypeService
               .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new AdminTypeDTO
               {
                   AdminTypeName = "Голова Куреня",
                   ID = 3
               });
            _adminTypeService
                .Setup(a => a.GetAdminTypeByIdAsync(It.IsAny<int>()))
               .ReturnsAsync(new AdminTypeDTO
               {
                   AdminTypeName = "Голова Куреня",
                   ID = 3
               });

            //Act
            var result = await _clubParticipantsService.EditAdministratorAsync(clubAdmDTO);

            //Assert
            _repoWrapper.Verify();
            Assert.IsInstanceOf<ClubAdministrationDTO>(result);
        }

        [Test]
        public void RemoveAdministratorAsync_ReturnsCorrect()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>,
                    IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(clubAdm);
            _adminTypeService
                .Setup(a => a.GetAdminTypeByIdAsync(It.IsAny<int>()))
                .Returns(() => Task<AdminTypeDTO>.Factory.StartNew(() => AdminType));
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()));
            _userManager
                .Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()));
            _repoWrapper
                .Setup(r => r.ClubAdministration.Update(It.IsAny<ClubAdministration>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());

            //Act
            var result = _clubParticipantsService.RemoveAdministratorAsync(It.IsAny<int>());

            //Assert
            _repoWrapper.Verify();
            Assert.NotNull(result);
        }

        [Test]
        public void RemoveAdministratorAsync_WithAnotherRole_ReturnsCorrect()
        {
            //Arrange
            AdminType.AdminTypeName = "Another";
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>,
                    IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(clubAdm);
            _adminTypeService
                .Setup(a => a.GetAdminTypeByIdAsync(It.IsAny<int>()))
                .Returns(() => Task<AdminTypeDTO>.Factory.StartNew(() => AdminType));
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()));
            _userManager
                .Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()));
            _repoWrapper
                .Setup(r => r.ClubAdministration.Update(It.IsAny<ClubAdministration>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());

            //Act
            var result = _clubParticipantsService.RemoveAdministratorAsync(It.IsAny<int>());

            //Assert
            _repoWrapper.Verify();
            Assert.NotNull(result);
        }

        [Test]
        public void CheckPreviousAdministratorsToDelete_ReturnsCorrect()
        {
            //Arrange
            _repoWrapper.Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                 It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                     .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId } });
            _adminTypeService
               .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new AdminTypeDTO
               {
                   AdminTypeName = "Голова Куреня",
                   ID = 3
               });
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()));
            _userManager
                .Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()));

            //Act
            var result = _clubParticipantsService.CheckPreviousAdministratorsToDelete();

            //Assert
            _repoWrapper.Verify();
            Assert.NotNull(result);
        }

        [Test]
        public void CheckPreviousAdministratorsToDelete_WithDifferrentAdminTypeId_ReturnsCorrect()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId, AdminTypeId = fakeId } })
                .Callback(() => _repoWrapper
                    .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                        It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                    .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId, AdminTypeId = anotherFakeId } }));
            _adminTypeService
               .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new AdminTypeDTO
               {
                   AdminTypeName = "Голова Куреня",
                   ID = 3
               });
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()));
            _userManager
                .Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()));

            //Act
            var result = _clubParticipantsService.CheckPreviousAdministratorsToDelete();

            //Assert
            _repoWrapper.Verify();
            _userManager.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Once);
            _userManager.Verify(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            Assert.NotNull(result);
        }

        [Test]
        public void CheckPreviousAdministratorsToDelete_WithDifferrentIDAndAdminTypeId_ReturnsCorrect()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId, AdminTypeId = fakeId } })
                .Callback(() => _repoWrapper
                    .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                        It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                    .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId, AdminTypeId = anotherFakeId } }));
            _adminTypeService
               .Setup(a => a.GetAdminTypeByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new AdminTypeDTO
               {
                   AdminTypeName = "Голова Куреня",
                   ID = 2
               });
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()));
            _userManager
                .Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()));

            //Act
            var result = _clubParticipantsService.CheckPreviousAdministratorsToDelete();

            //Assert
            _repoWrapper.Verify();
            _userManager.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Once);
            _userManager.Verify(u => u.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            Assert.NotNull(result);
        }

        [Test]
        public async Task GetAdministrationsOfUserAsync_ReturnsCorrectAdministrations()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId } });
            _mapper
                .Setup(m => m.Map<IEnumerable<ClubAdministration>, IEnumerable<ClubAdministrationDTO>>(It.IsAny<IEnumerable<ClubAdministration>>()))
                .Returns(GetTestClubAdministration());

            //Act
            var result = await _clubParticipantsService.GetAdministrationsOfUserAsync(It.IsAny<string>());

            //Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<ClubAdministrationDTO>>(result);
        }

        [Test]
        public async Task GetAdministrationsOfUserAsync_WithClub_ReturnsCorrectAdministrations()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() 
                { 
                    ID = fakeId,
                    Club = new DataAccess.Entities.Club()
                } });
            _mapper
                .Setup(m => m.Map<IEnumerable<ClubAdministration>, IEnumerable<ClubAdministrationDTO>>(It.IsAny<IEnumerable<ClubAdministration>>()))
                .Returns(GetTestClubAdministration());

            //Act
            var result = await _clubParticipantsService.GetAdministrationsOfUserAsync(It.IsAny<string>());

            //Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<ClubAdministrationDTO>>(result);
        }

        [Test]
        public async Task GetPreviousAdministrationsOfUserAsync_ReturnsCorrectAdministrations()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration()
                {
                    ID = fakeId,
                    Club = new DataAccess.Entities.Club ()
                } });
            _mapper
                .Setup(m => m.Map<IEnumerable<ClubAdministration>, IEnumerable<ClubAdministrationDTO>>(It.IsAny<IEnumerable<ClubAdministration>>()))
                .Returns(GetTestClubAdministration());

            //Act
            var result = await _clubParticipantsService.GetPreviousAdministrationsOfUserAsync(It.IsAny<string>());

            //Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<ClubAdministrationDTO>>(result);
        }

        [Test]
        public async Task GetAdministrationStatuses_ReturnsCorrectAdministrationStatuses()
        {
            //Arrange
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration()
                {
                    ID = fakeId
                } });
            _mapper
                .Setup(m => m.Map<IEnumerable<ClubAdministration>, IEnumerable<ClubAdministrationStatusDTO>>(It.IsAny<IEnumerable<ClubAdministration>>()))
                .Returns(GetTestClubAdministrationStatuses());

            //Act
            var result = await _clubParticipantsService.GetAdministrationStatuses(It.IsAny<string>());

            //Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<ClubAdministrationStatusDTO>>(result);
        }

        [Test]
        public async Task GetMembersByClubIdAsync_ReturnsMembers()
        {
            // Arrange
            _repoWrapper.Setup(r => r.ClubMembers.GetAllAsync(It.IsAny<Expression<Func<ClubMembers, bool>>>(),
            It.IsAny<Func<IQueryable<ClubMembers>, IIncludableQueryable<ClubMembers, object>>>()))
            .ReturnsAsync(new List<ClubMembers> { new ClubMembers() }); ;

            // Act
            var result = await _clubParticipantsService.GetMembersByClubIdAsync(It.IsAny<int>());

            // Assert
            Assert.NotNull(result);
            _mapper.Verify(m => m.Map<IEnumerable<ClubMembers>, IEnumerable<ClubMembersDTO>>(It.IsAny<IEnumerable<ClubMembers>>()));
        }

        [Test]
        public async Task AddFollowerAsync_ReturnsMembers()
        {
            // Arrange
            _repoWrapper
                .Setup(s => s.ClubMembers.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubMembers, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubMembers>, IIncludableQueryable<ClubMembers, object>>>()))
                .ReturnsAsync(new ClubMembers());
            _repoWrapper
                .Setup(r => r.ClubMembers.Delete(It.IsAny<ClubMembers>()));
            _repoWrapper
                .Setup(r => r.ClubMembers.CreateAsync(It.IsAny<ClubMembers>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());
            _repoWrapper
                .Setup(r => r.ClubMembers.GetAllAsync(It.IsAny<Expression<Func<ClubMembers, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubMembers>, IIncludableQueryable<ClubMembers, object>>>()))
                .ReturnsAsync(new List<ClubMembers> { new ClubMembers() });
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId } });
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new ClubAdministration());
            _adminTypeService
                .Setup(a => a.GetAdminTypeByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdminTypeDTO());
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            _mapper
                .Setup(m => m.Map<ClubMembers, ClubMembersDTO>(It.IsAny<ClubMembers>()))
                .Returns(new ClubMembersDTO());

            // Act
            var result = await _clubParticipantsService.AddFollowerAsync(fakeId, fakeIdString);

            // Assert
            Assert.NotNull(result);
            _repoWrapper.Verify(r => r.SaveAsync(), Times.Exactly(3));
        }

        [Test]
        public async Task AddFollowerAsync_WhereOldClubMemberIsNull_ReturnsMembers()
        {
            // Arrange
            _repoWrapper
                .Setup(s => s.ClubMembers.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubMembers, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubMembers>, IIncludableQueryable<ClubMembers, object>>>()))
                .ReturnsAsync((ClubMembers)null);
            _repoWrapper
                .Setup(r => r.ClubMembers.Delete(It.IsAny<ClubMembers>()));
            _repoWrapper
                .Setup(r => r.ClubMembers.CreateAsync(It.IsAny<ClubMembers>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());
            _repoWrapper
                .Setup(r => r.ClubMembers.GetAllAsync(It.IsAny<Expression<Func<ClubMembers, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubMembers>, IIncludableQueryable<ClubMembers, object>>>()))
                .ReturnsAsync(new List<ClubMembers> { new ClubMembers() });
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetAllAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new List<ClubAdministration> { new ClubAdministration() { ID = fakeId } });
            _repoWrapper
                .Setup(r => r.ClubAdministration.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubAdministration, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubAdministration>, IIncludableQueryable<ClubAdministration, object>>>()))
                .ReturnsAsync(new ClubAdministration());
            _adminTypeService
                .Setup(a => a.GetAdminTypeByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdminTypeDTO());
            _userManager
                .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            _mapper
                .Setup(m => m.Map<ClubMembers, ClubMembersDTO>(It.IsAny<ClubMembers>()))
                .Returns(new ClubMembersDTO());

            // Act
            var result = await _clubParticipantsService.AddFollowerAsync(fakeId, fakeIdString);

            // Assert
            Assert.NotNull(result);
            _repoWrapper.Verify(r => r.SaveAsync(), Times.Exactly(2));
        }

        [Test]
        public async Task ToggleApproveStatusAsyncTest()
        {
            //Arrange
            _repoWrapper
                .Setup(s => s.ClubMembers.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ClubMembers, bool>>>(),
                    It.IsAny<Func<IQueryable<ClubMembers>, IIncludableQueryable<ClubMembers, object>>>()))
                .ReturnsAsync(new ClubMembers());

            //Act
            await _clubParticipantsService.ToggleApproveStatusAsync(It.IsAny<int>());

            //Assert
            _repoWrapper.Verify(i => i.ClubMembers.Update(It.IsAny<ClubMembers>()), Times.Once());
        }

        [Test]
        public async Task RemoveFollowerAsync()
        {
            // Arrange
            _repoWrapper
                .Setup(r => r.ClubMembers.Delete(It.IsAny<ClubMembers>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());
            // Act
            await _clubParticipantsService.RemoveFollowerAsync(It.IsAny<int>());

            // Assert
            _repoWrapper.Verify(r => r.ClubMembers.Delete(It.IsAny<ClubMembers>()), Times.Once());
            _repoWrapper.Verify(r => r.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task RemoveMemberAsync()
        {
            // Arrange
            _repoWrapper
                .Setup(r => r.ClubMembers.Delete(It.IsAny<ClubMembers>()));
            _repoWrapper
                .Setup(r => r.SaveAsync());

            // Act
            await _clubParticipantsService.RemoveMemberAsync(It.IsAny<ClubMembers>());

            // Assert
            _repoWrapper.Verify(r => r.ClubMembers.Delete(It.IsAny<ClubMembers>()), Times.Once());
            _repoWrapper.Verify(r => r.SaveAsync(), Times.Once());
        }

        private IEnumerable<ClubAdministrationDTO> GetTestClubAdministration()
        {
            return new List<ClubAdministrationDTO>
            {
                new ClubAdministrationDTO{UserId = "Голова Куреня"},
                new ClubAdministrationDTO{UserId = "Голова Куреня"}
            }.AsEnumerable();
        }

        private IEnumerable<ClubAdministrationStatusDTO> GetTestClubAdministrationStatuses()
        {
            return new List<ClubAdministrationStatusDTO>
            {
                new ClubAdministrationStatusDTO{UserId = "Голова Куреня"},
                new ClubAdministrationStatusDTO{UserId = "Голова Куреня"}
            }.AsEnumerable();
        }

        private static AdminTypeDTO AdminType = new AdminTypeDTO
        {
            AdminTypeName = "Голова Куреня",
            ID = 1
        };

        private ClubAdministrationDTO clubAdmDTO = new ClubAdministrationDTO
        {
            ID = 1,
            AdminType = AdminType,
            ClubId = 1,
            AdminTypeId = 1,
            EndDate = DateTime.Today,
            StartDate = DateTime.Now,
            User = new ClubUserDTO(),
            UserId = "Голова Куреня"
        };

        private ClubAdministration clubAdm = new ClubAdministration
        {
            ID = 1,
            AdminType = new AdminType()
            {
                AdminTypeName = "Голова Куреня",
                ID = 1
            },
            AdminTypeId = AdminType.ID,
            UserId = "Голова Куреня"
        };

        private ClubAdministrationDTO clubFakeAdmDTO = new ClubAdministrationDTO
        {
            ID = 2,
            AdminType = AdminType,
            ClubId = 2,
            AdminTypeId = 2,
            EndDate = DateTime.Today,
            StartDate = DateTime.Now,
            User = new ClubUserDTO(),
            UserId = "Голова Куреня"
        };

        private int fakeId => 3;
        private int anotherFakeId => 2;
        private string fakeIdString => "1";
    }

}
