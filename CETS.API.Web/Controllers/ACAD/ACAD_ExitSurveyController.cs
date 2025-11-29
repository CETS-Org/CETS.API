using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_ExitSurvey.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_ExitSurveyController : ControllerBase
    {
        private readonly IACAD_ExitSurveyService _exitSurveyService;

        public ACAD_ExitSurveyController(IACAD_ExitSurveyService exitSurveyService)
        {
            _exitSurveyService = exitSurveyService;
        }

        /// <summary>
        /// Create a new exit survey
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateExitSurvey([FromBody] CreateExitSurveyRequest request)
        {
            try
            {
                var result = await _exitSurveyService.CreateExitSurveyAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get exit survey by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetExitSurveyById(string id)
        {
            try
            {
                var result = await _exitSurveyService.GetExitSurveyByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = "Exit survey not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get exit survey by academic request ID
        /// </summary>
        [HttpGet("academic-request/{academicRequestId}")]
        [Authorize]
        public async Task<IActionResult> GetExitSurveyByAcademicRequestId(string academicRequestId)
        {
            try
            {
                var result = await _exitSurveyService.GetExitSurveyByAcademicRequestIdAsync(academicRequestId);
                if (result == null)
                {
                    return NotFound(new { message = "Exit survey not found for this academic request" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all exit surveys for a student
        /// </summary>
        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetExitSurveysByStudent(string studentId)
        {
            try
            {
                var result = await _exitSurveyService.GetExitSurveysByStudentAsync(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all exit surveys (Admin/Staff only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllExitSurveys()
        {
            try
            {
                var result = await _exitSurveyService.GetAllExitSurveysAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get exit survey analytics (Admin/Staff only)
        /// </summary>
        [HttpGet("analytics")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetExitSurveyAnalytics()
        {
            try
            {
                var result = await _exitSurveyService.GetExitSurveyAnalyticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete an exit survey (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExitSurvey(string id)
        {
            try
            {
                var result = await _exitSurveyService.DeleteExitSurveyAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Exit survey not found" });
                }
                return Ok(new { message = "Exit survey deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

