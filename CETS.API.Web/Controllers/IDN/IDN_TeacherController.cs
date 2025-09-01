using Application.Interfaces.IDN;
using DTOs.IDN_Teacher.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_TeacherController : ControllerBase
    {
        private readonly ILogger<IDN_TeacherController> _logger;
        private readonly IIDN_TeacherService _teacherService;

        public IDN_TeacherController(ILogger<IDN_TeacherController> logger, IIDN_TeacherService teacherService)
        {
            _logger = logger;
            _teacherService = teacherService;
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetAllTeachersAsync()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        [HttpGet("teachers/{id:guid}")]
        public async Task<IActionResult> GetTeacherByIdAsync(Guid id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpGet("teachers/details/{id:guid}")]
        public async Task<IActionResult> GetTeacherDetailsAsync(Guid id)
        {
            var teacher = await _teacherService.GetTeacherDetailsAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }



        [HttpGet("teachers/code/{code}")]
        public async Task<IActionResult> GetTeacherByCodeAsync(string code)
        {
            var teacher = await _teacherService.GetTeacherByCodeAsync(code);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpGet("teachers/email/{email}")]
        public async Task<IActionResult> GetTeacherByEmailAsync(string email)
        {
            var teacher = await _teacherService.GetTeacherByEmailAsync(email);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpPost("teachers")]
        public async Task<IActionResult> CreateTeacherAsync([FromBody] CreateTeacherRequest dto)
        {
            var createdTeacher = await _teacherService.CreateTeacherAsync(dto);
            return Created("New teacher created", createdTeacher);
        }

        [HttpPut("teachers/{id:guid}")]
        public async Task<IActionResult> UpdateTeacherAsync(Guid id, [FromBody] UpdateTeacherRequest dto)
        {
            var updatedTeacher = await _teacherService.UpdateTeacherAsync(id, dto);
            if (updatedTeacher == null)
            {
                return NotFound();
            }
            return Ok(updatedTeacher);
        }

        [HttpPatch("teachers/activate/{id:guid}")]
        public async Task<IActionResult> ActivateTeacherAsync(Guid id)
        {
            await _teacherService.ActivateTeacherAsync(id);
            return NoContent();
        }

        [HttpDelete("teachers/{id:guid}")]
        public async Task<IActionResult> SoftDeleteTeacherAsync(Guid id)
        {
            await _teacherService.SoftDeleteTeacherAsync(id);
            return NoContent();
        }

    }
}
