using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_AcademicRequest.Requests;
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

        // POST api/academicrequest - Submit a new academic request
        [HttpPost]
        public async Task<ActionResult<AcademicRequestResponse>> SubmitRequest([FromBody] CreateAcademicRequest requestDto)
        {
            var result = await _academicRequestService.SubmitRequestAsync(requestDto);
            return Ok(result);
        }

        // GET api/academicrequest/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetByStudent(Guid studentId)
        {
            var result = await _academicRequestService.GetRequestsByStudentAsync(studentId);
            return Ok(result);
        }

        // GET api/academicrequest - Get all requests (for staff)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetAll()
        {
            var result = await _academicRequestService.GetAllRequestsAsync();
            return Ok(result);
        }

        // GET api/academicrequest/status/{statusId} - Get requests by status (for staff)
        [HttpGet("status/{statusId}")]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetByStatus(Guid statusId)
        {
            var result = await _academicRequestService.GetRequestsByStatusAsync(statusId);
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

        // PUT api/academicrequest/process - Process (approve/reject) a request (for staff)
        [HttpPut("process")]
        public async Task<IActionResult> ProcessRequest([FromBody] ProcessAcademicRequest requestDto)
        {
            await _academicRequestService.ProcessRequestAsync(requestDto);
            return Ok(new { message = "Request processed successfully" });
        }
    }
}
