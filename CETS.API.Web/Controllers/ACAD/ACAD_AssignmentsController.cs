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

        /// <summary>
        /// Lấy danh sách assignments theo classMeetingId kèm số lượng submissions
        /// </summary>
        /// <param name="classMeetingId">ID của buổi học</param>
        /// <returns>Danh sách assignments với count submissions</returns>
        [HttpGet("class-Assignment/{classMeetingId}")]
        public async Task<IActionResult> GetAssignmentsWithSubmissionCount(Guid classMeetingId)
        {
            var result = await _AssignmentService.GetAssignmentsWithSubmissionCountAsync(classMeetingId);
            return Ok(result);
        }

    }
}
