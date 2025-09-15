using Application.Implementations.IDN;
using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Account.Requests;
using MassTransit.JobService;
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
        private readonly IIDN_JwtService _jwtService;

        public IDN_AccountController(ILogger<IDN_AccountController> logger, IIDN_AccountService accountService, IIDN_JwtService jwtService)
        {
            _logger = logger;
            _accountService = accountService;
            _jwtService = jwtService;
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

        #region Login
        [HttpPost("login/student")]
        public async Task<IActionResult> StudentLogin([FromBody] LoginRequest dto)
        {
            var account = await _accountService.ValidateUserCredentialsAsync(dto.Email, dto.Password);
            if (account == null || account.RoleNames.All(r => r != "Student"))
            {
                return Unauthorized("Invalid credentials or not a student account.");
            }
            var token = _jwtService.GenerateJwtToken(account);
            return Ok(new
            {
                message = "Login successful",
                token,
                account = account
            });
        }

        [HttpPost("login/teacher")]
        public async Task<IActionResult> TeacherLogin([FromBody] LoginRequest dto)
        {
            var account = await _accountService.ValidateUserCredentialsAsync(dto.Email, dto.Password);
            if (account == null || account.RoleNames.All(r => r != "Teacher"))
            {
                return Unauthorized("Invalid credentials or not a teacher account.");
            }
            var token = _jwtService.GenerateJwtToken(account);
            return Ok(new
            {
                message = "Login successful",
                token,
                account = account
            });
        }

        [HttpPost("login/academicStaff")]
        public async Task<IActionResult> AcademicStaffLogin([FromBody] LoginRequest dto)
        {
            var account = await _accountService.ValidateUserCredentialsAsync(dto.Email, dto.Password);
            if (account == null || account.RoleNames.All(r => r != "AcademicStaff"))
            {
                return Unauthorized("Invalid credentials or not a Academic Staff account.");
            }
            var token = _jwtService.GenerateJwtToken(account);
            return Ok(new
            {
                message = "Login successful",
                token,
                account = account
            });
        }

        [HttpPost("login/accountantStaff")]
        public async Task<IActionResult> AccountantStaffLogin([FromBody] LoginRequest dto)
        {
            var account = await _accountService.ValidateUserCredentialsAsync(dto.Email, dto.Password);
            if (account == null || account.RoleNames.All(r => r != "AccountantStaff"))
            {
                return Unauthorized("Invalid credentials or not a Accountant Staff account.");
            }
            var token = _jwtService.GenerateJwtToken(account);
            return Ok(new
            {
                message = "Login successful",
                token,
                account = account
            });
        }
        [HttpPost("login/admin")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginRequest dto)
        {
            var account = await _accountService.ValidateUserCredentialsAsync(dto.Email, dto.Password);
            if (account == null || account.RoleNames.All(r => r != "Admin"))
            {
                return Unauthorized("Invalid credentials or not a Accountant Staff account.");
            }
            var token = _jwtService.GenerateJwtToken(account);
            return Ok(new
            {
                message = "Login successful",
                token,
                account = account
            });
        }
        #endregion
    }
}
