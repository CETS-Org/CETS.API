using Application.Interfaces.COM;
using DTOs.COM.COM_Chat.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.COM
{
    [Route("api/[controller]")]
    [ApiController]
    public class COM_ChatController : ControllerBase
    {
        private readonly ICOM_ChatService _chatService;
        private readonly ILogger<COM_ChatController> _logger;

        public COM_ChatController(ICOM_ChatService chatService, ILogger<COM_ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("room")]
        public async Task<IActionResult> CreateRoomAsync([FromBody] CreateChatRoomRequest request)
        {
            try
            {
                var result = await _chatService.CreateRoomAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/rooms")]
        public async Task<IActionResult> GetUserRoomsAsync(string userId)
        {
            try
            {
                var result = await _chatService.GetUserRoomsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user rooms");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoomByIdAsync(string roomId)
        {
            try
            {
                var result = await _chatService.GetRoomByIdAsync(roomId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching room details");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
        {
            try
            {
                var result = await _chatService.SendMessageAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("room/{roomId}/messages")]
        public async Task<IActionResult> GetMessagesAsync(string roomId, [FromQuery] int limit = 50, [FromQuery] int skip = 0)
        {
            try
            {
                var result = await _chatService.GetMessagesAsync(roomId, limit, skip);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
