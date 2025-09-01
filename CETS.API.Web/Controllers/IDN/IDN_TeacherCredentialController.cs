using Application.Interfaces.IDN;
using DTOs.IDN_TeacherCredential.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_TeacherCredentialController : ControllerBase
    {
        private readonly IIDN_TeacherCredentialService _teacherCredentialService;
        public IDN_TeacherCredentialController(IIDN_TeacherCredentialService teacherCredentialService)
        {
            _teacherCredentialService = teacherCredentialService;
        }

        [HttpGet("credentials/{teacherId:guid}")]
        public async Task<IActionResult> GetCredentialsByTeacherId(Guid teacherId)
        {
            var credentials = await _teacherCredentialService.GetCredentialsByTeacherIdAsync(teacherId);
            return Ok(credentials);
        }

        [HttpGet("credentials/code/{teacherCode}")]
        public async Task<IActionResult> GetCredentialsByTeacherCode(string teacherCode)
        {
            var credentials = await _teacherCredentialService.GetCredentialsByTeacherCodeAsync(teacherCode);
            return Ok(credentials);
        }

        [HttpGet("credential-types")]
        public async Task<IActionResult> GetCredentialTypes()
        {
            var credentialTypes = await _teacherCredentialService.GetCredentialTypesAsync();
            return Ok(credentialTypes);
        }

        [HttpPost("credentials")]
        public async Task<IActionResult> CreateCredential([FromBody] CreateTeacherCredentialRequest credential)
        {
            if (credential == null)
            {
                return BadRequest("Credential data is required.");
            }
            var createdCredential = await _teacherCredentialService.CreateAsync(credential);
            return Created("New credential created", createdCredential);
        }

        [HttpPut("credentials/{credentialId:guid}")]
        public async Task<IActionResult> UpdateCredential(Guid credentialId, [FromBody] UpdateTeacherCredentialRequest credential)
        {
            var updatedCredential = await _teacherCredentialService.UpdateAsync(credentialId, credential);
            if (updatedCredential == null)
            {
                return NotFound();
            }
            return Ok(updatedCredential);
        }

        [HttpDelete("credentials/{credentialId:guid}")]
        public async Task<IActionResult> DeleteCredential(Guid credentialId)
        {
            await _teacherCredentialService.DeleteAsync(credentialId);
            return NoContent();
        }

    }
}
