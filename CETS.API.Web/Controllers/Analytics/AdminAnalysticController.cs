using Application.Interfaces.Analytics;
using DTOs.Analytics.Dashboard.Requests;
using DTOs.Analytics.Dashboard.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.Analytics
{
    /// <summary>
    /// Dashboard analytics endpoints for admin visualization
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAnalysticController : ControllerBase
    {
        private readonly IDashboardAnalyticsService _dashboardService;
        private readonly ILogger<AdminAnalysticController> _logger;

        public AdminAnalysticController(
            IDashboardAnalyticsService dashboardService,
            ILogger<AdminAnalysticController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("revenue")]
        [ProducesResponseType(typeof(RevenueAnalyticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRevenueAnalytics()
        {
            try
            {
                var result = await _dashboardService.GetRevenueAnalyticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue analytics");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        [HttpGet("top-courses")]
        [ProducesResponseType(typeof(CourseEnrollmentStatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopEnrolledCourses(
            [FromQuery] int topN = 5,
            [FromQuery] string? categoryFilter = null,
            [FromQuery] string sortBy = "enrollments",
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                if (topN < 1 || topN > 50)
                {
                    return BadRequest(new { message = "topN must be between 1 and 50" });
                }

                var request = new TopCoursesRequest
                {
                    TopN = topN,
                    CategoryFilter = categoryFilter,
                    SortBy = sortBy,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var result = await _dashboardService.GetTopEnrolledCoursesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top courses");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        [HttpGet("dropout-analysis")]
        [ProducesResponseType(typeof(StudentDropoutAnalyticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudentDropoutAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? ageGroupFilter = null,
            [FromQuery] string? courseTypeFilter = null,
            [FromQuery] bool includeDemographics = true,
            [FromQuery] bool includeRecommendations = true)
        {
            try
            {
                var request = new DropoutAnalysisRequest
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    AgeGroupFilter = ageGroupFilter,
                    CourseTypeFilter = courseTypeFilter,
                    IncludeDemographics = includeDemographics,
                    IncludeRecommendations = includeRecommendations
                };

                var result = await _dashboardService.GetStudentDropoutAnalyticsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dropout analytics");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("enrollment-analysis")]
        [ProducesResponseType(typeof(StudentEnrollmentAnalyticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnrollmentAnalytics()
        {
            try
            {
                var result = await _dashboardService.GetEnrollmentAnalyticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollment analytics");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("ai-recommendations")]
        [ProducesResponseType(typeof(AIAnalysisResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAIRecommendations([FromBody] AIRecommendationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                // Validate focus areas
                var validFocusAreas = new[] { "revenue", "retention", "enrollment", "operations", "marketing" };
                var invalidAreas = request.FocusAreas.Where(a => !validFocusAreas.Contains(a.ToLower())).ToList();
                
                if (invalidAreas.Any())
                {
                    return BadRequest(new { message = $"Invalid focus areas: {string.Join(", ", invalidAreas)}" });
                }

                var result = await _dashboardService.GetAIRecommendationsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI recommendations");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("ai-recommendations")]
        [ProducesResponseType(typeof(AIAnalysisResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAIRecommendationsSimple(
            [FromQuery] string? focusAreas = "revenue,retention,enrollment",
            [FromQuery] string timeframe = "last_6_months",
            [FromQuery] bool includeRiskAnalysis = true,
            [FromQuery] bool includeOpportunities = true)
        {
            try
            {
                var request = new AIRecommendationRequest
                {
                    FocusAreas = focusAreas?.Split(',').Select(a => a.Trim()).ToList() ?? new List<string>(),
                    Timeframe = timeframe,
                    IncludeRiskAnalysis = includeRiskAnalysis,
                    IncludeOpportunities = includeOpportunities
                };

                var result = await _dashboardService.GetAIRecommendationsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI recommendations");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        [HttpGet("complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompleteDashboard()
        {
            try
            {
                var revenue = await _dashboardService.GetRevenueAnalyticsAsync();
                var topCourses = await _dashboardService.GetTopEnrolledCoursesAsync(new TopCoursesRequest { TopN = 5 });
                var dropout = await _dashboardService.GetStudentDropoutAnalyticsAsync(new DropoutAnalysisRequest());
                var aiRecommendations = await _dashboardService.GetAIRecommendationsAsync(new AIRecommendationRequest
                {
                    FocusAreas = new List<string> { "revenue", "retention", "enrollment" },
                    Timeframe = "last_6_months",
                    IncludeRiskAnalysis = true,
                    IncludeOpportunities = true
                });

                var result = new
                {
                    revenue,
                    topCourses,
                    dropout,
                    aiRecommendations,
                    generatedAt = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complete dashboard");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var revenue = await _dashboardService.GetRevenueAnalyticsAsync();
                var topCourses = await _dashboardService.GetTopEnrolledCoursesAsync(new TopCoursesRequest { TopN = 5 });
                var dropout = await _dashboardService.GetStudentDropoutAnalyticsAsync(new DropoutAnalysisRequest());

                var summary = new
                {
                    currentMonthRevenue = revenue.CurrentMonth,
                    currentMonthGrowth = revenue.Monthly.LastOrDefault()?.Growth ?? 0,
                    totalCourses = topCourses.TotalCourses,
                    totalEnrollments = topCourses.TotalEnrollments,
                    dropoutRate = dropout.OverallDropoutRate,
                    highRiskStudents = dropout.HighRiskStudents,
                    retentionRate = 100 - dropout.OverallDropoutRate,
                    avgCourseRating = topCourses.TopCourses.Any() ? topCourses.TopCourses.Average(c => c.AverageRating) : 0
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard summary");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}


