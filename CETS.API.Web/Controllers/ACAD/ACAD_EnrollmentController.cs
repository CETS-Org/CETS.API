using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Course.Responses;
using DTOs.ACAD.ACAD_Enrollment.Requests;
using DTOs.ACAD.ACAD_Enrollment.Responses;
using DTOs.IDN.IDN_Student.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Utils.Authorization;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ACAD_EnrollmentController : ControllerBase
    {
        private readonly ILogger<ACAD_EnrollmentController> _logger;
        private readonly IACAD_CourseService _courseService;
        private readonly IACAD_EnrollmentService _enrollmentService;
    
        public ACAD_EnrollmentController(ILogger<ACAD_EnrollmentController> logger, IACAD_CourseService courseService, IACAD_EnrollmentService enrollmentService)
        {
            _logger = logger;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
        }

        [HttpGet("CoursesByStudent/{studentId}")]
        public async Task<IActionResult> GetStudentCoursesEnrollment(Guid studentId)
        {
            var courses = await _enrollmentService.GetStudentCoursesEnrollmentAsync(studentId);

            if (courses == null || !courses.Any())
                return NotFound(new { message = "Student chưa enroll khóa học nào." });

            return Ok(courses);
        }

        [HttpGet("academic-results/{studentId}")]
        public async Task<ActionResult<AcademicResultResponse>> GetStudentAcademicResults(Guid studentId)
        {
            var result = await _enrollmentService.GetStudentAcademicResultsAsync(studentId);
            return Ok(result);
        }

        //View Course Detail in Academic Results Student
        [HttpGet("{studentId}/coursedetails-results/{courseId}/")]
        public async Task<ActionResult<StudentCourseDetailResponse>> GetStudentCourseDetail(Guid studentId, Guid courseId)
        {
            var result = await _enrollmentService.GetStudentCourseDetailAsync(studentId, courseId);

            if (result == null)
                return NotFound($"Student {studentId} chưa ghi danh khóa học {courseId}");

            return Ok(result);
        }

        [HttpGet("students/{studentId}/learning-path/overview")]
        public async Task<IActionResult> GetLearningPathOverview(Guid studentId)
        {
            var result = await _enrollmentService.GetLearningPathOverviewAsync(studentId);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy thông tin cho học viên." });

            return Ok(result);
        }


        [HttpGet("waiting-students")]
        [AuthorizeRoles("Teacher", "Admin", "AcademicStaff")]
        [ProducesResponseType(typeof(WaitingStudentSearchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WaitingStudentSearchResult>> GetWaitingStudents(
            [FromQuery] Guid courseId,
            [FromQuery] string? q = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (courseId == Guid.Empty)
            {
                return BadRequest(new { message = "CourseId is required." });
            }

            try
            {
                var result = await _enrollmentService.GetStudentWaitListAsync(courseId, q, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting waiting list for Course {CourseId}", courseId);
                // Tùy policy trả lỗi của bạn, có thể trả 500 hoặc BadRequest
                return StatusCode(500, new { message = "An error occurred while fetching the waiting list." });
            }
        }

        /// <summary>
        /// Bulk update final grades for multiple students
        /// </summary>
        /// <param name="request">Request containing list of enrollment final grade updates</param>
        /// <returns>Result with updated count and details for each enrollment</returns>
        [HttpPut("bulk-update-final-grades")]
        [AuthorizeRoles("Teacher", "Admin", "AcademicStaff")]
        public async Task<IActionResult> BulkUpdateFinalGrades([FromBody] BulkUpdateFinalGradesRequest request)
        {
            try
            {
                var result = await _enrollmentService.BulkUpdateFinalGradesAsync(request);

                // Return 207 Multi-Status if there are partial failures
                if (result.Data.FailedCount > 0 && result.Data.UpdatedCount > 0)
                {
                    return StatusCode(207, result);
                }

                // Return 200 OK if all succeeded
                if (result.Data.FailedCount == 0)
                {
                    return Ok(result);
                }

                // Return 400 Bad Request if all failed
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk update on final grades");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = "An unexpected error occurred while processing your request"
                });
            }
        }

    }
}
