using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_LearningMaterial.Requests;
using DTOs.ACAD.ACAD_LearningMaterial.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_LearningMaterialController : ControllerBase
    {
        private readonly ILogger<ACAD_LearningMaterialController> _logger;
        private readonly IACAD_LearningMaterialService _learningMaterialService;

        public ACAD_LearningMaterialController(
            ILogger<ACAD_LearningMaterialController> logger,
            IACAD_LearningMaterialService learningMaterialService)
        {
            _logger = logger;
            _learningMaterialService = learningMaterialService;
        }

        /// <summary>
        /// Create a new learning material and get upload URL
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LearningMaterialUploadResponse>> CreateLearningMaterial([FromBody] CreateLearningMaterialRequest request)
        {
            try
            {
                var result = await _learningMaterialService.CreateLearningMaterialAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating learning material");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLearningMaterial(Guid id, [FromBody] UpdateLearningMaterialRequest request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest("ID mismatch");

                var result = await _learningMaterialService.UpdateLearningMaterialAsync(request);
                
                if (result != null)
                {
                    return Ok(result);
                }
                
                // For metadata-only updates, return NoContent
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Learning material not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning material with ID {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete learning material (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLearningMaterial(Guid id)
        {
            try
            {
                await _learningMaterialService.DeleteLearningMaterialAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Learning material not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting learning material with ID {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get learning material by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LearningMaterialResponse>> GetLearningMaterial(Guid id)
        {
            try
            {
                var result = await _learningMaterialService.GetLearningMaterialByIdAsync(id);
                if (result == null)
                    return NotFound("Learning material not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning material with ID {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

     
        /// <summary>
        /// Get learning materials by class meeting (session)
        /// </summary>
        [HttpGet("class-meeting/{classMeetingId}")]
        public async Task<ActionResult<IEnumerable<LearningMaterialResponse>>> GetLearningMaterialsByClassMeeting(Guid classMeetingId)
        {
            try
            {
                var result = await _learningMaterialService.GetLearningMaterialsByClassMeetingAsync(classMeetingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning materials for class meeting {ClassMeetingId}", classMeetingId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get learning materials by uploader
        /// </summary>
        [HttpGet("uploader/{uploaderId}")]
        public async Task<ActionResult<IEnumerable<LearningMaterialResponse>>> GetLearningMaterialsByUploader(Guid uploaderId)
        {
            try
            {
                var result = await _learningMaterialService.GetLearningMaterialsByUploaderAsync(uploaderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning materials for uploader {UploaderId}", uploaderId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

      

        /// <summary>
        /// Get download URL for learning material
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<ActionResult<string>> GetDownloadUrl(Guid id)
        {
            try
            {
                var material = await _learningMaterialService.GetLearningMaterialByIdAsync(id);
                if (material == null)
                    return NotFound("Learning material not found");

                var downloadUrl = await _learningMaterialService.GetDownloadUrlAsync(id);
                
                return Ok(new { 
                    downloadUrl,
                    materialInfo = new {
                        id = material.Id,
                        title = material.Title,
                        storeUrl = material.StoreUrl,
                        createdAt = material.CreatedAt
                    }
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Learning material not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting download URL for learning material {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
