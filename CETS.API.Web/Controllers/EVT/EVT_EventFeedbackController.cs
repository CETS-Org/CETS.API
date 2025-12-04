using Application.Interfaces.EVT;
using DTOs.EVT.EVT_EventFeedback.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.EVT
{
	[Route("api/[controller]")]
	[ApiController]
	public class EVT_EventFeedbackController : ControllerBase
	{
		private readonly IEVT_EventFeedbackService _service;

		public EVT_EventFeedbackController(IEVT_EventFeedbackService service)
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

		[HttpGet("event/{eventId:guid}")]
		public async Task<IActionResult> GetByEventIdAsync(Guid eventId)
		{
			var items = await _service.GetByEventIdAsync(eventId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateEventFeedbackRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateEventFeedbackRequest request)
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



