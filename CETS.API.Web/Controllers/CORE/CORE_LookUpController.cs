using Application.Interfaces.CORE;
using DTOs.CORE.LookUp.Requests;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CETS.API.Web.Controllers.CORE
{
    [Route("api/[controller]")]
    [ApiController]
    public class CORE_LookUpController : ControllerBase
    {
        private readonly ICORE_LookUpService _service;

        public CORE_LookUpController(ICORE_LookUpService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
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

        [HttpGet("type/{typeId:guid}")]
        public async Task<IActionResult> GetByTypeIdAsync(Guid typeId)
        {
            var items = await _service.GetByTypeIdAsync(typeId);
            return Ok(items);
        }

        [HttpGet("type/code/{typeCode}")]
        public async Task<IActionResult> GetByTypeCodeAsync(string typeCode)
        {
            var items = await _service.GetByTypeCodeAsync(typeCode);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateLookUpRequest dto)
        {
            var created = await _service.CreateAsync(dto);
            return Created("New lookup created", created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateLookUpRequest dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(updated);
        }

        [HttpPatch("deactivate/{id:guid}")]
        public async Task<IActionResult> DeactivateAsync(Guid id)
        {
            var deactivated = await _service.DeactivateAsync(id);
            return Ok(deactivated);
        }

        [HttpPatch("activate/{id:guid}")]
        public async Task<IActionResult> ActivateAsync(Guid id)
        {
            var activated = await _service.ActivateAsync(id);
            return Ok(activated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
