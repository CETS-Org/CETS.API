using Application.Interfaces.Analytics;
using DTOs.Analytics.ClassOverview.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.Analytics
{
    /// <summary>
    /// Class-level Analytics API
    /// Provides comprehensive analytics for individual classes in the center
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassAnalyticsController : ControllerBase
    {
        private readonly IClassAnalyticsService _service;

        public ClassAnalyticsController(IClassAnalyticsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get comprehensive analytics for a specific class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>Detailed class analytics including attendance, performance, engagement, operational status, and teacher effectiveness</returns>
        /// <response code="200">Returns class analytics</response>
        /// <response code="404">Class not found</response>
        [HttpGet("{classId:guid}")]
        [Authorize(Roles = "Admin,FinanceManager,AcademicManager")]
        public async Task<IActionResult> GetClassOverview(Guid classId)
        {
            var result = await _service.GetClassOverviewAsync(classId);
            
            if (result == null)
            {
                return NotFound(new { message = $"Class with ID {classId} not found" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get summary list of all classes with basic metrics
        /// </summary>
        /// <param name="filter">Filter options (courseId, teacherId, status, date range, pagination)</param>
        /// <returns>Paginated list of class summaries</returns>
        /// <response code="200">Returns list of class summaries</response>
        [HttpGet]
        [Authorize(Roles = "Admin,FinanceManager,AcademicManager")]
        public async Task<IActionResult> GetAllClassesOverview([FromQuery] ClassFilterRequest filter)
        {
            var result = await _service.GetAllClassesOverviewAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get class summary by class ID (lightweight version)
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>Class summary with basic metrics</returns>
        /// <response code="200">Returns class summary</response>
        /// <response code="404">Class not found</response>
        [HttpGet("{classId:guid}/summary")]
        [Authorize(Roles = "Admin,FinanceManager,AcademicManager")]
        public async Task<IActionResult> GetClassSummary(Guid classId)
        {
            var result = await _service.GetClassSummaryAsync(classId);
            
            if (result == null)
            {
                return NotFound(new { message = $"Class with ID {classId} not found" });
            }

            return Ok(result);
        }
    }
}



