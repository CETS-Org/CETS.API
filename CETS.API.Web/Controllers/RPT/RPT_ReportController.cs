using Application.Interfaces.RPT;
using DTOs.RPT.RPT_Report.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.RPT
{
	[Route("api/[controller]")]
	[ApiController]
	public class RPT_ReportController : ControllerBase
	{
		private readonly IRPT_ReportService _service;

		public RPT_ReportController(IRPT_ReportService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllAsync()
		{
			var items = await _service.GetAllAsync();
			return Ok(items);
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetByIdAsync(Guid id)
		{
			var item = await _service.GetByIdAsync(id);
			if (item == null) return NotFound();
			return Ok(item);
		}

		[HttpGet("status/{statusId:guid}")]
		public async Task<IActionResult> GetByStatusIdAsync(Guid statusId)
		{
			var items = await _service.GetByStatusIdAsync(statusId);
			return Ok(items);
		}

		[HttpGet("submitter/{submitterId:guid}")]
		public async Task<IActionResult> GetBySubmitterAsync(Guid submitterId)
		{
			var items = await _service.GetBySubmitterAsync(submitterId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateReportRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateReportRequest request)
		{
			var updated = await _service.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteAsync(Guid id)
		{
			await _service.DeleteAsync(id);
			return NoContent();
		}
	}
}



