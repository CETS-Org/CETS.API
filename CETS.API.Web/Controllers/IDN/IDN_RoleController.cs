using Application.Interfaces.IDN;
<<<<<<< HEAD
using DTOs.IDN.IDN_Role.Requests;
=======
using DTOs.IDN_Role.Requests;
>>>>>>> 9dc6edc (feat: add Role and AccountRole apis)
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_RoleController : ControllerBase
    {
        private readonly IIDN_RoleService _roleService;

        public IDN_RoleController(IIDN_RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetStaffRoles()
        {
            var roles = await _roleService.SearchRolesByKeywordAsync("Staff");
            return Ok(roles.Select(r => new
            {
                r.Id,
                r.RoleName
            }));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateRoleRequest request)
        {
            var created = await _roleService.CreateAsync(request);
            return Created("New role added", created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateRoleRequest request)
        {
            var updated = await _roleService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }
    }
}


