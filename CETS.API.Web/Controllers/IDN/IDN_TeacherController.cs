using Application.Interfaces.Common.Storage;
using Application.Interfaces.IDN;
using DTOs.IDN.IDN_Teacher.Requests;
using Infrastructure.Implementations.Common.Storage;
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
        private readonly IFileStorageService _fileStorageService;

        public IDN_TeacherController(ILogger<IDN_TeacherController> logger, IIDN_TeacherService teacherService, IFileStorageService fileStorageService)
        {
            _logger = logger;
            _teacherService = teacherService;
            _fileStorageService = fileStorageService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeachersAsync()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTeacherByIdAsync(Guid id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> GetTeacherDetailsAsync(Guid id)
        {
            var teacher = await _teacherService.GetTeacherDetailsAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }



        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetTeacherByCodeAsync(string code)
        {
            var teacher = await _teacherService.GetTeacherByCodeAsync(code);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetTeacherByEmailAsync(string email)
        {
            var teacher = await _teacherService.GetTeacherByEmailAsync(email);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacherAsync([FromBody] CreateTeacherRequest dto)
        {
            try
            {
                var teacher = await _teacherService.CreateTeacherWithAccountAsync(dto);
                return Ok(teacher);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("InvalidOperationException when creating teacher: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("ArgumentException when creating teacher: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating teacher");
                return StatusCode(500, new { message = "An error occurred while creating the teacher. Please try again." });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTeacherAsync(Guid id, [FromBody] UpdateTeacherRequest dto)
        {
            var updatedTeacher = await _teacherService.UpdateTeacherAsync(id, dto);
            if (updatedTeacher == null)
            {
                return NotFound();
            }
            return Ok(updatedTeacher);
        }

        [HttpPatch("updateprofile/{id:guid}")]
        public async Task<IActionResult> PatchTeacherProfileAsync(
            Guid id,
            [FromBody] UpdateTeacherProfileRequest dto)
        {
            string role = "AcademicStaff";

            bool isTeacherSelf = true; 
            bool isStaff = role == "AcademicStaff" || role == "Admin";

            if (role == "Teacher" && isTeacherSelf)
            {
                if (dto.TeacherCode != null ||
                    dto.CID != null )
                {
                    return BadRequest("Teacher không được phép cập nhật TeacherCode, CID");
                }
            }

            if (role != "Teacher" && !isStaff)
            {
                return Forbid();
            }

            var updated = await _teacherService.UpdateTeacherProfileAsync(id, dto, User);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        [HttpPatch("restore/{id:guid}")]
        public async Task<IActionResult> RestoreTeacherAsync(Guid id)
        {
            var restored = await _teacherService.RestoreTeacherAsync(id);
            if (restored == null)
            {
                return NotFound();
            }
            return Ok(restored);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> SoftDeleteTeacherAsync(Guid id)
        {
            var deleted = await _teacherService.SoftDeleteTeacherAsync(id);
            if (deleted == null)
            {
                return NotFound();
            }
            return Ok(deleted);
        }

        [HttpGet("avatar/upload-url")]
        public async Task<IActionResult> GetAvatarUploadUrl(string fileName, string contentType)
        {
            var (url, filePath) = await _fileStorageService.GetPresignedPutUrlAsync("teacher", fileName, contentType);
            return Ok(new
            {
                uploadUrl = url,
                filePath = filePath,
                publicUrl = _fileStorageService.GetPublicUrl(filePath)
            });
        }
    }
}
