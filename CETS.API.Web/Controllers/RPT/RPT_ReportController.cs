using Application.Interfaces.RPT;
using DTOs.RPT.RPT_Report.Requests;
using DTOs.RPT.RPT_Report.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.RPT
{
	[Route("api/[controller]")]
	[ApiController]
	public class RPT_ReportController : ControllerBase
	{
		private readonly IRPT_ReportService _service;

		public RPT_ReportController(IRPT_ReportService service)
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

		[HttpGet("status/{statusId:guid}")]
		public async Task<IActionResult> GetByStatusIdAsync(Guid statusId)
		{
			var items = await _service.GetByStatusIdAsync(statusId);
			return Ok(items);
		}

		[HttpGet("submitter/{submitterId:guid}")]
		public async Task<IActionResult> GetBySubmitterAsync(Guid submitterId)
		{
			var items = await _service.GetBySubmitterAsync(submitterId);
			return Ok(items);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateReportRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateReportRequest request)
		{
			var updated = await _service.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteAsync(Guid id)
		{
			await _service.DeleteAsync(id);
			return NoContent();
		}

		[HttpPost("academic-request")]
		public async Task<IActionResult> SubmitAcademicRequestAsync([FromBody] SubmitAcademicReportRequest request)
		{
			try
			{
				// Validation is handled by data annotations and service layer
				var result = await _service.SubmitAcademicRequestAsync(request);
				return Created($"Created successfully with id: {result.Report.Id}", result);
			}
			catch (ArgumentException ex)
			{
				// A1: Missing Required Fields or A2: Invalid Date/Time
				return BadRequest(new { error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				// E2: System Error
				return StatusCode(500, new { error = ex.Message });
			}
			catch (Exception ex)
			{
				// E2: System Error - Unexpected error
				return StatusCode(500, new { error = "An unexpected error occurred while processing your request.", details = ex.Message });
			}
		}

		/// <summary>
		/// Get all academic requests submitted by a specific user
		/// </summary>
		[HttpGet("academic-request/submitter/{submitterId:guid}")]
		public async Task<IActionResult> GetAcademicRequestsBySubmitterAsync(Guid submitterId)
		{
			var requests = await _service.GetAcademicRequestsBySubmitterAsync(submitterId);
			return Ok(requests);
		}

		/// <summary>
		/// Get detailed information about a specific academic request
		/// </summary>
		[HttpGet("academic-request/{requestId:guid}")]
		public async Task<IActionResult> GetAcademicRequestDetailsAsync(Guid requestId)
		{
			var request = await _service.GetAcademicRequestDetailsAsync(requestId);
			if (request == null)
				return NotFound(new { error = "Academic request not found" });
			return Ok(request);
		}

		/// <summary>
		/// Get all academic requests for a specific course
		/// </summary>
		[HttpGet("academic-request/course/{courseId:guid}")]
		public async Task<IActionResult> GetAcademicRequestsByCourseAsync(Guid courseId)
		{
			var requests = await _service.GetAcademicRequestsByCourseAsync(courseId);
			return Ok(requests);
		}

		/// <summary>
		/// Get all academic requests for a specific class
		/// </summary>
		[HttpGet("academic-request/class/{classId:guid}")]
		public async Task<IActionResult> GetAcademicRequestsByClassAsync(Guid classId)
		{
			var requests = await _service.GetAcademicRequestsByClassAsync(classId);
			return Ok(requests);
		}

		/// <summary>
		/// Get all pending academic requests (for staff review)
		/// POST-2: Academic staff can review pending requests
		/// </summary>
		[HttpGet("academic-request/pending")]
		public async Task<IActionResult> GetPendingAcademicRequestsAsync()
		{
			var requests = await _service.GetPendingAcademicRequestsAsync();
			return Ok(requests);
		}


	[HttpPut("academic-request/{requestId:guid}/process")]
	
	public async Task<IActionResult> ProcessAcademicRequestAsync(
		Guid requestId,
		[FromBody] ProcessAcademicReportRequest request)
	{
		try
		{
			await _service.ProcessAcademicRequestAsync(requestId, request);
			return NoContent();
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { error = "An unexpected error occurred while processing the request.", details = ex.Message });
		}
	}

	/// <summary>
	/// Get download URL for report attachment
	/// </summary>
	[HttpGet("download/{id:guid}")]
	public async Task<ActionResult<string>> GetDownloadUrl(Guid id)
	{
		try
		{
			var report = await _service.GetByIdAsync(id);
			if (report == null)
				return NotFound("Report not found");

			var downloadUrl = await _service.GetDownloadUrlAsync(id);
			
			return Ok(new { 
				downloadUrl,
				reportInfo = new {
					id = report.Id,
					title = report.Title,
					createdAt = report.CreatedAt
				}
			});
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(ex.Message);
		}
		catch (Exception ex)
		{
			return StatusCode(500, $"Internal server error: {ex.Message}");
		}
	}

		
	}
}



