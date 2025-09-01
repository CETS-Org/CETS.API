using Application.Interfaces.IDN;
using DTOs.IDN_Student.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_StudentController : ControllerBase
    {
        private readonly ILogger<IDN_StudentController> _logger;
        private readonly IIDN_StudentService _studentService;
        public IDN_StudentController(ILogger<IDN_StudentController> logger, IIDN_StudentService studentService)
        {
            _logger = logger;
            _studentService = studentService;
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudentsAsync()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("students/{id:guid}")]
        public async Task<IActionResult> GetStudentByIdAsync(Guid id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpGet("students/code/{code}")]
        public async Task<IActionResult> GetStudentByCodeAsync(string code)
        {
            var student = await _studentService.GetStudentByCodeAsync(code);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpPost("students")]
        public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentRequest dto)
        {
            var createdStudent = await _studentService.CreateStudentAsync(dto);
            return Created("New student created", createdStudent);
        }

        [HttpPut("students/{id:guid}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid id, [FromBody] UpdateStudentRequest dto)
        {
            var updatedStudent = await _studentService.UpdateStudentAsync(id, dto);
            if (updatedStudent == null)
            {
                return NotFound();
            }
            return Ok(updatedStudent);
        }

        [HttpPatch("students/activate/{id:guid}")]
        public async Task<IActionResult> ActivateStudentAsync(Guid id)
        {
            await _studentService.ActivateStudentAsync(id);
            return NoContent();
        }


        [HttpDelete("students/{id:guid}")]
        public async Task<IActionResult> SoftDeleteStudentAsync(Guid id)
        {
            var deletedStudent = await _studentService.SoftDeleteStudentAsync(id);
            if (deletedStudent == null)
            {
                return NotFound();
            }
            return Ok(deletedStudent);
        }





    }
}
