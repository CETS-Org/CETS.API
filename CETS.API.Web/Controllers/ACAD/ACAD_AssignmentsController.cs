using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using Application.Interfaces.Common.Storage;
using DTOs.ACAD.ACAD_Assignment.Requests;
using DTOs.ACAD.ACAD_Assignment.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_AssignmentsController : ControllerBase
    {
        private readonly ILogger<ACAD_AssignmentsController> _logger;
        private readonly IACAD_AssignmentService _AssignmentService;
        private readonly IFileStorageService _fileStorageService;

        public ACAD_AssignmentsController(
            ILogger<ACAD_AssignmentsController> logger,
            IACAD_AssignmentService AssignmentService,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _AssignmentService = AssignmentService;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Create a new assignment with file upload
        /// </summary>
        [HttpPost("create-assignment")]
        public async Task<ActionResult<AssignmentUploadResponse>> CreateAssignmentWithFile([FromBody] CreateAssignmentWithFileRequest request)
        {
            try
            {
                var result = await _AssignmentService.CreateAssignmentWithFileAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment with file");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new quiz assignment with questions
        /// </summary>
        [HttpPost("create-quiz-assignment")]
        public async Task<ActionResult<QuizAssignmentResponse>> CreateQuizAssignment([FromBody] CreateQuizAssignmentRequest request)
        {
            try
            {
                var result = await _AssignmentService.CreateQuizAssignmentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz assignment");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new speaking assignment with questions
        /// </summary>
        [HttpPost("create-speaking-assignment")]
        public async Task<ActionResult<SpeakingAssignmentResponse>> CreateSpeakingAssignment([FromBody] CreateSpeakingAssignmentRequest request)
        {
            try
            {
                var result = await _AssignmentService.CreateSpeakingAssignmentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating speaking assignment");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpGet("class-meeting/{classMeetingId}/student/{studentId}/assignments")]
        public async Task<IActionResult> GetAssignmentsAndSubmissions(Guid classMeetingId, Guid studentId)
        {
            var result = await _AssignmentService.GetAssignmentsWithSubmissions(classMeetingId, studentId);
            return Ok(result);
        }

        /// <summary>
        /// Get assignment by ID (works for Homework, Quiz, and Speaking assignments)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AssignmentResponse>> GetAssignmentById(Guid id)
        {
            try
            {
                var result = await _AssignmentService.GetAssignmentByIdAsync(id);
                if (result == null)
                    return NotFound("Assignment not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        /// <summary>
        /// Get download URL for assignment
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<ActionResult<string>> GetDownloadUrl(Guid id)
        {
            try
            {
                var assignment = await _AssignmentService.GetAssignmentByIdAsync(id);
                if (assignment == null)
                    return NotFound("Assignment not found");

                var downloadUrl = await _AssignmentService.GetDownloadUrlAsync(id);

                return Ok(new
                {
                    downloadUrl,
                    assignmentInfo = new
                    {
                        id = assignment.Id,
                        title = assignment.Title,
                        dueDate = assignment.DueAt,
                        createdAt = assignment.CreatedAt
                    }
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Assignment not found");
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



        [HttpPut("update/{id}")]
        public async Task<ActionResult<AssignmentResponse>> UpdateAssignment(Guid id, [FromBody] UpdateAssignmentRequest request)
        {
            try
            {
                // Set the ID from route parameter to ensure consistency
                request.Id = id;

                var result = await _AssignmentService.UpdateAssignmentAsync(request);
                if (result == null)
                    return NotFound("Assignment not found");

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Assignment not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(Guid id)
        {
            try
            {
                await _AssignmentService.DeleteAssignmentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Assignment not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get presigned URL for question data (used when taking test, viewing details, or editing)
        /// </summary>
        [HttpGet("{id}/question-data-url")]
        public async Task<ActionResult<string>> GetQuestionDataUrl(Guid id)
        {
            try
            {
                var questionDataUrl = await _AssignmentService.GetQuestionDataUrlAsync(id);
                return Ok(new { questionDataUrl });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Assignment not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question data URL for assignment {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("question-json-upload-url")]
        public async Task<IActionResult> GetQuestionJsonUploadUrl([FromQuery] string fileName = "quiz-assignment.json")
        {
            try
            {
                var (uploadUrl, filePath) = await _fileStorageService.GetPresignedPutUrlAsync("assignments/questions", fileName, "application/json");
                return Ok(new
                {
                    uploadUrl = uploadUrl,
                    filePath = filePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting presigned URL for question JSON upload");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("audio-url")]
        public async Task<IActionResult> GetAudioUploadUrl(string fileName)
        {
            string contentType = "audio/mpeg";
            var (url, filePath) = await _fileStorageService.GetPresignedPutUrlAsync("assignments/audio", fileName, contentType);

            return Ok(new
            {
                uploadUrl = url,
                filePath = filePath,
                publicUrl = _fileStorageService.GetPublicUrl(filePath)
            });
        }
    }
}
