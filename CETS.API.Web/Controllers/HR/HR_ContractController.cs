using Application.Interfaces.HR;
using DTOs.HR.HR_Contract.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.HR
{
	[Route("api/[controller]")]
	[ApiController]
	public class HR_ContractController : ControllerBase
	{
		private readonly IHR_ContractService _service;

		public HR_ContractController(IHR_ContractService service)
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

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateContractRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateContractRequest request)
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
	}
}



