using Application.Implementations.ACAD;
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

        /// <summary>
        /// Lấy danh sách submissions theo assignmentId và assignmentSkill (reading, writing, speaking, listening)
        /// </summary>
        /// <param name="assignmentId">ID của assignment</param>
        /// <param name="assignmentSkill">Skill code: reading, writing, speaking, listening</param>
        /// <returns>Danh sách submissions</returns>
        [HttpGet]
        [Route("api/submissions")]
        public async Task<IActionResult> GetSubmissions([FromQuery] Guid assignmentId, [FromQuery] string? assignmentSkill)
        {
            try
            {
                var submissions = await _submissionService.GetSubmissionsByAssignmentAndSkillAsync(assignmentId, assignmentSkill);
                return Ok(new
                {
                    success = true,
                    data = submissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submissions for assignment {AssignmentId} with skill {AssignmentSkill}", assignmentId, assignmentSkill);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
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
        public async Task<ActionResult<SubmitAssignmentRequest>> Submit([FromBody] SubmitAssignmentRequest request)
        {
            var result = await _submissionService.SubmitAssignmentAsync(request);
            return Ok(result);
        }

     
        [HttpPost("speaking-upload-urls")]
        public async Task<ActionResult<SpeakingSubmissionUploadUrlsResponse>> GetSpeakingSubmissionUploadUrls([FromBody] GetSpeakingSubmissionUploadUrlsRequest request)
        {
            try
            {
                var result = await _submissionService.GetSpeakingSubmissionUploadUrlsAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upload URLs for speaking assignment {AssignmentId} and student {StudentId}", request.AssignmentID, request.StudentID);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

     
        [HttpPost("submit-speaking")]
        public async Task<ActionResult<SubmissionResponse>> SubmitSpeakingSubmission([FromBody] SubmitSpeakingSubmissionRequest request)
        {
            try
            {
                var result = await _submissionService.SubmitSpeakingSubmissionAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing speaking submission for assignment {AssignmentId} and student {StudentId}", request.AssignmentID, request.StudentID);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

       
        [HttpPost("start-attempt")]
        public async Task<ActionResult<SubmissionResponse>> StartAttempt([FromBody] StartAttemptRequest request)
        {
            try
            {
                var result = await _submissionService.StartAttemptAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting attempt for assignment {AssignmentId} and student {StudentId}", request.AssignmentID, request.StudentID);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
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
        /// Lấy danh sách submissions với download URLs theo AssignmentId
        /// </summary>
        /// <param name="assignmentId">ID của assignment</param>
        /// <returns>Assignment info và danh sách submissions với download URLs</returns>
        [HttpGet("assignment/{assignmentId}/downloads")]
        public async Task<IActionResult> GetSubmissionsWithDownloadUrls(Guid assignmentId)
        {
            try
            {
                var result = await _submissionService.GetSubmissionsWithDownloadUrlsAsync(assignmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submissions with download URLs for assignment {AssignmentId}", assignmentId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        [HttpGet("download/{id}")]
        public async Task<ActionResult<string>> GetDownloadUrl(Guid id)
        {
            try
            {
                var submission = await _submissionService.GetSubmissionByIdAsync(id);
                if (submission == null)
                    return NotFound("Submission not found");

                var downloadUrl = await _submissionService.GetDownloadUrlAsync(id);

                return Ok(new
                {
                    downloadUrl,
                    submissionInfo = new
                    {
                        id = submission.Id,
                        createdAt = submission.CreatedAt
                    }
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Submission not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting download URL for assignment {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Bulk update multiple student submissions (scores and/or feedback)
        /// </summary>
        /// <param name="request">Request containing list of submission updates</param>
        /// <returns>Result with updated count and details for each submission</returns>
        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdateSubmissions([FromBody] BulkUpdateSubmissionsRequest request)
        {
            try
            {
                var result = await _submissionService.BulkUpdateSubmissionsAsync(request);
                // Return 207 Multi-Status if there are partial failures
                if (result.Data.FailedCount > 0 && result.Data.UpdatedCount > 0)
                {
                    return StatusCode(207, result);
                }
                // Return 200 OK if all succeeded
                if (result.Data.FailedCount == 0)
                {
                    return Ok(result);
                }
                // Return 400 Bad Request if all failed
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk update on submissions");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = "An unexpected error occurred while processing your request"
                });
            }
        }

        [HttpPost("SubmitWritingSubmisson")]
        public async Task<IActionResult> SubmitWritingSubmisson([FromForm] SubmitWritingSubmissionRequest request)
        {
            try
            {
                // Validate file exists
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "File is required"
                    });
                }

                // Validate file type (docx, doc, or pdf) - get extension from actual file
                var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
                if (string.IsNullOrEmpty(fileExtension) || 
                    (fileExtension != ".docx" && fileExtension != ".doc" && fileExtension != ".pdf"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Only DOCX, DOC, and PDF files are allowed"
                    });
                }

                // Submit writing assignment - this will extract text, grade by AI, and save to database
                var result = await _submissionService.SubmitWritingAssignmentAsync(request);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        submissionId = result.Id,
                        score = result.Score,
                        feedback = result.Feedback,
                        isAiScore = result.IsAiScore,
                        uploadUrl = result.UploadUrl,
                        storeUrl = result.StoreUrl,
                        submittedAt = result.CreatedAt
                    }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for writing assignment submission");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Unsupported file type for writing assignment submission");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting writing assignment for student {StudentId}", request.StudentId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }
    }
}
