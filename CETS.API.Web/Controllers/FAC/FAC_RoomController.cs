using Application.Interfaces.FAC;
using DTOs.FAC.FAC_Room.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.FAC
{
	[Route("api/[controller]")]
	[ApiController]
	public class FAC_RoomController : ControllerBase
	{
		private readonly IFAC_RoomService _service;

		public FAC_RoomController(IFAC_RoomService service)
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

		[HttpGet("type/{roomTypeId:guid}")]
		public async Task<IActionResult> GetByTypeAsync(Guid roomTypeId)
		{
			var items = await _service.GetByTypeAsync(roomTypeId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateRoomRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateRoomRequest request)
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



