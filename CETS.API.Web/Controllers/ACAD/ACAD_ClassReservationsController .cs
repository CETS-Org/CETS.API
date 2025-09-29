using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_ClassReservation.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CETS.API.Web.Controllers.ACAD
{
    [ApiController]
    [Route("api/class-reservations")]
    public class ACAD_ClassReservationsController : ControllerBase
    {
        private readonly IACAD_ClassReservationService _reservationService;

        public ACAD_ClassReservationsController(IACAD_ClassReservationService reservationService)
        {
            _reservationService = reservationService;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservationList = _reservationService.GetAllReservationAsync();
            
            return Ok(reservationList);
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReservationById(Guid id)
        {
            var reservation = await _reservationService.GetReservationById(id);
            if (reservation == null)
            {
                return NotFound(); 
            }
            return Ok(reservation);
        }

        
        [HttpGet("student/{studentId:guid}")]
        public async Task<IActionResult> GetReservationByStudentId(Guid studentId)
        {
            var reservation = await _reservationService.GetReservationByStudentId(studentId);
            if (reservation == null)
            {
                return NotFound();
            }
            return Ok(reservation);
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateClassReservationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newReservationId = await _reservationService.CreateReservationAsync(request);
           
            return CreatedAtAction(nameof(GetReservationById), new { id = newReservationId }, new { id = newReservationId });
        }


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateReservation([FromRoute] Guid id, [FromBody] UpdateClassReservationRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id != request.Id)
                return BadRequest("Route id and body id do not match.");

            try
            {
                await _reservationService.UpdateReservationAsync(request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
