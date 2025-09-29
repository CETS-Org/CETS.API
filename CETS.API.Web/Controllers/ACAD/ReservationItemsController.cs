using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_ClassReservation.Requests;
using DTOs.ACAD.ACAD_ClassReservation.Responses;
using DTOs.ACAD.ACAD_ReservationItem.Requests;
using DTOs.ACAD.ACAD_ReservationItem.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CETS.API.Web.Controllers.ACAD
{
    [ApiController]
    [Route("api/reservation-items")]
    public class ReservationItemsController : ControllerBase
    {
        private readonly IACAD_ReservationItemService _reservationItemService;

        
        public ReservationItemsController(IACAD_ReservationItemService reservationItemService)
        {
            _reservationItemService = reservationItemService;
        }
       
        [HttpGet]
        public async Task<IActionResult> GetAllReservationItems()
        {
            
            var reservationItems = await _reservationItemService.GetAllReservationItemAsync().ToListAsync();
            return Ok(reservationItems);
        }

      
        [HttpGet("{id:guid}")]      
        public async Task<IActionResult> GetReservationItemById(Guid id)
        {
            var reservationItem = await _reservationItemService.GetReservationItemByIdAsync(id);
            if (reservationItem == null)
            {
                return NotFound($"Không tìm thấy Reservation Item với ID: {id}");
            }
            return Ok(reservationItem);
        }

      
        [HttpGet("by-reservation/{reservationId:guid}")]    
        public async Task<IActionResult> GetReservationItemByReservationId(Guid reservationId)
        {
            var reservationItem =  _reservationItemService.GetReservationItemByReservationId(reservationId);
            if (reservationItem == null)
            {
                return NotFound($"Không tìm thấy Reservation Item cho Reservation ID: {reservationId}");
            }
            return Ok(reservationItem);
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateReservationItem([FromBody] CreateReservationItemRequests request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newReservationItem = await _reservationItemService.CreateReservationItemAsync(request);
            
            return CreatedAtAction(nameof(GetReservationItemById), new { id = newReservationItem.Id }, newReservationItem);
        }

       
        [HttpPost("list")]      
        public async Task<IActionResult> CreateListReservationItem([FromBody] List<CreateReservationItemRequests> request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newItems = await _reservationItemService.CreateListReservationItemAsync(request);
            return Ok(newItems);
        }

        [HttpPut("{id:guid}")]  
        public async Task<IActionResult> UpdateReservationItem(Guid id, [FromBody] UpdateReservationItemRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID trong URL và trong body không khớp.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedItem = await _reservationItemService.UpdateReservationItemAsync(request);
                return Ok(updatedItem);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

      
        [HttpDelete("{id:guid}")]   
        public async Task<IActionResult> DeleteReservationItem(Guid id)
        {
            try
            {
                var success = await _reservationItemService.DeleteReservationItemAsync(id);
                if (success)
                {
                    return NoContent(); 
                }
                
                return BadRequest("Không thể xóa reservation item.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
