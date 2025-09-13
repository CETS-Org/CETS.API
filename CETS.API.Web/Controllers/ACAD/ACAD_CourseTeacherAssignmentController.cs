using Application.Interfaces.ACAD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseTeacherAssignmentController : ControllerBase
    {
        private readonly ILogger<ACAD_CourseTeacherAssignmentController> _logger;
        private readonly IACAD_CourseTeacherAssignmentService _courseAssignmentService;

        public ACAD_CourseTeacherAssignmentController(ILogger<ACAD_CourseTeacherAssignmentController> logger, IACAD_CourseTeacherAssignmentService courseAssignmentService)
        {
            _logger = logger;
            _courseAssignmentService = courseAssignmentService;
        }

        [HttpGet("CoursesByTeacher/{teacherId:guid}")]
        public async Task<IActionResult> GetCoursesByTeacher(Guid teacherId)
        {
            var courses = await _courseAssignmentService.GetCoursesByTeacherIdAsync(teacherId);

            if (!courses.Any())
                return NotFound(new { Message = "No courses assigned for this teacher." });

            return Ok(courses);
        }

    }


}
