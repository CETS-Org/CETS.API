using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Syllabus.Requests;
using DTOs.ACAD.ACAD_Syllabus.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_SyllabusController : ControllerBase
    {
        private readonly ILogger<ACAD_SyllabusController> _logger;
        private readonly IACAD_SyllabusService _syllabusService;

        public ACAD_SyllabusController(
            ILogger<ACAD_SyllabusController> logger,
            IACAD_SyllabusService syllabusService)
        {
            _logger = logger;
            _syllabusService = syllabusService;
        }

        /// <summary>
        /// Create a new syllabus
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SyllabusResponse>> CreateSyllabusAsync([FromBody] CreateSyllabusRequest request)
        {
            try
            {
                var syllabus = await _syllabusService.CreateSyllabusAsync(request);
                return Ok(syllabus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating syllabus");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all syllabi for a course
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<SyllabusResponse>>> GetSyllabiByCourseAsync(Guid courseId)
        {
            try
            {
                var syllabi = await _syllabusService.GetSyllabiByCourseAsync(courseId);
                return Ok(syllabi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving syllabi for course {CourseId}", courseId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update a syllabus
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SyllabusResponse>> UpdateSyllabusAsync(Guid id, [FromBody] UpdateSyllabusRequest request)
        {
            try
            {
                if (id != request.SyllabusID)
                    return BadRequest("ID mismatch");

                var syllabus = await _syllabusService.UpdateAsync(id, request);
                return Ok(syllabus);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Syllabus not found for update with ID {SyllabusId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating syllabus with ID {SyllabusId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a syllabus
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSyllabusAsync(Guid id)
        {
            try
            {
                await _syllabusService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Syllabus not found for deletion with ID {SyllabusId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting syllabus with ID {SyllabusId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

