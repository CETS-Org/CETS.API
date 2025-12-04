using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Course.Requests;
using DTOs.ACAD.ACAD_CoursePackage.Requests;
using DTOs.ACAD.ACAD_CoursePackage.Responses;
using DTOs.ACAD.ACAD_CoursePackage.Search;
using DTOs.ACAD.ACAD_CoursePackageItem.Requests;
using DTOs.ACAD.ACAD_CoursePackageItem.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CoursePackageController : ControllerBase
    {
        private readonly IACAD_CoursePackageService _coursePackageService;
        private readonly ILogger<ACAD_CoursePackageController> _logger;

        public ACAD_CoursePackageController(
            IACAD_CoursePackageService coursePackageService,
            ILogger<ACAD_CoursePackageController> logger)
        {
            _coursePackageService = coursePackageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all course packages
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CoursePackageResponse>>> GetAll()
        {
            try
            {
                var packages = await _coursePackageService.GetAllPackagesAsync();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all course packages");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all active course packages
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CoursePackageResponse>>> GetActivePackages()
        {
            try
            {
                var packages = await _coursePackageService.GetActivePackagesAsync();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active course packages");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course package by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CoursePackageResponse>> GetById(Guid id)
        {
            try
            {
                var package = await _coursePackageService.GetPackageByIdAsync(id);
                if (package == null)
                    return NotFound($"Course package with ID {id} not found");

                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course package with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course package detail with courses
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<CoursePackageDetailResponse>> GetPackageDetail(Guid id)
        {
            try
            {
                var package = await _coursePackageService.GetPackageDetailAsync(id);
                if (package == null)
                    return NotFound($"Course package with ID {id} not found");

                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course package detail with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new course package
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreatePackage([FromBody] CreateCoursePackageRequest request)
        {
            try
            {
                var packageId = await _coursePackageService.CreatePackageAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = packageId }, packageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course package");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing course package
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdateCoursePackageRequest request)
        {
            try
            {
                if (id != request.Id)
                    return BadRequest("Package ID mismatch");

                await _coursePackageService.UpdatePackageAsync(request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course package not found for update with ID {PackageId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course package with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a course package (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            try
            {
                await _coursePackageService.SoftDeletePackageAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course package not found for deletion with ID {PackageId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course package with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Activate a course package
        /// </summary>
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivatePackage(Guid id)
        {
            try
            {
                await _coursePackageService.ActivatePackageAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course package not found for activation with ID {PackageId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating course package with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivate a course package
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivatePackage(Guid id)
        {
            try
            {
                await _coursePackageService.DeactivatePackageAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course package not found for deactivation with ID {PackageId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating course package with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get package items for a specific package
        /// </summary>
        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<CoursePackageItemResponse>>> GetPackageItems(Guid id)
        {
            try
            {
                var items = await _coursePackageService.GetPackageItemsAsync(id);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving package items for package ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add a course to a package
        /// </summary>
        [HttpPost("{id}/courses")]
        public async Task<IActionResult> AddCourseToPackage(Guid id, [FromBody] AddCourseToPackageRequest request)
        {
            try
            {
                await _coursePackageService.AddCourseToPackageAsync(id, request);
                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course to package with ID {PackageId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a course from a package
        /// </summary>
        [HttpDelete("{id}/courses/{courseId}")]
        public async Task<IActionResult> RemoveCourseFromPackage(Guid id, Guid courseId)
        {
            try
            {
                var request = new RemoveCourseFromPackageRequest
                {
                    PackageID = id,
                    CourseID = courseId
                };

                await _coursePackageService.RemoveCourseFromPackageAsync(request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course package item not found for removal. PackageId: {PackageId}, CourseId: {CourseId}", id, courseId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing course from package. PackageId: {PackageId}, CourseId: {CourseId}", id, courseId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update package item sequence
        /// </summary>
        [HttpPatch("items/{itemId}/sequence")]
        public async Task<IActionResult> UpdatePackageItemSequence(Guid itemId, [FromBody] int newSequence)
        {
            try
            {
                await _coursePackageService.UpdatePackageItemSequenceAsync(itemId, newSequence);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course package item not found for sequence update with ID {ItemId}", itemId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package item sequence with ID {ItemId}", itemId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Advanced search with basic query parameters for course packages
        /// </summary>
        [HttpGet("search-basic")]
        public async Task<ActionResult<CoursePackageSearchResult>> SearchBasicAsync([FromQuery] CoursePackageSearchQuery query, CancellationToken ct)
        {
            try
            {
                var result = await _coursePackageService.SearchBasicAsync(query, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing basic search for course packages");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get package statistics including total packages, active packages, revenue from sales, and packages sold
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<CoursePackageStatisticsResponse>> GetStatistics()
        {
            try
            {
                var statistics = await _coursePackageService.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course package statistics");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get presigned URL for uploading package image directly to R2
        /// </summary>
        [HttpPost("image-upload-url")]
        public async Task<ActionResult<CoursePackageImageUploadResponse>> GetImageUploadUrlAsync([FromBody] ImageUploadRequest request)
        {
            try
            {
                var response = await _coursePackageService.GetImageUploadUrlAsync(request.FileName, request.ContentType);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image upload URL for package");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
