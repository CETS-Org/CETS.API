using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_AssignmentsController : ControllerBase
    {
        private readonly IACAD_AssignmentService _AssignmentService;

        public ACAD_AssignmentsController(IACAD_AssignmentService AssignmentService)
        {
            _AssignmentService = AssignmentService;
        }

        [HttpGet("class-meeting/{classMeetingId}/student/{studentId}/assignments")]
        public async Task<IActionResult> GetAssignmentsAndSubmissions(Guid classMeetingId, Guid studentId)
        {
            var result = await _AssignmentService.GetAssignmentsWithSubmissions(classMeetingId, studentId);
            return Ok(result);
        }


    }
}
