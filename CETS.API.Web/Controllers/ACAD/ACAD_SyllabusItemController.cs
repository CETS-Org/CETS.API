using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_SyllabusItem.Requests;
using DTOs.ACAD.ACAD_SyllabusItem.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_SyllabusItemController : ControllerBase
    {
        private readonly ILogger<ACAD_SyllabusItemController> _logger;
        private readonly IACAD_SyllabusService _syllabusService;

        public ACAD_SyllabusItemController(
            ILogger<ACAD_SyllabusItemController> logger,
            IACAD_SyllabusService syllabusService)
        {
            _logger = logger;
            _syllabusService = syllabusService;
        }

        /// <summary>
        /// Create a new syllabus item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SyllabusItemResponse>> AddSyllabusItemAsync([FromBody] CreateSyllabusItemRequest request)
        {
            try
            {
                var item = await _syllabusService.AddSyllabusItemAsync(request);
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating syllabus item");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all items for a syllabus
        /// </summary>
        [HttpGet("syllabus/{syllabusId}")]
        public async Task<ActionResult<IEnumerable<SyllabusItemResponse>>> GetItemsBySyllabusAsync(Guid syllabusId)
        {
            try
            {
                var items = await _syllabusService.GetItemsBySyllabusAsync(syllabusId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving syllabus items for syllabus {SyllabusId}", syllabusId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update a syllabus item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SyllabusItemResponse>> UpdateSyllabusItemAsync(Guid id, [FromBody] UpdateSyllabusItemRequest request)
        {
            try
            {
                if (id != request.SyllabusItemID)
                    return BadRequest("ID mismatch");

                var item = await _syllabusService.UpdateSyllabusItemAsync(request);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Syllabus item not found for update with ID {ItemId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating syllabus item with ID {ItemId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Soft delete a syllabus item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSyllabusItemAsync(Guid id)
        {
            try
            {
                await _syllabusService.SoftDeleteSyllabusItemAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Syllabus item not found for deletion with ID {ItemId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting syllabus item with ID {ItemId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

