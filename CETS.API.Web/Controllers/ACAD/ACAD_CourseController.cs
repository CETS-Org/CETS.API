using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Course.Requests;
using DTOs.ACAD.ACAD_Course.Responses;
using DTOs.ACAD.ACAD_Course.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseController : ODataController
    {
        private readonly ILogger<ACAD_CourseController> _logger;
        private readonly IACAD_CourseService _courseService;

        public ACAD_CourseController(ILogger<ACAD_CourseController> logger, IACAD_CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        /// <summary>
        /// Get all courses for listing (with OData support)
        /// </summary>
        [EnableQuery]
        [HttpGet("list")]
        public async Task<IActionResult> GetAllCoursesForListAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesForListAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all courses");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [EnableQuery]
        [HttpGet]
        public async Task<IActionResult> GetAllCoursesAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all courses");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Get all courses with detailed information
        /// </summary>
        [HttpGet("detail")]
        public async Task<ActionResult<IEnumerable<CourseDetailResponse>>> GetAllCoursesDetailsAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesDetailsAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all detailed courses");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course by ID (basic information)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseResponse>> GetCourseByIdAsync(Guid id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound($"Course with ID {id} not found");

                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course with ID {CourseId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course detail by ID (with full information)
        /// </summary>
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<CourseDetailResponse>> GetCourseDetailAsync(Guid id)
        {
            try
            {
                var course = await _courseService.GetCourseDetailAsync(id);
                if (course == null)
                    return NotFound($"Course with ID {id} not found");

                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course detail with ID {CourseId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new course with all related details (benefits, requirements, skills, schedules)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCourseAsync([FromBody] CreateCourseRequest request)
        {
            try
            {
                var courseId = await _courseService.CreateCourseAsync(request);
                return Ok(new { Id = courseId, Message = "Course created successfully with all related details" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get presigned URL for uploading course image directly to R2
        /// </summary>
        [HttpPost("image-upload-url")]
        public async Task<ActionResult<CourseImageUploadResponse>> GetImageUploadUrlAsync([FromBody] ImageUploadRequest request)
        {
            try
            {
                var response = await _courseService.GetImageUploadUrlAsync(request.FileName, request.ContentType);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image upload URL");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Update an existing course
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourseAsync(Guid id, [FromBody] UpdateCourseRequest request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest("Course ID mismatch");

                await _courseService.UpdateCourseAsync(request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found for update with ID {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course with ID {CourseId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a course
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteCourseAsync(Guid id)
        {
            try
            {
                await _courseService.SoftDeleteCourseAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found for deletion with ID {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course with ID {CourseId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Advanced search with basic query parameters
        /// </summary>
        [HttpGet("search-basic")]
        public async Task<ActionResult<CourseSearchResult>> SearchBasicAsync([FromQuery] CourseSearchQuery query, CancellationToken ct)
        {
            try
            {
                var result = await _courseService.SearchBasicAsync(query, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing basic search");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Get courses by level ID
        /// </summary>
        [HttpGet("level/{levelId}")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetCoursesByLevelAsync(Guid levelId)
        {
            try
            {
                var filterRequest = new FilterCourseRequest { LevelId = levelId };
                var courses = await _courseService.FilterCoursesAsync(filterRequest);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses by level ID {LevelId}", levelId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get courses by format ID
        /// </summary>
        [HttpGet("format/{formatId}")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetCoursesByFormatAsync(Guid formatId)
        {
            try
            {
                var filterRequest = new FilterCourseRequest { FormatId = formatId };
                var courses = await _courseService.FilterCoursesAsync(filterRequest);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses by format ID {FormatId}", formatId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get courses by teacher ID
        /// </summary>
        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetCoursesByTeacherAsync(Guid teacherId)
        {
            try
            {
                var filterRequest = new FilterCourseRequest { TeacherId = teacherId };
                var courses = await _courseService.FilterCoursesAsync(filterRequest);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses by teacher ID {TeacherId}", teacherId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Activate a course
        /// </summary>
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateCourseAsync(Guid id)
        {
            try
            {
                await _courseService.ActivateCourseAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found for activation with ID {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating course with ID {CourseId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivate a course
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateCourseAsync(Guid id)
        {
            try
            {
                await _courseService.DeactivateCourseAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found for deactivation with ID {CourseId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating course with ID {CourseId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
      
    }
}
