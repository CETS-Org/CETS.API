using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Attendance.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_AttendanceController : ControllerBase
    {
        private readonly IACAD_AttendanceService _attendanceService;

        public ACAD_AttendanceController(IACAD_AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Lấy thống kê điểm danh (tổng số buổi, số present, absent) theo student và course
        /// </summary>
        [HttpGet("courses/{courseId}/students/{studentId}/summary")]
        public async Task<IActionResult> GetStudentAttendanceSummary(Guid courseId, Guid studentId)
        {
            var summary = await _attendanceService.GetStudentAttendanceSummaryAsync(studentId, courseId);

            if (summary == null)
                return NotFound(new { message = "Không tìm thấy dữ liệu điểm danh cho student trong course này" });

            return Ok(summary);
        }

        [HttpGet("students/{studentId}/attendance-report")]
        public async Task<IActionResult> GetStudentAttendanceSummaries(Guid studentId)
        {
            var reports = await _attendanceService.GetStudentAttendanceReportAsync(studentId);
            return Ok(reports);
        }

        /// <summary>
        /// Lấy danh sách học sinh trong một lớp để điểm danh
        /// </summary>
        /// <param name="classId">ID của lớp học</param>
        /// <param name="classMeetingId">ID của buổi học (optional) - Nếu truyền thì sẽ lấy trạng thái điểm danh từ ACAD_Attendance</param>
        /// <returns>Danh sách học sinh kèm trạng thái điểm danh (nếu có classMeetingId)</returns>
        [HttpGet("classes/{classId}/students/{classMeetingId}")]
        public async Task<IActionResult> GetStudentsByClassForAttendance(Guid classId, Guid classMeetingId)
        {
            var students = await _attendanceService.GetStudentsByClassForAttendanceAsync(classId, classMeetingId);
            return Ok(students);
        }

        /// <summary>
        /// Điểm danh hàng loạt cho một buổi học
        /// </summary>
        /// <param name="request">
        /// Request bao gồm:
        /// - ClassMeetingId: ID của buổi học
        /// - TeacherId: ID của giáo viên điểm danh
        /// - AbsentStudentIds: Danh sách ID học sinh vắng mặt (các học sinh không có trong list này sẽ được đánh dấu Present)
        /// </param>
        [HttpPost("bulk-mark")]
        public async Task<IActionResult> BulkMarkAttendance([FromBody] BulkAttendanceRequest request)
        {
            try
            {
                var result = await _attendanceService.BulkMarkAttendanceAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}

