//using BankingSystem.API.Controllers;
//using BankingSystem.Core.ServiceContracts;
//using Moq;

//namespace BankingSystem.Tests.Controllers;

//public class PersonControllerTests
//{
//    private readonly Mock<IPersonAuthService> _personAuthServiceMock = new();
//    private readonly PersonController _controller;

//    public PersonControllerTests()
//    {
//        _controller = new PersonController(
//            _personAuthServiceMock.Object);
//    }

//    //private void SetUserContext(string personId)
//    //{
//    //    var claims = new[] { new Claim("personId", personId) };
//    //    var identity = new ClaimsIdentity(claims, "TestAuth");
//    //    var claimsPrincipal = new ClaimsPrincipal(identity);
//    //    _controller.ControllerContext = new ControllerContext
//    //    {
//    //        HttpContext = new DefaultHttpContext { User = claimsPrincipal }
//    //    };
//    //}

//    //[Fact]
//    //public async Task GetPersonInfo_ReturnsOk_WhenPersonExists()
//    //{
//    //    SetUserContext("123");
//    //    var personDto = new Person();
//    //    var expectedResult = Result<Person>.Success(personDto);

//    //    _personServiceMock.Setup(s => s.GetPersonById("123")).ReturnsAsync(expectedResult);

//    //    var result = await _controller.GetPersonInfo();

//    //    var okResult = Assert.IsType<OkObjectResult>(result);
//    //    Assert.Equal(personDto, okResult.Value);
//    //}

//    //[Fact]
//    //public async Task GetPersonInfo_ReturnsBadRequest_WhenPersonIsNull()
//    //{
//    //    SetUserContext("123");
//    //    var personDto = new Person();
//    //    var expectedResult = Result<Person>.Failure(CustomError.NotFound(""));

//    //    _personServiceMock.Setup(s => s.GetPersonById("123")).ReturnsAsync(expectedResult);

//    //    var result = await _controller.GetPersonInfo();

//    //    Assert.IsNotType<OkObjectResult>(result);
//    //}

//    //[Fact]
//    //public async Task RegisterUser_ReturnsCreated_WhenRegistrationSuccessful()
//    //{
//    //    var registerDto = new PersonRegisterDto();
//    //    var personResult = new RegisterResponse();
//    //    var expectedResult = Result<RegisterResponse>.Success(personResult);

//    //    _personAuthServiceMock.Setup(s => s.RegisterPersonAsync(registerDto))
//    //        .ReturnsAsync(expectedResult);

//    //    var result = await _controller.RegisterUser(registerDto);
//    //    var createdResult = Assert.IsType<CreatedResult>(result);
//    //    Assert.Equal("register", createdResult.Location);
//    //}

//    //[Fact]
//    //public async Task RegisterUser_ReturnsBadRequest_WhenRegistrationIsNotSuccessful()
//    //{
//    //    var registerDto = new PersonRegisterDto();
//    //    var personResult = new RegisterResponse();
//    //    var expectedResult = Result<RegisterResponse>.Failure(CustomError.Validation(""));

//    //    _personAuthServiceMock.Setup(s => s.RegisterPersonAsync(registerDto))
//    //        .ReturnsAsync(expectedResult);

//    //    var result = await _controller.RegisterUser(registerDto);
//    //     Assert.IsNotType<CreatedResult>(result);
//    //}

//    //[Fact]
//    //public async Task LoginPerson_ReturnsCreated_WithToken()
//    //{
//    //    var loginDto = new PersonLoginDto();
//    //    var expectedResult = Result<string>.Success("jwt-token");

//    //    _personAuthServiceMock.Setup(s => s.AuthenticationPersonAsync(loginDto))
//    //        .ReturnsAsync(expectedResult);

//    //    var result = await _controller.LoginPerson(loginDto);

//    //    Assert.IsType<CreatedResult>(result);
//    //}

//    //[Fact]
//    //public async Task LoginPerson_ReturnsBadRequest_WhenLoginIsNotSuccessful()
//    //{
//    //    var loginDto = new PersonLoginDto();
//    //    var expectedResult = Result<string>.Failure(CustomError.Validation(""));

//    //    _personAuthServiceMock.Setup(s => s.AuthenticationPersonAsync(loginDto))
//    //        .ReturnsAsync(expectedResult);

//    //    var result = await _controller.LoginPerson(loginDto);

//    //    Assert.IsNotType<CreatedResult>(result);
//    //}
//}