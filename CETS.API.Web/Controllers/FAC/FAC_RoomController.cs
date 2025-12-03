using Application.Interfaces.FAC;
using DTOs.FAC.FAC_Room.Requests;
using DTOs.FAC.FAC_Room.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.FAC
{
	[Route("api/[controller]")]
	[ApiController]
	public class FAC_RoomController : ControllerBase
	{
		private readonly IFAC_RoomService _service;

		public FAC_RoomController(IFAC_RoomService service)
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

		[HttpGet("type/{roomTypeId:guid}")]
		public async Task<IActionResult> GetByTypeAsync(Guid roomTypeId)
		{
			var items = await _service.GetByTypeAsync(roomTypeId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateRoomRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateRoomRequest request)
		{
			var updated = await _service.UpdateAsync(id, request);
			return Ok(updated);
		}

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PatchAsync(Guid id, [FromBody] UpdateRoomRequest request)
        {
            var updated = await _service.PatchAsync(id, request);
            return Ok(updated);
        }


        [HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteAsync(Guid id)
		{
			await _service.DeleteAsync(id);
			return NoContent();
		}

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateRoomStatus(Guid id, [FromBody] UpdateRoomStatusRequest request)
        {
            var updated = await _service.UpdateRoomStatusAsync(id, request.RoomStatusId);
            return Ok(updated);
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetRoomStatusesAsync()
        {
            var statuses = await _service.GetRoomStatusesAsync();
            return Ok(statuses);
        }

        [HttpGet("{id:guid}/check-availability")]
        public async Task<IActionResult> CheckAvailability(Guid id, [FromQuery] DateTime date, [FromQuery] int slotNumber)
        {
            var result = await _service.CheckSlotAvailabilityAsync(id, date, slotNumber);
            return Ok(result);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRoomsForSlot([FromQuery] DateTime date, [FromQuery] Guid slotId)
        {
            var rooms = await _service.GetAvailableRoomsForSlotAsync(date, slotId);
            return Ok(rooms);
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetWeeklySchedule([FromQuery] DateTime weekStart, [FromQuery] DateTime weekEnd)
        {
            var schedule = await _service.GetWeeklyScheduleAsync(weekStart, weekEnd);
            return Ok(schedule);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            var stats = await _service.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetRoomTypesAsync()
        {
            var types = await _service.GetRoomTypesAsync();
            return Ok(types);
        }

        [HttpGet("{roomId:guid}/slot-info")]
        public async Task<IActionResult> GetSlotInfo(Guid roomId, [FromQuery] DateOnly date, [FromQuery] int slotNumber)
        {
            var result = await _service.GetSlotInfoAsync(roomId, date, slotNumber);
            return Ok(result);
        }

        [HttpPost("book-slot")]
        public async Task<IActionResult> BookSlot([FromBody] BookRoomSlotRequest request)
        {
            var meetingId = await _service.BookSlotAsync(request);
            return Ok(new { MeetingId = meetingId, Message = "Booked successfully" });
        }

        [HttpDelete("bookings/{meetingId:guid}")]
        public async Task<IActionResult> CancelBooking(Guid meetingId)
        {
            await _service.CancelSlotBookingAsync(meetingId);
            return Ok(new { Message = "Booking cancelled" });
        }

        [HttpPost("available-rooms")]
        public async Task<ActionResult<IEnumerable<RoomOptionDto>>> GetAvailableRooms(
          [FromBody] GetAvailableRoomsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var rooms = await _service  .GetAvailableRoomsAsync(request);
                return Ok(rooms);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                Console.WriteLine(ex);
                return StatusCode(500, new { message = "Error while fetching available rooms." });
            }
        }

    }
}



