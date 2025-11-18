using Application.Interfaces.COM;
using DTOs.COM.COM_Notification.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.COM;

[Route("api/[controller]")]
[ApiController]
public class COM_NotificationController : ControllerBase
{
    private readonly ICOM_NotificationService _notificationService;

    public COM_NotificationController(ICOM_NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _notificationService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserAsync(string userId)
    {
        var result = await _notificationService.GetByUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var result = await _notificationService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateNotificationRequest request)
    {
        var created = await _notificationService.CreateAsync(request);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateNotificationRequest request)
    {
        var updated = await _notificationService.UpdateAsync(id, request);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _notificationService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsReadAsync(string id)
    {
        var updated = await _notificationService.MarkAsReadAsync(id);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpPost("user/{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsReadAsync(string userId)
    {
        var updatedCount = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { updatedCount });
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateManyAsync([FromBody] IEnumerable<CreateNotificationRequest> requests)
    {
        var created = await _notificationService.CreateManyAsync(requests);
        return Ok(created);
    }
}
