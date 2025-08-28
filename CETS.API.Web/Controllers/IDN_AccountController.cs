using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_AccountController : ControllerBase
    {
        private readonly ILogger<IDN_AccountController> _logger;
        private readonly IIDN_AccountService _accountService;

        public IDN_AccountController(ILogger<IDN_AccountController> logger, IIDN_AccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetAccountStatusesAsync()
        {
            var statuses = await _accountService.GetAccountStatusesAsync();
            return Ok(statuses);
        }
    }
}
