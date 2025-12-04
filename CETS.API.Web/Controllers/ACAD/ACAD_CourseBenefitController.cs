using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseBenefit.Requests;
using DTOs.ACAD.ACAD_CourseBenefit.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseBenefitController : ControllerBase
    {
        private readonly IACAD_CourseBenefitService _courseBenefitService;

        public ACAD_CourseBenefitController(IACAD_CourseBenefitService courseBenefitService)
        {
            _courseBenefitService = courseBenefitService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseBenefitResponse>>> GetAll()
        {
            try
            {
                var benefits = await _courseBenefitService.GetAllCourseBenefitsAsync();
                return Ok(benefits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseBenefitResponse>> GetById(Guid id)
        {
            try
            {
                var benefit = await _courseBenefitService.GetCourseBenefitByIdAsync(id);
                if (benefit == null)
                    return NotFound();

                return Ok(benefit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<CourseBenefitResponse>>> GetByCourseId(Guid courseId)
        {
            try
            {
                var benefits = await _courseBenefitService.GetBenefitsByCourseIdAsync(courseId);
                return Ok(benefits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateCourseBenefitRequest request)
        {
            try
            {
                var id = await _courseBenefitService.CreateCourseBenefitAsync(request);
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
        public async Task<ActionResult> Update(Guid id, UpdateCourseBenefitRequest request)
        {
            try
            {

                await _courseBenefitService.UpdateCourseBenefitAsync(id, request);
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
                await _courseBenefitService.DeleteCourseBenefitAsync(id);
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
