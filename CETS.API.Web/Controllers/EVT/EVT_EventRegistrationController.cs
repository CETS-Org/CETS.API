using Application.Interfaces.EVT;
using DTOs.EVT.EVT_EventRegistration.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.EVT
{
	[Route("api/[controller]")]
	[ApiController]
	public class EVT_EventRegistrationController : ControllerBase
	{
		private readonly IEVT_EventRegistrationService _service;

		public EVT_EventRegistrationController(IEVT_EventRegistrationService service)
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

		[HttpGet("account/{accountId:guid}")]
		public async Task<IActionResult> GetByAccountIdAsync(Guid accountId)
		{
			var items = await _service.GetByAccountIdAsync(accountId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateEventRegistrationRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateEventRegistrationRequest request)
		{
			var updated = await _service.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpPost("{id:guid}/checkin")]
		public async Task<IActionResult> CheckInAsync(Guid id)
		{
			var updated = await _service.CheckInAsync(id, DateTime.UtcNow);
			return Ok(updated);
		}

		[HttpPost("{id:guid}/checkout")]
		public async Task<IActionResult> CheckOutAsync(Guid id)
		{
			var updated = await _service.CheckOutAsync(id, DateTime.UtcNow);
			return Ok(updated);
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> SoftDeleteAsync(Guid id)
		{
			var deleted = await _service.SoftDeleteAsync(id);
			return Ok(deleted);
		}

		[HttpPatch("restore/{id:guid}")]
		public async Task<IActionResult> RestoreRegistrationAsync(Guid id)
		{
			var restored = await _service.RestoreRegistrationAsync(id);
			return Ok(restored);
        }
    }
}



