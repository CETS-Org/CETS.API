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
			try
			{
				var items = await _service.GetAllAsync();
				return Ok(items);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetByIdAsync(Guid id)
		{
			try
			{
				var item = await _service.GetByIdAsync(id);
				if (item == null) return NotFound($"Teacher availability with id '{id}' not found.");
				return Ok(item);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpGet("teacher/{teacherId:guid}")]
		public async Task<IActionResult> GetByTeacherIdAsync(Guid teacherId)
		{
			try
			{
				var items = await _service.GetByTeacherIdAsync(teacherId);
				return Ok(items);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpGet("teacher/{teacherId:guid}/date/{teachDate}")]
		public async Task<IActionResult> GetByTeacherAndDateAsync(Guid teacherId, DateOnly teachDate)
		{
			try
			{
				var items = await _service.GetByTeacherAndDateAsync(teacherId, teachDate.DayOfWeek);
				return Ok(items);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateTeacherAvailabilityRequest request)
		{
			try
			{
				var created = await _service.CreateAsync(request);
				return Created("Created", created);
			}
			catch (Exception ex)
			{
				return BadRequest($"{ex.Message}");
			}
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTeacherAvailabilityRequest request)
		{
			try
			{
				var updated = await _service.UpdateAsync(id, request);
				return Ok(updated);
			}
			catch (KeyNotFoundException)
			{
				return NotFound($"Teacher availability with id '{id}' not found.");
			}
			catch (Exception ex)
			{
				return BadRequest($"Error updating teacher availability: {ex.Message}");
			}
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteAsync(Guid id)
		{
			try
			{
				await _service.DeleteAsync(id);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound($"Teacher availability with id '{id}' not found.");
			}
			catch (Exception ex)
			{
				return BadRequest($"Error deleting teacher availability: {ex.Message}");
			}
		}
	}
}



