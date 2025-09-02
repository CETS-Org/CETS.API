using Application.Interfaces.CORE;
using DTOs.CORE.LookUpType.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.CORE
{
    [Route("api/[controller]")]
    [ApiController]
    public class CORE_LookUpTypeController : ControllerBase
    {
        private readonly ICORE_LookUpTypeService _service;

        public CORE_LookUpTypeController(ICORE_LookUpTypeService service)
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
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var item = await _service.GetByCodeAsync(code);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            var item = await _service.GetByNameAsync(name);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateLookUpTypeRequest dto)
        {
            var created = await _service.CreateAsync(dto);
            return Created("New lookup type created", created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateLookUpTypeRequest dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
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
