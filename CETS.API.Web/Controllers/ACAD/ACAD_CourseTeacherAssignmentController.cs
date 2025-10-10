using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseTeacherAssignment.Responses;
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

        /// <summary>
        /// Lấy danh sách các khóa học mà giáo viên đang dạy
        /// </summary>
        /// <param name="teacherId">ID của giáo viên</param>
        /// <returns>Danh sách TeachingClassResponse</returns>
        /// 
        
        [HttpGet("CoursesByTeacher/{teacherId:guid}")]
        public async Task<IActionResult> GetCoursesByTeacher(Guid teacherId)
        {
            var courses = await _courseAssignmentService.GetCoursesByTeacherIdAsync(teacherId);

            if (!courses.Any())
                return NotFound(new { Message = "No courses assigned for this teacher." });

            return Ok(courses);
        }

        #region view teaching courses
        [HttpGet("teaching-courses/{teacherId:guid}")]
        public async Task<IActionResult> GetTeachingCoursesByTeacher(Guid teacherId)
        {
            try
            {
                var teachingCourses = await _courseAssignmentService.GetAllTeachingCourses(teacherId);
                if (!teachingCourses.Any())
                    return NotFound(new { Message = "This teacher has not been assigned to teach any courses." });
                return Ok(teachingCourses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teaching courses for teacher {TeacherId}", teacherId);
                return StatusCode(500, new { Message = "An error occurred while retrieving the teacher's course list." });
            }
        }
        #endregion

        #region view teaching classes
        [HttpGet("teaching-classes/{teacherId:guid}/{courseId:guid}")]
        public async Task<IActionResult> GetTeachingClassesByTeacher(Guid teacherId, Guid courseId)
        {
            var teachingClasses = await _courseAssignmentService.GetTeachingClassesByTeacherIdAndCourseIdAsync(teacherId, courseId);
            if (teachingClasses == null)
                return NotFound(new { Message = "No courses assigned for this teacher." });

            return Ok(teachingClasses);
        }
        #endregion
    }


}
