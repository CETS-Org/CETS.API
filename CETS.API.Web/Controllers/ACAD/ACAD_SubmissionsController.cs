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

        /// <summary>
        /// Lấy danh sách submissions theo AssignmentId
        /// </summary>
        /// <param name="assignmentId">ID của assignment</param>
        /// <returns>Danh sách submissions của assignment đó</returns>
        [HttpGet("assignment/{assignmentId}")]
        public async Task<IActionResult> GetSubmissionsByAssignment(Guid assignmentId)
        {
            var result = await _submissionService.GetSubmissionsByAssignmentAsync(assignmentId);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật điểm cho submission
        /// </summary>
        /// <param name="request">Request bao gồm SubmissionId và Score</param>
        /// <returns>Submission đã được cập nhật điểm</returns>
        [HttpPut("update-score")]
        public async Task<IActionResult> UpdateScore([FromBody] UpdateSubmissionScoreRequest request)
        {
            try
            {
                var result = await _submissionService.UpdateScoreAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật feedback cho submission
        /// </summary>
        /// <param name="request">Request bao gồm SubmissionId và Feedback</param>
        /// <returns>Submission đã được cập nhật feedback</returns>
        [HttpPut("update-feedback")]
        public async Task<IActionResult> UpdateFeedback([FromBody] UpdateSubmissionFeedbackRequest request)
        {
            try
            {
                var result = await _submissionService.UpdateFeedbackAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
