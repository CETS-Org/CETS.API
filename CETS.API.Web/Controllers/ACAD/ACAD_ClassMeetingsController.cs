using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_ClassMeetings.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_ClassMeetingsController : ControllerBase
    {
        private readonly IACAD_ClassMeetingsService _classMeetingsService;

        public ACAD_ClassMeetingsController(IACAD_ClassMeetingsService classMeetingsService)
        {
            _classMeetingsService = classMeetingsService;
        }

        [EnableQuery]
        [HttpGet("Schedule/{studentId}")]
        public async Task<IActionResult> GetByStudentAsync(Guid studentId, CancellationToken ct)
        {
            var schedules = await _classMeetingsService.WeeklyScheduleGetByStudentAsync(studentId, ct);
            return Ok(schedules);
        }

        [EnableQuery]
        [HttpGet("Schedule/Teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherAsync(Guid teacherId, CancellationToken ct)
        {
            var schedules = await _classMeetingsService.WeeklyScheduleGetByTeacherAsync(teacherId, ct);
            return Ok(schedules);
        }

        [EnableQuery]
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetAllClassMeetingByClassIdAs(Guid classId)
        {
            var listSession = await _classMeetingsService.GetAllClassMeetingByClassId(classId);
            return Ok(listSession);
        }

        [HttpGet("{classMeetingId}/covered-topic")]
        public async Task<IActionResult> GetCoveredTopic(Guid classMeetingId)
        {
            var result = await _classMeetingsService.GetCoveredTopicAsync(classMeetingId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateClassMeetingRequest request)
        {
            
            try
            {
                var id = await _classMeetingsService.CreateClassMeetingAsync(request);                        

                return Ok(new { Id = id, Message = "ClassMeeting created successfully with all related details" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMeeting([FromRoute] Guid id, [FromBody] UpdateClassMeetingRequest request)
        {
            if (request.Id == Guid.Empty)
            {
                request.Id = id;
            }
            else if (request.Id != id)
            {
                return BadRequest(new { message = "Route id and body id do not match." });
            }

            try
            {
                await _classMeetingsService.UpdateClassMeetingAsync(request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
