using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseTeacherAssignment.Requests;
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

        [HttpPost("available-teachers")]
        [ProducesResponseType(typeof(List<TeacherOptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableTeachersForClass([FromBody] GetAvailableTeachersRequest request)
        {
            // 1. Validate cơ bản
            if (request.CourseId == Guid.Empty)
                return BadRequest(new { Message = "CourseId is required." });

          /*  if (request.Schedules == null || !request.Schedules.Any())
                return BadRequest(new { Message = "At least one schedule is required to check availability." });

            if (request.EndDate < request.StartDate)
                return BadRequest(new { Message = "EndDate must be after StartDate." });
          */

            try
            {
                // 2. Gọi Service xử lý logic tìm kiếm
                var availableTeachers = await _courseAssignmentService.GetAvailableTeachersAsync(request);

                // 3. Trả về danh sách (dù rỗng cũng trả về 200 OK list rỗng)
                return Ok(availableTeachers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding available teachers for Course {CourseId}", request.CourseId);
                return StatusCode(500, new { Message = "An error occurred while searching for teachers." });
            }
        }

        [HttpGet("course/{courseId:guid}")]
        public async Task<IActionResult> GetAssignmentsByCourse(Guid courseId)
        {
            var assignments = await _courseAssignmentService.GetAssignmentsByCourseIdAsync(courseId);
            return Ok(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateCourseTeacherAssignmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _courseAssignmentService.CreateAssignmentAsync(request);
                return CreatedAtAction(nameof(GetAssignmentsByCourse), new { courseId = result.CourseID }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpDelete("{assignmentId:guid}")]
        public async Task<IActionResult> DeleteAssignment(Guid assignmentId)
        {
            try
            {
                await _courseAssignmentService.DeleteAssignmentAsync(assignmentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }


}
