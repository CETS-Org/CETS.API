using Application.Implementations.IDN;
using Application.Interfaces.ExternalServices.Email;
using Application.Interfaces.ExternalServices.Security;
using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Account.Requests;
using MassTransit.JobService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Web;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_AccountController : ODataController
    {
        private readonly ILogger<IDN_AccountController> _logger;
        private readonly IIDN_AccountService _accountService;
        private readonly IJwtService _jwtService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;

        public IDN_AccountController(ILogger<IDN_AccountController> logger, IIDN_AccountService accountService, IJwtService jwtService, IMailService mailService, IConfiguration configuration)
        {
            _logger = logger;
            _accountService = accountService;
            _jwtService = jwtService;
            _mailService = mailService;
            _configuration = configuration;
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
                return NotFound(new { message = $"Account with id = {id} not found" });
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

        [HttpPost]
        public async Task<IActionResult> CreateAccountAsync([FromBody] CreateAccountRequest dto)
        {
            try
            {
                var account = await _accountService.CreateAccountAsync(dto);
                return Ok(account);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpPatch("{id:guid}/profile")]
        public async Task<IActionResult> PatchAccountProfileAsync(
        Guid id,
        [FromBody] UpdateAccountProfileRequest dto)
        {
            var account = await _accountService.UpdateAccountProfileAsync(id, dto, User);

            if (account == null)
                return NotFound();

            return Ok(account);
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

        [HttpGet("checkEmailExist/{email}")]
        public async Task<IActionResult> CheckEmailExist(string email)
        {
            var result = await _accountService.CheckEmailExist(email);
            if (result)
            {
                return Ok(true);
            }
            return NotFound(false);
        }

        [HttpGet("checkCIDExist/{cid}")]
        public async Task<IActionResult> CheckCIDExist(string cid)
        {
            var result = await _accountService.CheckCIDExist(cid);
            if (result)
            {
                return Ok(true);
            }
            return NotFound(false);
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
        [HttpPost("googleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest dto)
        {
            var account = await _accountService.ValidateGoogleAccountAsync(dto);
            if (account == null)
            {
                return Unauthorized("Invalid Google token.");
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

        #region register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest dto)
        {
            try
            {
                var account = await _accountService.RegisterAsync(dto);
                return Ok(new
                {
                    message = "Account created successfully! Please check your email for verification.",
                    account = account
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Account Verification
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAccountAsync([FromBody] VerifyAccountRequest dto)
        {
            try
            {
                var result = await _accountService.VerifyAccountAsync(dto);
                if (result)
                {
                    return Ok(new { message = "Account has been verified successfully!" });
                }
                return BadRequest(new { message = "Verification failed." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verify-by-link")]
        public async Task<IActionResult> VerifyAccountByLinkAsync([FromQuery] string email, [FromQuery] string code)
        {
            try
            {
                var request = new VerifyAccountRequest
                {
                    Email = email,
                    VerificationCode = code
                };
                
                var result = await _accountService.VerifyAccountAsync(request);
                if (result)
                {
                    // Redirect to frontend success page
                    var successUrl = _configuration["VerificationSettings:FrontendSuccessUrl"] ?? "https://localhost:3000/verification-success";
                    return Redirect(successUrl);
                }
                
                // Redirect to frontend error page
                var errorUrl = _configuration["VerificationSettings:FrontendErrorUrl"] ?? "https://localhost:3000/verification-error";
                return Redirect(errorUrl);
            }
            catch (KeyNotFoundException ex)
            {
                // Redirect to frontend error page with error message
                var errorUrl = _configuration["VerificationSettings:FrontendErrorUrl"] ?? "https://localhost:3000/verification-error";
                var encodedMessage = Uri.EscapeDataString(ex.Message);
                return Redirect($"{errorUrl}?error={encodedMessage}");
            }
            catch (InvalidOperationException ex)
            {
                // Redirect to frontend error page with error message
                var errorUrl = _configuration["VerificationSettings:FrontendErrorUrl"] ?? "https://localhost:3000/verification-error";
                var encodedMessage = Uri.EscapeDataString(ex.Message);
                return Redirect($"{errorUrl}?error={encodedMessage}");
            }
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationCodeAsync([FromBody] ResendVerificationRequest dto)
        {
            try
            {
                var result = await _accountService.ResendVerificationCodeAsync(dto.Email);
                if (result)
                {
                    return Ok(new { message = "A new verification code has been sent to your email!" });
                }
                return BadRequest(new { message = "Failed to send verification code." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region forgot password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            try
            {
                var result = await _accountService.GetOTP(email);
                if (result == null)
                {
                    return BadRequest(new  { message = "Failed to send OTP." });
                }
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = _accountService.VerifyOTP(request);

            if (!isValid)
                return BadRequest("Invalid or expired OTP.");

            // Generate JWT token for password reset
            var token = _jwtService.GenerateOtpJwt(request.Email, request.Otp);

            return Ok(new 
            { 
                message = "OTP verified successfully.",
                token = token,
                email = request.Email
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // Validate JWT token first
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new { message = "Token is required for password reset." });
                }

                // Validate the password reset JWT token
                if (!_jwtService.ValidatePasswordResetToken(request.Token, request.Email))
                {
                    return BadRequest(new { message = "Invalid or expired token." });
                }

                var updatedAccount = await _accountService.ChangePassword(request.NewPassword, request.Email);
                return Ok(new { message = "Password has been reset successfully.", account = updatedAccount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Reset password fail!" });
            }
        }

        #endregion

        #region change password
        [HttpPost("change-password")]
        [Authorize]       
        
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var account = await _accountService.GetAccountByEmailAsync(request.Email);
                if (account == null)
                {
                    return NotFound(new { message = "Account not found." });
                }
                // Validate current password
                var validCredentials = await _accountService.ValidateUserCredentialsAsync(request.Email, request.OldPassword);
                if (validCredentials == null)
                {
                    return BadRequest(new { message = "Current password is incorrect." });
                }
                // Change to new password
                var updatedAccount = await _accountService.ChangePassword(request.NewPassword, request.Email);
                return Ok(new { message = "Password changed successfully.", account = updatedAccount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to change password." });
            }
        }
        #endregion
    }
}
