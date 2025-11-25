using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Class.Requests;
using DTOs.ACAD.ACAD_Class.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_ClassesController : ControllerBase
    {
        private readonly IACAD_ClassService _classService;
        public ACAD_ClassesController(IACAD_ClassService classService)
        {
            _classService = classService;
        }
        [HttpGet("learningClass")]
        public async Task<IActionResult> GetLearningClassByStdID(Guid studentId)
        {
            var result = await _classService.GetLearningClassByStudentId(studentId);

            if (result == null)
                return NotFound(new { message = "Student not have any learning class" });

            return Ok(result);
        }

        [HttpGet("feedbackClasses")]
        public async Task<IActionResult> GetFeedbackClassesByStudentId(Guid studentId)
        {
            var result = await _classService.GetFeedbackClassesByStudentId(studentId);

            if (result == null || !result.Any())
                return NotFound(new { message = "Student not have any classes for feedback" });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(Guid id)
        {
            var result = await _classService.GetClassByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Class not found" });
            return Ok(result);
        }

        [HttpGet("{id:guid}/detail")]
        public async Task<ActionResult<ClassDetailResponse>> GetClassDetailAsync(Guid id)
        {
            var detail = await _classService.GetClassDetailAsync(id);
            if (detail == null)
                return NotFound();
            return Ok(detail);
        }
          
        [HttpGet("by-course/{courseId:guid}")]
        public async Task<IActionResult> GetClassesByCourse2(Guid courseId)
        {
            var classes = await _classService.GetClassesByCourseIdAsync2(courseId);

            if (!classes.Any())
                return NotFound(new { message = "No classes found for this course" });

            return Ok(classes);
        }
        [HttpGet("staff-classes")]
        public async Task<ActionResult<List<ClassRowResponse>>> GetAllClassRows()
        {
            var classes = await _classService.GetAllClassRowsAsync();
            return Ok(classes);
        }

        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            
            if (request.EndDate < request.StartDate)
                return BadRequest(new { message = "EndDate must be greater than or equal to StartDate." });

            if (request.Capacity <= 0)
                return BadRequest(new { message = "Capacity must be greater than 0." });

            var id = await _classService.CreateClassAsync(request);


            return Ok(new { Id = id, Message = "Class created successfully with all related details" });
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClass([FromRoute] Guid id, [FromBody] UpdateClassRequest request)
        {
            // Đồng bộ Id giữa route và body để tránh nhầm lẫn
            if (request.Id == Guid.Empty)
            {
                request.Id = id;
            }
            else if (request.Id != id)
            {
                return BadRequest(new { message = "Route id and body id do not match." });
            }

            // (Tuỳ chọn) guard cơ bản cho ngày/thời gian & capacity
            if (request.EndDate < request.StartDate)
                return BadRequest(new { message = "EndDate must be greater than or equal to StartDate." });

            if (request.Capacity <= 0)
                return BadRequest(new { message = "Capacity must be greater than 0." });

            try
            {
                await _classService.UpdateClassAsync(request);
                return Ok(new { Id = id, Message = "Class updated successfully with all related details" });
            }
            catch (KeyNotFoundException ex)
            {
                // Nếu service ném NotFound (nên dùng kiểu này trong service)
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Các xung đột nghiệp vụ: trùng lịch, giáo viên không rảnh, v.v.
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Fallback cho lỗi không dự liệu (tuỳ bạn có global exception handler hay không)
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClass([FromRoute] Guid id)
        {
            try
            {
                await _classService.SoftDeleteClassAsync(id); 
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("staff-view-byCourse")]
        [ProducesResponseType(typeof(List<ClassStaffViewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ClassStaffViewResponse>>> GetClassByCourseStaffView([FromQuery] Guid courseId)
        {
            // 1. Validate đầu vào
            if (courseId == Guid.Empty)
            {
                return BadRequest(new { message = "Course ID is required." });
            }

            // 2. Gọi Service
            var result = await _classService.GetClassByCourseStaffView(courseId);

            // 3. Trả về kết quả (kể cả khi list rỗng vẫn trả về 200 OK với mảng rỗng [])
            return Ok(result);
        }

        [HttpGet("staff-view/{id:guid}")]
        [ProducesResponseType(typeof(ClassStaffViewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClassStaffViewResponse>> GetClassStaffViewById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Id is required." });

            var result = await _classService.GetClassByIdStaffView(id); 
            if (result is null)
                return NotFound(new { message = "Class not found." });

            return Ok(result);
        }

        [HttpPost("composite")] // Route: api/ACAD_Classes/composite
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateClassComposite([FromBody] CreateClassWithScheduleRequest request)
        {
            // 1. Validate cơ bản
            if (request.EndDate < request.StartDate)
                return BadRequest(new { message = "EndDate must be greater than or equal to StartDate." });

            if (request.Capacity <= 0)
                return BadRequest(new { message = "Capacity must be greater than 0." });

            // 2. Gọi Service transaction gộp
            try
            {
                // Giả định hàm này bạn đã viết trong Service trả về Guid của Class mới
                var classId = await _classService.CreateClassWithScheduleAsync(request);

                return Ok(new { Id = classId, Message = "Class and meetings created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
