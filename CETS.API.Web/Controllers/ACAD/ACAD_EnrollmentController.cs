using Application.Interfaces.ACAD;
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

        [HttpGet("student/{studentId}/courses")]
        public async Task<IActionResult> GetStudentCoursesEnrollment(Guid studentId)
        {
            var courses = await _enrollmentService.GetStudentCoursesEnrollmentAsync(studentId);

            if (courses == null || !courses.Any())
                return NotFound(new { message = "Student chưa enroll khóa học nào." });

            return Ok(courses);
        }


    }
}
