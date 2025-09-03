using Application.Interfaces.HR;
using DTOs.HR.HR_TeacherAvailability.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.HR
{
	[Route("api/[controller]")]
	[ApiController]
	public class HR_TeacherAvailabilityController : ControllerBase
	{
		private readonly IHR_TeacherAvailabilityService _service;

		public HR_TeacherAvailabilityController(IHR_TeacherAvailabilityService service)
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

		[HttpGet("teacher/{teacherId:guid}")]
		public async Task<IActionResult> GetByTeacherIdAsync(Guid teacherId)
		{
			var items = await _service.GetByTeacherIdAsync(teacherId);
			return Ok(items);
		}

		[HttpGet("teacher/{teacherId:guid}/date/{teachDate}")]
		public async Task<IActionResult> GetByTeacherAndDateAsync(Guid teacherId, DateTime teachDate)
		{
			var items = await _service.GetByTeacherAndDateAsync(teacherId, teachDate);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateTeacherAvailabilityRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTeacherAvailabilityRequest request)
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



