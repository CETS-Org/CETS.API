using Application.Interfaces.IDN;
using DTOs.IDN.IDN_AccountRole.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDN_AccountRoleController : ControllerBase
    {
        private readonly IIDN_AccountRoleService _accountRoleService;

        public IDN_AccountRoleController(IIDN_AccountRoleService accountRoleService)
        {
            _accountRoleService = accountRoleService;
        }

        [HttpGet("roles/{accountId:guid}")]
        public async Task<IActionResult> GetRolesByAccountId(Guid accountId)
        {
            var roles = await _accountRoleService.GetRolesByAccountIdAsync(accountId);
            return Ok(roles);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var assigned = await _accountRoleService.AssignRoleAsync(request);
            return Ok(assigned);
        }

        [HttpDelete("unassign")]
        public async Task<IActionResult> UnassignRole([FromBody] UnassignRoleRequest request)
        {
            var success = await _accountRoleService.UnassignRoleAsync(request);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


