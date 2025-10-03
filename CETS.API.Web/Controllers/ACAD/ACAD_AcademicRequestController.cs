using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_AcademicRequest.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_AcademicRequestController : ControllerBase
    {
        private readonly IACAD_AcademicRequestService _academicRequestService;

        public ACAD_AcademicRequestController(IACAD_AcademicRequestService academicRequestService)
        {
            _academicRequestService = academicRequestService;
        }

        // GET api/academicrequest/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetByStudent(Guid studentId)
        {
            var result = await _academicRequestService.GetRequestsByStudentAsync(studentId);
            return Ok(result);
        }

        // GET api/academicrequest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AcademicRequestResponse>> GetDetails(Guid id)
        {
            var result = await _academicRequestService.GetDetailsAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
