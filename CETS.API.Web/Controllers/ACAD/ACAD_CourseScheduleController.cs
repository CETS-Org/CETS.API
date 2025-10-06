using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_CourseSchedule.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseScheduleController : ControllerBase
    {
        private readonly IACAD_CourseScheduleService _courseScheduleService;

        public ACAD_CourseScheduleController(IACAD_CourseScheduleService courseScheduleService)
        {
            _courseScheduleService = courseScheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var schedules = await _courseScheduleService.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var schedule = await _courseScheduleService.GetDetailByIdAsync(id);
            if (schedule == null) return NotFound();
            return Ok(schedule);
        }

        [HttpGet("course/{courseId:guid}")]
        public async Task<IActionResult> GetSchedulesByCourseIdAsync(Guid courseId)
        {
            var schedules = await _courseScheduleService.GetSchedulesByCourseIdAsync(courseId);
            return Ok(schedules);
        }

        [HttpGet("day/{dayOfWeek}")]
        public async Task<IActionResult> GetSchedulesByDayOfWeekAsync(DayOfWeek dayOfWeek)
        {
            var schedules = await _courseScheduleService.GetSchedulesByDayOfWeekAsync(dayOfWeek);
            return Ok(schedules);
        }

        [HttpGet("timeslot/{timeSlotId:guid}")]
        public async Task<IActionResult> GetSchedulesByTimeSlotIdAsync(Guid timeSlotId)
        {
            var schedules = await _courseScheduleService.GetSchedulesByTimeSlotIdAsync(timeSlotId);
            return Ok(schedules);
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckTimeSlotAvailabilityAsync(
            [FromQuery] Guid courseId, 
            [FromQuery] Guid timeSlotId, 
            [FromQuery] DayOfWeek dayOfWeek)
        {
            var isAvailable = await _courseScheduleService.IsTimeSlotAvailableAsync(courseId, timeSlotId, dayOfWeek);
            return Ok(new { IsAvailable = isAvailable });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCourseScheduleRequest request)
        {
            try
            {
                var created = await _courseScheduleService.CreateAsync(request);
                return Created("Created", created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateCourseScheduleRequest request)
        {
            try
            {
                var updated = await _courseScheduleService.UpdateAsync(id, request);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                await _courseScheduleService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}
