using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
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
        [HttpGet("course/{courseId:guid}")]
        public async Task<IActionResult> GetClassesByCourse(Guid courseId)
        {
            var classes = await _classService.GetClassesByCourseIdAsync(courseId);

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
    }
}
