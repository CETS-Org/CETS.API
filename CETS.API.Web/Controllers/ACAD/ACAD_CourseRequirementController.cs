using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseRequirement.Requests;
using DTOs.ACAD.ACAD_CourseRequirement.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseRequirementController : ControllerBase
    {
        private readonly IACAD_CourseRequirementService _courseRequirementService;

        public ACAD_CourseRequirementController(IACAD_CourseRequirementService courseRequirementService)
        {
            _courseRequirementService = courseRequirementService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseRequirementResponse>>> GetAll()
        {
            try
            {
                var requirements = await _courseRequirementService.GetAllCourseRequirementsAsync();
                return Ok(requirements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseRequirementResponse>> GetById(Guid id)
        {
            try
            {
                var requirement = await _courseRequirementService.GetCourseRequirementByIdAsync(id);
                if (requirement == null)
                    return NotFound();

                return Ok(requirement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<CourseRequirementResponse>>> GetByCourseId(Guid courseId)
        {
            try
            {
                var requirements = await _courseRequirementService.GetRequirementsByCourseIdAsync(courseId);
                return Ok(requirements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateCourseRequirementRequest request)
        {
            try
            {
                var id = await _courseRequirementService.CreateCourseRequirementAsync(request);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateCourseRequirementRequest request)
        {
            try
            {
                await _courseRequirementService.UpdateCourseRequirementAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _courseRequirementService.DeleteCourseRequirementAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
