using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Student.Requests;
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

        [HttpGet]
        public async Task<IActionResult> GetAllStudentsAsync()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStudentByIdAsync(Guid id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetStudentByCodeAsync(string code)
        {
            var student = await _studentService.GetStudentByCodeAsync(code);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentRequest dto)
        {
            var createdStudent = await _studentService.CreateStudentAsync(dto);
            return Created("New student created", createdStudent);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid id, [FromBody] UpdateStudentRequest dto)
        {
            var updatedStudent = await _studentService.UpdateStudentAsync(id, dto);
            if (updatedStudent == null)
            {
                return NotFound();
            }
            return Ok(updatedStudent);
        }

        [HttpPatch("restore/{id:guid}")]
        public async Task<IActionResult> RestoreStudentAsync(Guid id)
        {
            var student = await _studentService.RestoreStudentAsync(id);
            return Ok(student);
        }


        [HttpDelete("{id:guid}")]
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
