using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_WeeklyFeedback.Request;
using DTOs.ACAD.ACAD_WeeklyFeedback.Response;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [ApiController]
    [Route("api/weekly-feedback")]
    public class ACAD_WeeklyFeedbackController : Controller
    {
        private readonly IWeeklyFeedbackService _svc;     

        public ACAD_WeeklyFeedbackController(IWeeklyFeedbackService svc)
        {
            _svc = svc;         
        }

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertWeeklyFeedbackRequestDto req, CancellationToken ct)
        {
            var teacherId = req.TeacherId ?? throw new ArgumentNullException(nameof(req.TeacherId));
            await _svc.UpsertAsync(teacherId, req, ct);
            return Ok(new { ok = true });
        }


        [HttpGet("class/{classId:guid}/week/{weekNumber:int}")]
        public async Task<ActionResult<IReadOnlyList<WeeklyFeedbackViewDto>>> GetByClassWeek(Guid classId, int weekNumber, CancellationToken ct)
        {
            var data = await _svc.GetByClassWeekAsync(classId, weekNumber, ct);
            return Ok(data);
        }

        [HttpGet("student/{studentId:guid}")]
        public async Task<ActionResult<IReadOnlyList<WeeklyFeedbackViewDto>>> GetByStudent(Guid studentId, [FromQuery] Guid? classId, CancellationToken ct)
        {
            var data = await _svc.GetByStudentAsync(studentId, classId, ct);
            return Ok(data);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WeeklyFeedbackViewDto>> GetDetail(Guid id, CancellationToken ct)
        {
            var data = await _svc.GetDetailAsync(id, ct);
            if (data is null) return NotFound();
            return Ok(data);
        }
    }
}
