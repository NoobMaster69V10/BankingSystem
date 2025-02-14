using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AtmController : ControllerBase
{
    private readonly IAtmService _atmService;

    public AtmController(IAtmService atmService)
    {
        _atmService = atmService;
    }
    
    [HttpPost("authorize-card")]
    public async Task<ActionResult<ApiResponse>> Authorize(CardAuthorizationDto cardAuthorizationDto)
    {
        var response = await _atmService.AuthorizeCardAsync(cardAuthorizationDto);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}