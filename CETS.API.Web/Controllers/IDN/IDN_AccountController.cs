using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Account.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_AccountController : ODataController
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

        [EnableQuery]
        [HttpGet]
        public async Task<IActionResult> GetAllAccountsAsync([FromQuery] AccountFilterRequest filter)
        {
            var accounts = await _accountService.GetAllAccountsAsync(filter);
            return Ok(accounts);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAccountByIdAsync(Guid id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        [HttpGet("email")]
        public async Task<IActionResult> GetAccountByEmailAsync([FromQuery] string email)
        {
            var account = await _accountService.GetAccountByEmailAsync(email);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAccountAsync(Guid id, [FromBody] UpdateAccountRequest dto)
        {
            var updatedAccount = await _accountService.UpdateAccountAsync(id, dto);
            if (updatedAccount == null)
            {
                return NotFound();
            }
            return Ok(updatedAccount);
        }

        [HttpPatch("deactivate/{id:guid}")]
        public async Task<IActionResult> DeactivateAccountAsync(Guid id)
        {
            var deactivatedAccount = await _accountService.DeactivateAccountAsync(id);
            if (deactivatedAccount == null)
            {
                return NotFound();
            }
            return Ok(deactivatedAccount);
        }

        [HttpPatch("activate/{id:guid}")]
        public async Task<IActionResult> ActivateAccountAsync(Guid id)
        {
            var activatedAccount = await _accountService.ActivateAccountAsync(id);
            if (activatedAccount == null)
            {
                return NotFound();
            }
            return Ok(activatedAccount);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAccountAsync(Guid id)
        {
            var result = await _accountService.SoftDeleteAccountAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPatch("restore/{id:guid}")]
        public async Task<IActionResult> RestoreAccountAsync(Guid id)
        {
            var restoredAccount = await _accountService.RestoreAccountAsync(id);
            if (restoredAccount == null)
            {
                return NotFound();
            }
            return Ok(restoredAccount);
        }
    }
}
