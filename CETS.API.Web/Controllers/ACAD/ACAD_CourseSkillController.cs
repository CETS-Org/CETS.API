using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseSkill.Requests;
using DTOs.ACAD.ACAD_CourseSkill.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseSkillController : ControllerBase
    {
        private readonly IACAD_CourseSkillService _courseSkillService;

        public ACAD_CourseSkillController(IACAD_CourseSkillService courseSkillService)
        {
            _courseSkillService = courseSkillService;
        }

        /// <summary>
        /// Get all course skills
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseSkillResponse>>> GetAll()
        {
            try
            {
                var courseSkills = await _courseSkillService.GetAllCourseSkillsAsync();
                return Ok(courseSkills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course skill by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseSkillResponse>> GetById(Guid id)
        {
            try
            {
                var courseSkill = await _courseSkillService.GetCourseSkillByIdAsync(id);
                if (courseSkill == null)
                    return NotFound();

                return Ok(courseSkill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course skills by course ID
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<CourseSkillResponse>>> GetByCourse(Guid courseId)
        {
            try
            {
                var courseSkills = await _courseSkillService.GetCourseSkillsByCourseAsync(courseId);
                return Ok(courseSkills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course skills by skill ID
        /// </summary>
        [HttpGet("skill/{skillId}")]
        public async Task<ActionResult<IEnumerable<CourseSkillResponse>>> GetBySkill(Guid skillId)
        {
            try
            {
                var courseSkills = await _courseSkillService.GetCourseSkillsBySkillAsync(skillId);
                return Ok(courseSkills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get course skill by course ID and skill ID
        /// </summary>
        [HttpGet("course/{courseId}/skill/{skillId}")]
        public async Task<ActionResult<CourseSkillResponse>> GetByCourseAndSkill(Guid courseId, Guid skillId)
        {
            try
            {
                var courseSkill = await _courseSkillService.GetCourseSkillByCourseAndSkillAsync(courseId, skillId);
                if (courseSkill == null)
                    return NotFound();

                return Ok(courseSkill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new course skill relationship
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSkillRequest request)
        {
            try
            {
                var id = await _courseSkillService.CreateCourseSkillAsync(request);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing course skill relationship
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseSkillRequest request)
        {
            try
            {
                await _courseSkillService.UpdateCourseSkillAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a course skill relationship
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _courseSkillService.DeleteCourseSkillAsync(id);
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
