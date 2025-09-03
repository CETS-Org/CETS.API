using Application.Interfaces.EVT;
using DTOs.EVT.EVT_Event.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.EVT
{
	[Route("api/[controller]")]
	[ApiController]
	public class EVT_EventController : ControllerBase
	{
		private readonly IEVT_EventService _service;

		public EVT_EventController(IEVT_EventService service)
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

		[HttpGet("type/{eventTypeId:guid}")]
		public async Task<IActionResult> GetByTypeIdAsync(Guid eventTypeId)
		{
			var items = await _service.GetByTypeIdAsync(eventTypeId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateEventRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateEventRequest request)
		{
			var updated = await _service.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> SoftDeleteAsync(Guid id)
		{
			var deleted = await _service.SoftDeleteAsync(id);
			return Ok(deleted);
		}

		[HttpPatch("restore/{id:guid}")]
		public async Task<IActionResult> RestoreAsync(Guid id)
		{
			var restored = await _service.RestoreEventAsync(id);
			return Ok(restored);
        }
    }
}



