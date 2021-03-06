﻿using AutoMapper;
using EPlast.BLL.DTO.City;
using EPlast.BLL.Interfaces.City;
using EPlast.BLL.Interfaces.Logging;
using EPlast.DataAccess.Entities;
using EPlast.WebApi.Controllers;
using EPlast.WebApi.Models.City;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPlast.Tests.Controllers
{

    class CityControllerTests
    {

        private readonly Mock<ICityService> _cityService;
        private readonly Mock<ICityParticipantsService> _cityParticipantsService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILoggerService<CitiesController>> _logger;
        private readonly Mock<ICityAccessService> _cityAccessService;
        private readonly Mock<ICityDocumentsService> _cityDocumentsService;
        private readonly Mock<Microsoft.AspNetCore.Identity.UserManager<User>> _userManager;


        public CityControllerTests()
        {
            _cityAccessService = new Mock<ICityAccessService>();
            _cityService = new Mock<ICityService>();
            _cityParticipantsService = new Mock<ICityParticipantsService>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILoggerService<CitiesController>>();
            _cityDocumentsService = new Mock<ICityDocumentsService>();
            var store = new Mock<Microsoft.AspNetCore.Identity.IUserStore<User>>();
            _userManager = new Mock<Microsoft.AspNetCore.Identity.UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private CitiesController CreateCityController => new CitiesController(_logger.Object,
             _mapper.Object,
           _cityService.Object,
             _cityDocumentsService.Object,
           _cityAccessService.Object,
           _userManager.Object,
        _cityParticipantsService.Object
          );

        [TestCase(1, 1, "Львів")]
        public async Task GetCities_Valid_Test(int page, int pageSize, string cityName)
        {
            // Arrange
            CitiesController citycon = CreateCityController;
            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User.IsInRole("Admin"))
                .Returns(true);
            var context = new ControllerContext(
                new ActionContext(
                    httpContext.Object, new RouteData(),
                    new ControllerActionDescriptor()));
            citycon.ControllerContext = context;
            _cityService
                .Setup(c => c.GetAllDTOAsync(It.IsAny<string>()))
                .ReturnsAsync(GetCitiesBySearch());

            // Act
            var result = await citycon.GetCities(page, pageSize, cityName);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(((result as ObjectResult).Value as CitiesViewModel)
                .Cities.Where(c => c.Name.Equals("Львів")));
        }

        [Test]
        public async Task GetCities_Valid_Test()
        {
            // Arrange
            CitiesController citycon = CreateCityController;
            var httpContext = new Mock<HttpContext>();
            var context = new ControllerContext(
                new ActionContext(
                    httpContext.Object, new RouteData(),
                    new ControllerActionDescriptor()));
            citycon.ControllerContext = context;
            _cityService
                .Setup(c => c.GetCities())
                .ReturnsAsync(GetFakeCitiesForAdministration());

            // Act
            var result = await citycon.GetCities();

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(((result as ObjectResult).Value as List<CityForAdministrationDTO>)
                .Where(n => n.Name.Equals("Львів")));
        }

        [TestCase(2)]
        public async Task GetMembers_Valid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(cs => cs.GetCityMembersAsync(It.IsAny<int>()))
                .ReturnsAsync(new CityProfileDTO());
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController cityController = CreateCityController;

            // Act
            var result = await cityController.GetMembers(id);

            // Assert
            _mapper.Verify(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()));
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.NotNull(result);
        }

        [TestCase(2)]
        public async Task GetProfile_Valid_Test(int id)
        {

            _cityService.Setup(c => c.GetCityProfileAsync(It.IsAny<int>(), It.IsAny<User>()))
                .ReturnsAsync(new CityProfileDTO());
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetProfile(id);

            // Assert
            _mapper.Verify(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()));
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(2)]
        public async Task GetFollowers_Valid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(c => c.GetCityFollowersAsync(It.IsAny<int>()))
                .ReturnsAsync(new CityProfileDTO());
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetFollowers(id);

            // Assert
            _mapper.Verify(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()));
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(2)]
        public async Task GetFollowers_Invalid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(c => c.GetCityFollowersAsync(It.IsAny<int>()))
                .ReturnsAsync(() => null);
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetFollowers(id);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [TestCase(2)]
        public async Task GetProfile_Invalid_Test(int id)
        {

            _cityService.Setup(c => c.GetCityProfileAsync(It.IsAny<int>(), It.IsAny<User>()))
                .ReturnsAsync(() => null);
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetProfile(id);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [TestCase(2)]
        public async Task GetMembers_Invalid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(cs => cs.GetCityMembersAsync(It.IsAny<int>()))
                .ReturnsAsync(() => null);
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController cityController = CreateCityController;

            // Act
            var result = await cityController.GetMembers(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task GetProfile_Invalid_Test()
        {

            _cityService.Setup(c => c.GetCityProfileAsync(It.IsAny<int>(), It.IsAny<User>()))
                .ReturnsAsync(() => null);
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetProfile(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [TestCase(2)]
        public async Task GetAdmins_Valid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(c => c.GetCityAdminsAsync(It.IsAny<int>()))
                .ReturnsAsync(new CityProfileDTO());
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetAdmins(id);

            // Assert
            _mapper.Verify(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()));
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(2)]
        public async Task GetAdmins_Invalid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(c => c.GetCityAdminsAsync(It.IsAny<int>()))
                .ReturnsAsync(() => null);
            _mapper.Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetAdmins(id);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [TestCase(2)]
        public async Task GetDocuments_Valid_Test(int id)
        {
            // Arrange
            _cityService
                .Setup(c => c.GetCityDocumentsAsync(It.IsAny<int>()))
                .ReturnsAsync(new CityProfileDTO());
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetDocuments(id);

            // Assert
            _mapper.Verify(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()));
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetDocumentsInvalidCheck()
        {
            // Arrange
            _cityService
                .Setup(c => c.GetCityDocumentsAsync(It.IsAny<int>()))
                .ReturnsAsync(() => null);
            _mapper
                .Setup(m => m.Map<CityProfileDTO, CityViewModel>(It.IsAny<CityProfileDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetDocuments(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Details_Valid_Test()
        {
            // Arrange
            _cityService
                .Setup(c => c.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new CityDTO());
            _mapper
                .Setup(m => m.Map<CityDTO, CityViewModel>(It.IsAny<CityDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.Details(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Details_Invalid_Test()
        {
            // Arrange
            _cityService
                .Setup(c => c.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() => null);
            _mapper
                .Setup(m => m.Map<CityDTO, CityViewModel>(It.IsAny<CityDTO>()))
                .Returns(new CityViewModel());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.Details(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [TestCase("logoName")]
        public async Task GetPhotoBase64_Valid_Test(string logoName)
        {
            // Arrange
            _cityService
                .Setup(c => c.GetLogoBase64(It.IsAny<string>()))
                .ReturnsAsync(new string("some string"));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetPhotoBase64(logoName);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Create_Valid_Test()
        {
            // Arrange
            CityViewModel TestVM = new CityViewModel();
            _cityService
                .Setup(c => c.CreateAsync(It.IsAny<CityDTO>()))
                .ReturnsAsync(new int());
            _mapper
                .Setup(m => m.Map<CityViewModel, CityDTO>(It.IsAny<CityViewModel>()))
                .Returns(new CityDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.Create(TestVM);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Create_InvalidModelState_Valid_Test()
        {
            // Arrange
            CityViewModel TestVM = new CityViewModel();
            _cityService
                .Setup(c => c.CreateAsync(It.IsAny<CityDTO>()))
                .ReturnsAsync(new int());
            _mapper
                .Setup(m => m.Map<CityViewModel, CityDTO>(It.IsAny<CityViewModel>()))
                .Returns(new CityDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;
            citycon.ModelState.AddModelError("NameError", "Required");

            // Act
            var result = await citycon.Create(TestVM);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Edit_InvalidModelState_Valid_Test()
        {
            // Arrange
            CityViewModel TestVM = new CityViewModel();
            _cityService
                .Setup(c => c.EditAsync(It.IsAny<CityDTO>()));
            _mapper
                .Setup(m => m.Map<CityViewModel, CityDTO>(It.IsAny<CityViewModel>()))
                .Returns(new CityDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;
            citycon.ModelState.AddModelError("NameError", "Required");

            // Act
            var result = await citycon.Edit(TestVM);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Edit_Valid_Test()
        {
            // Arrange
            CityViewModel TestVM = new CityViewModel();
            _cityService
                .Setup(c => c.EditAsync(It.IsAny<CityDTO>()));
            _mapper
                .Setup(m => m.Map<CityViewModel, CityDTO>(It.IsAny<CityViewModel>()))
                .Returns(new CityDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.Edit(TestVM);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task AddFollower_Valid_Test()
        {
            _cityParticipantsService.Setup(c => c.AddFollowerAsync(It.IsAny<int>(), It.IsAny<User>()))
                .ReturnsAsync(new CityMembersDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.AddFollower(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AddFollowerWithId_Valid_Test()
        {
            // Arrange
            _cityParticipantsService
                .Setup(c => c.AddFollowerAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new CityMembersDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.AddFollowerWithId(GetFakeID(), GetStringFakeId());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Remove_Valid_Test()
        {
            // Arrange
            _cityService
                .Setup(c => c.RemoveAsync(It.IsAny<int>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.Remove(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task RemoveFollower_Valid_Test()
        {
            // Arrange
            _cityParticipantsService
                .Setup(c => c.RemoveFollowerAsync(It.IsAny<int>()));
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.RemoveFollower(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task ChangeApproveStatus_Valid_Test()
        {
            // Arrange
            _cityParticipantsService
                .Setup(c => c.ToggleApproveStatusAsync(It.IsAny<int>()))
                .ReturnsAsync(new CityMembersDTO());
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.ChangeApproveStatus(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AddAdmin_Valid_Test()
        {
            // Arrange
            CityAdministrationViewModel admin = new CityAdministrationViewModel();
            _mapper
                .Setup(m => m.Map<CityAdministrationViewModel, CityAdministrationDTO>(It.IsAny<CityAdministrationViewModel>()))
                .Returns(new CityAdministrationDTO() { AdminType = new BLL.DTO.Admin.AdminTypeDTO() });
            _cityParticipantsService
                .Setup(c => c.AddAdministratorAsync(It.IsAny<CityAdministrationDTO>()));
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.AddAdmin(admin);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task RemoveAdmin_Valid_Test()
        {
            // Arrange
            _mapper
                .Setup(m => m.Map<CityAdministrationViewModel, CityAdministrationDTO>(It.IsAny<CityAdministrationViewModel>()))
                .Returns(new CityAdministrationDTO());
            _cityParticipantsService
                .Setup(c => c.RemoveAdministratorAsync(It.IsAny<int>()));
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            _cityParticipantsService.Setup(c => c.EditAdministratorAsync(It.IsAny<CityAdministrationDTO>()));
            // Act
            var result = await citycon.RemoveAdmin(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task EditAdmin_Valid_Test()
        {
            // Arrange
            CityAdministrationViewModel admin = new CityAdministrationViewModel();
            _mapper
                .Setup(m => m.Map<CityAdministrationViewModel, CityAdministrationDTO>(It.IsAny<CityAdministrationViewModel>()))
                .Returns(new CityAdministrationDTO());
            _cityParticipantsService
                .Setup(c => c.EditAdministratorAsync(It.IsAny<CityAdministrationDTO>()));
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.EditAdmin(admin);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AddDocument_Valid_Test()
        {
            // Arrange
            CityDocumentsViewModel document = new CityDocumentsViewModel();
            _mapper
                .Setup(m => m.Map<CityDocumentsViewModel, CityDocumentsDTO>(It.IsAny<CityDocumentsViewModel>()))
                .Returns(new CityDocumentsDTO());
            _cityDocumentsService
                .Setup(c => c.AddDocumentAsync(It.IsAny<CityDocumentsDTO>()));
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.AddDocument(document);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetFileBase64_Valid_Test()
        {
            // Arrange
            _cityDocumentsService
                .Setup(c => c.DownloadFileAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<string>());
            CitiesController citycon = CreateCityController;

            _cityParticipantsService.Setup(c => c.RemoveAdministratorAsync(It.IsAny<int>()));
            // Act
            var result = await citycon.GetFileBase64(GetFakeFileName());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task RemoveDocument_Valid_Test()
        {
            // Arrange
            _cityDocumentsService
                .Setup(c => c.DeleteFileAsync(It.IsAny<int>()));
            _logger
                .Setup(l => l.LogInformation(It.IsAny<string>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.RemoveDocument(GetFakeID());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }


        [Test]
        public async Task GetDocumentTypesAsync_Valid_Test()
        {
            // Arrange
            _cityDocumentsService
                .Setup(c => c.GetAllCityDocumentTypesAsync())
                .ReturnsAsync(It.IsAny<IEnumerable<CityDocumentTypeDTO>>());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetDocumentTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public void GetLegalStatuses_Valid_Test()
        {
            // Arrange
            CitiesController citycon = CreateCityController;

            // Act
            var result = citycon.GetLegalStatuses();

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetCitiesThatUserHasAccessTo_Valid_Test()
        {
            // Arrange
            _cityAccessService
                .Setup(c => c.GetCitiesAsync(It.IsAny<User>()));
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetCitiesThatUserHasAccessTo();

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetUserAdministrations_Valid_Test()
        {
            // Arrange
            _cityParticipantsService
                .Setup(c => c.GetAdministrationsOfUserAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<IEnumerable<CityAdministrationDTO>>());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetUserAdministrations(GetStringFakeId());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetUserPreviousAdministrations_Valid_Test()
        {
            // Arrange
            _cityParticipantsService
                .Setup(c => c.GetPreviousAdministrationsOfUserAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<IEnumerable<CityAdministrationDTO>>());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetUserPreviousAdministrations(GetStringFakeId());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetAllAdministrationStatuses_Valid_Test()
        {
            // Arrange
            _cityParticipantsService
                .Setup(c => c.GetAdministrationStatuses(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<IEnumerable<CityAdministrationStatusDTO>>());
            CitiesController citycon = CreateCityController;

            // Act
            var result = await citycon.GetAllAdministrationStatuses(GetStringFakeId());

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        private int GetFakeID()
        {
            return 1;
        }
        private string GetStringFakeId()
        {
            return "1";
        }
        private string GetFakeFileName()
        {
            return "FileName";
        }

        private List<CityDTO> GetCitiesBySearch()
        {
            return new List<CityDTO>()
            {
                new CityDTO()
                {
                    Name = "Львів",
                }
            };
        }

        private IEnumerable<CityForAdministrationDTO> GetFakeCitiesForAdministration()
        {
            return new List<CityForAdministrationDTO>()
            {
                new CityForAdministrationDTO
                {
                    Name = "Львів"
                }
            }.AsEnumerable();
        }
    }
}

