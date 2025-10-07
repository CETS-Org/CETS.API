using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Submission.Requests;
using DTOs.ACAD.ACAD_Submission.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_SubmissionsController : ControllerBase
    {
        private readonly ILogger<ACAD_SubmissionsController> _logger;
        private readonly IACAD_SubmissionService _submissionService;

        public ACAD_SubmissionsController(ILogger<ACAD_SubmissionsController> logger, IACAD_SubmissionService submissionService)
        {
            _logger = logger;
            _submissionService = submissionService;
        }

        [HttpGet("courses/assignments-summary/{courseId}/students/{studentId}")]
        public async Task<IActionResult> GetAssignmentsSummary(Guid courseId, Guid studentId)
        {
            var (submitted, total) = await _submissionService.GetAssignmentsSubmittedSummaryAsync(studentId, courseId);

            if (total == 0)
                return NotFound(new { message = "Course này chưa có assignment nào" });

            return Ok(new { submitted, total, summary = $"{submitted}/{total} assignments submitted" });
        }

        [HttpPost("submit")]
        public async Task<ActionResult<SubmissionResponse>> Submit([FromBody] SubmitAssignmentRequest request)
        {
            var result = await _submissionService.SubmitAssignmentAsync(request);
            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<SubmissionDetailResponse>> GetDetail(Guid id)
        {
            var result = await _submissionService.GetSubmissionsByAssignmentAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
