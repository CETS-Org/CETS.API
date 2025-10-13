using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Course.Responses;
using DTOs.ACAD.ACAD_Enrollment.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
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


    }
}
