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
            var statuses = await _accountService.GetStatusesAsync();
            return Ok(statuses);
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAllAccountsAsync()
        {
            var accounts = await _accountService.GetAllAsync();
            return Ok(accounts);
        }

        [HttpGet("accounts/{id:guid}")]
        public async Task<IActionResult> GetAccountByIdAsync(Guid id)
        {
            var account = await _accountService.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        [HttpGet("accounts/email")]
        public async Task<IActionResult> GetAccountByEmailAsync([FromQuery] string email)
        {
            var account = await _accountService.GetByEmailAsync(email);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }
    }
}
