using Application.Interfaces.COM;
using DTOs.COM.COM_Feedback.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.COM
{
	[Route("api/[controller]")]
	[ApiController]
	public class COM_FeedbackController : ControllerBase
	{
		private readonly ICOM_FeedbackService _service;

		public COM_FeedbackController(ICOM_FeedbackService service)
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
		public async Task<IActionResult> CreateAsync([FromBody] CreateFeedbackRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateFeedbackRequest request)
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
			var restored = await _service.RestoreAsync(id);
			return Ok(restored);
        }

		[HttpPost("combined")]
		public async Task<IActionResult> CreateCombinedFeedbackAsync([FromBody] CreateCombinedFeedbackRequest request)
		{
			var result = await _service.CreateCombinedFeedbackAsync(request);
			
			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Created("Created", result);
		}

		[HttpGet("course/{courseId:guid}")]
		public async Task<IActionResult> GetFeedbacksByCourseIdAsync(Guid courseId)
		{
			var feedbacks = await _service.GetFeedbacksByCourseIdAsync(courseId);
			return Ok(feedbacks);
		}
    }
}



