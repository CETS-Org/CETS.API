using Application.Interfaces.Common.Storage;
using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Student.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_StudentController : ControllerBase
    {
        private readonly ILogger<IDN_StudentController> _logger;
        private readonly IIDN_StudentService _studentService;
        private readonly IFileStorageService _fileStorageService;
        public IDN_StudentController(ILogger<IDN_StudentController> logger, IIDN_StudentService studentService, IFileStorageService fileStorageService)
        {
            _logger = logger;
            _studentService = studentService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentsAsync()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
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

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PatchStudentProfile(
       Guid id,
       [FromBody] UpdateStudentProfileRequest dto,
       [FromHeader(Name = "x-role")] string role = "Student")
        {
            // giả lập user role bằng header (sau này thay JWT)
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role)
            }, "FakeAuth"));

            var result = await _studentService.UpdateStudentProfileAsync(id, dto, fakeUser);

            if (result == null)
                return NotFound(new { error = $"Student with AccountId {id} not found" });

            return Ok(result);
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

        [HttpGet("avatar/upload-url")]
        public async Task<IActionResult> GetAvatarUploadUrl(string fileName, string contentType)
        {
            var (url, filePath) = await _fileStorageService.GetPresignedPutUrlAsync("students/avatars", fileName, contentType);
            return Ok(new
            {
                uploadUrl = url,
                filePath = filePath,
                publicUrl = _fileStorageService.GetPublicUrl(filePath)
            });
        }


    }
}
