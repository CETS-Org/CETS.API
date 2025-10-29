using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseWishlist.Requests;
using DTOs.ACAD.ACAD_CourseWishlist.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseWishlistController : ODataController
    {
        private readonly ILogger<ACAD_CourseWishlistController> _logger;
        private readonly IACAD_CourseWishlistService _wishlistService;

        public ACAD_CourseWishlistController(
            ILogger<ACAD_CourseWishlistController> logger,
            IACAD_CourseWishlistService wishlistService)
        {
            _logger = logger;
            _wishlistService = wishlistService;
        }

        /// <summary>
        /// Add a course to student's wishlist
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddCourseToWishlistAsync([FromBody] AddToWishlistRequest request)
        {
            try
            {
                var result = await _wishlistService.AddCourseToWishlistAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entity not found while adding to wishlist");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while adding to wishlist");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course to wishlist");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a course is in student's wishlist
        /// </summary>
        [HttpGet("check")]
        public async Task<ActionResult<bool>> IsCourseInWishlistAsync([FromQuery] Guid studentId, [FromQuery] Guid courseId)
        {
            try
            {
                var result = await _wishlistService.IsCourseInWishlistAsync(studentId, courseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking wishlist status");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all wishlist items for a student
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<WishlistItemResponse>>> GetStudentWishlistAsync(Guid studentId)
        {
            try
            {
                var result = await _wishlistService.GetStudentWishlistAsync(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student wishlist for StudentId {StudentId}", studentId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a course from student's wishlist
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> RemoveCourseFromWishlistAsync([FromQuery] Guid studentId, [FromQuery] Guid courseId)
        {
            try
            {
                await _wishlistService.RemoveCourseFromWishlistAsync(studentId, courseId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Wishlist item not found for removal");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing course from wishlist");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

