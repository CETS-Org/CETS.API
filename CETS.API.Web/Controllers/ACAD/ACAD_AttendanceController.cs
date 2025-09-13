using Application.Interfaces.ACAD;
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
    }
}

