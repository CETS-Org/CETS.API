using Application.Interfaces.COM;
using DTOs.COM_FeedbackRecord.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.COM
{
	[Route("api/[controller]")]
	[ApiController]
	public class COM_FeedbackRecordController : ControllerBase
	{
		private readonly ICOM_FeedbackRecordService _service;

		public COM_FeedbackRecordController(ICOM_FeedbackRecordService service)
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

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateFeedbackRecordRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateFeedbackRecordRequest request)
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
	}
}



