using Application.Interfaces.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.Analytics
{
    /// <summary>
    /// Analytics and reporting endpoints for dashboard
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get complete overall overview with all metrics (A-F)
        /// </summary>
        /// <returns>Overall overview response with all 6 metric categories</returns>
        /// <response code="200">Returns the overall overview metrics</response>
        [HttpGet("overall-overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverallOverview()
        {
            var result = await _analyticsService.GetOverallOverviewAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get student metrics only (Section A)
        /// </summary>
        /// <returns>Student metrics including total, active, new students, growth rate</returns>
        /// <response code="200">Returns student metrics</response>
        [HttpGet("student-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentMetrics()
        {
            var result = await _analyticsService.GetStudentMetricsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get financial metrics only (Section B)
        /// </summary>
        /// <returns>Financial metrics including revenue, invoices, payment status</returns>
        /// <response code="200">Returns financial metrics</response>
        [HttpGet("financial-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFinancialMetrics()
        {
            var result = await _analyticsService.GetFinancialMetricsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get course and class metrics only (Section C)
        /// </summary>
        /// <returns>Course and class metrics including enrollments, fill rates, completion rates</returns>
        /// <response code="200">Returns course and class metrics</response>
        [HttpGet("course-class-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCourseClassMetrics()
        {
            var result = await _analyticsService.GetCourseClassMetricsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get teacher metrics only (Section D)
        /// </summary>
        /// <returns>Teacher metrics including total teachers, workload, ratings, contracts</returns>
        /// <response code="200">Returns teacher metrics</response>
        [HttpGet("teacher-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherMetrics()
        {
            var result = await _analyticsService.GetTeacherMetricsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get event metrics only (Section E)
        /// </summary>
        /// <returns>Event metrics including total events, registrations, attendance rates</returns>
        /// <response code="200">Returns event metrics</response>
        [HttpGet("event-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventMetrics()
        {
            var result = await _analyticsService.GetEventMetricsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get feedback and satisfaction metrics only (Section F)
        /// </summary>
        /// <returns>Feedback metrics including ratings, NPS, customer satisfaction scores</returns>
        /// <response code="200">Returns feedback and satisfaction metrics</response>
        [HttpGet("feedback-metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeedbackMetrics()
        {
            var result = await _analyticsService.GetFeedbackMetricsAsync();
            return Ok(result);
        }
    }
}






