using Application.Implementations.IDN;
using Application.Interfaces.ExternalServices.Email;
using Application.Interfaces.ExternalServices.Security;
using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Account.Requests;
using MassTransit.JobService;
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
                    // Return HTML page for successful verification
                    var loginUrl = _configuration["VerificationSettings:FrontendLoginUrl"] ?? "https://localhost:3000/login";
                    var htmlContent = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <title>Account Verified - CETS</title>
                            <meta charset='UTF-8'>
                            <style>
                                body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding: 50px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center; }}
                                .container {{ max-width: 500px; margin: 0 auto; background: white; padding: 50px 40px; border-radius: 20px; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }}
                                .success-icon {{ color: #4CAF50; font-size: 80px; margin-bottom: 30px; font-weight: bold; }}
                                .title {{ color: #333; font-size: 28px; margin-bottom: 20px; font-weight: 600; }}
                                .message {{ color: #666; font-size: 16px; margin-bottom: 40px; line-height: 1.6; }}
                                .button {{ background: linear-gradient(45deg, #4CAF50, #45a049); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; display: inline-block; font-weight: 600; font-size: 16px; transition: transform 0.3s ease; }}
                                .button:hover {{ transform: translateY(-2px); box-shadow: 0 5px 15px rgba(76, 175, 80, 0.3); }}
                                .logo {{ margin-bottom: 20px; }}
                                .logo img {{ height: 50px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='logo'>
                                    <img src='https://i.ibb.co/0c2dT3L/cets-logo.png' alt='CETS Logo'>
                                </div>
                                <div class='title'>Account Verified Successfully!</div>
                                <div class='message'>Your CETS account has been verified successfully. You can now log in to your account and start your learning journey.</div>
                                <a href='{loginUrl}' class='button'>Go to Login</a>
                            </div>
                        </body>
                        </html>";
                    
                    return Content(htmlContent, "text/html");
                }
                return BadRequest(new { message = "Verification failed." });
            }
            catch (KeyNotFoundException ex)
            {
                var errorMessage = System.Web.HttpUtility.HtmlEncode(ex.Message);
                var errorHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Verification Failed - CETS</title>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding: 50px; background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center; }}
                            .container {{ max-width: 500px; margin: 0 auto; background: white; padding: 50px 40px; border-radius: 20px; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }}
                            .error-icon {{ color: #f44336; font-size: 80px; margin-bottom: 30px; font-weight: bold; }}
                            .title {{ color: #333; font-size: 28px; margin-bottom: 20px; font-weight: 600; }}
                            .message {{ color: #666; font-size: 16px; margin-bottom: 40px; line-height: 1.6; }}
                            .button {{ background: linear-gradient(45deg, #f44336, #d32f2f); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; display: inline-block; font-weight: 600; font-size: 16px; transition: transform 0.3s ease; }}
                            .button:hover {{ transform: translateY(-2px); box-shadow: 0 5px 15px rgba(244, 67, 54, 0.3); }}
                            .logo {{ margin-bottom: 20px; }}
                            .logo img {{ height: 50px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='logo'>
                                <img src='https://i.ibb.co/0c2dT3L/cets-logo.png' alt='CETS Logo'>
                            </div>
                            <div class='title'>Verification Failed</div>
                            <div class='message'>{errorMessage}</div>
                            <a href='javascript:history.back()' class='button'>Go Back</a>
                        </div>
                    </body>
                    </html>";
                return Content(errorHtml, "text/html");
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = System.Web.HttpUtility.HtmlEncode(ex.Message);
                var errorHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Verification Failed - CETS</title>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding: 50px; background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center; }}
                            .container {{ max-width: 500px; margin: 0 auto; background: white; padding: 50px 40px; border-radius: 20px; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }}
                            .error-icon {{ color: #f44336; font-size: 80px; margin-bottom: 30px; font-weight: bold; }}
                            .title {{ color: #333; font-size: 28px; margin-bottom: 20px; font-weight: 600; }}
                            .message {{ color: #666; font-size: 16px; margin-bottom: 40px; line-height: 1.6; }}
                            .button {{ background: linear-gradient(45deg, #f44336, #d32f2f); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; display: inline-block; font-weight: 600; font-size: 16px; transition: transform 0.3s ease; }}
                            .button:hover {{ transform: translateY(-2px); box-shadow: 0 5px 15px rgba(244, 67, 54, 0.3); }}
                            .logo {{ margin-bottom: 20px; }}
                            .logo img {{ height: 50px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='logo'>
                                <img src='https://i.ibb.co/0c2dT3L/cets-logo.png' alt='CETS Logo'>
                            </div>
                            <div class='title'>Verification Failed</div>
                            <div class='message'>{errorMessage}</div>
                            <a href='javascript:history.back()' class='button'>Go Back</a>
                        </div>
                    </body>
                    </html>";
                return Content(errorHtml, "text/html");
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
    }
}
