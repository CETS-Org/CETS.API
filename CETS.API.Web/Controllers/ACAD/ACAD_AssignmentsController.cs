using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
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

        public ACAD_AssignmentsController(
            ILogger<ACAD_AssignmentsController> logger,
            IACAD_AssignmentService AssignmentService)
        {
            _logger = logger;
            _AssignmentService = AssignmentService;
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
                
                return Ok(new { 
                    downloadUrl,
                    assignmentInfo = new {
                        id = assignment.Id,
                        title = assignment.Title,
                        dueDate = assignment.DueDate,
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

        /// <summary>
        /// Update an existing assignment
        /// </summary>
        /// <param name="id">Assignment ID</param>
        /// <param name="request">Update assignment request</param>
        /// <returns>Updated assignment response</returns>
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

    }
}
