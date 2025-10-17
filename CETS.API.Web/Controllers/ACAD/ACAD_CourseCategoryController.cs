using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseCategory.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseCategoryController : ControllerBase
    {
        private readonly IACAD_CourseCategoryService _categoryService;
        private readonly ILogger<ACAD_CourseCategoryController> _logger;

        public ACAD_CourseCategoryController(
            IACAD_CourseCategoryService categoryService,
            ILogger<ACAD_CourseCategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all course categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseCategoryResponse>>> GetAllAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCourseCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all course categories");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a course category by Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CourseCategoryResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCourseCategoryByIdAsync(id);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course category with ID {CategoryId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


