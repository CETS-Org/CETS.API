using Application.Interfaces.ACAD;
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

    }
}
