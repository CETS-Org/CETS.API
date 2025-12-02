using Application.Interfaces.RPT;
using Application.Interfaces.CORE;
using Domain.Constants;
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
		private readonly ICORE_LookUpService _lookUpService;

		public RPT_ReportController(IRPT_ReportService service, ICORE_LookUpService lookUpService)
		{
			_service = service;
			_lookUpService = lookUpService;
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

		/// <summary>
		/// Get all system complaints (filtered by ReportType)
		/// </summary>
		[HttpGet("system-complaints")]
		public async Task<IActionResult> GetSystemComplaintsAsync()
		{
			var items = await _service.GetSystemComplaintsAsync();
			return Ok(items);
		}

		/// <summary>
		/// Get system complaints by status (for admin - gets all reports with that status)
		/// </summary>
		[HttpGet("system-complaints/status/{statusId:guid}")]
		public async Task<IActionResult> GetSystemComplaintsByStatusAsync(Guid statusId)
		{
			// For admin view, we get all reports filtered by status (no reportType filter)
			var allItems = await _service.GetAllAsync();
			var filteredItems = allItems.Where(r => r.ReportStatusID == statusId).ToList();
			return Ok(filteredItems);
		}

		/// <summary>
		/// Get system complaints by report type
		/// </summary>
		[HttpGet("system-complaints/type/{reportTypeId:guid}")]
		public async Task<IActionResult> GetSystemComplaintsByReportTypeAsync(Guid reportTypeId)
		{
			var items = await _service.GetSystemComplaintsByReportTypeAsync(reportTypeId);
			return Ok(items);
		}

		/// <summary>
		/// Get all report types for selection/dropdown
		/// </summary>
		[HttpGet("report-types")]
		public async Task<IActionResult> GetReportTypesAsync()
		{
			var reportTypes = await _lookUpService.GetByTypeCodeAsync(LookUpTypes.ReportType);
			var result = reportTypes.Select(x => new
			{
				id = x.LookUpId,
				code = x.Code,
				name = x.Name,
				isActive = x.IsActive
			}).ToList();
			return Ok(result);
		}

		/// <summary>
		/// Get all report statuses for selection/dropdown
		/// </summary>
		[HttpGet("report-statuses")]
		public async Task<IActionResult> GetReportStatusesAsync()
		{
			var reportStatuses = await _lookUpService.GetByTypeCodeAsync(LookUpTypes.ReportStatus);
			var result = reportStatuses.Select(x => new
			{
				id = x.LookUpId,
				code = x.Code,
				name = x.Name,
				isActive = x.IsActive
			}).ToList();
			return Ok(result);
		}

		/// <summary>
		/// Get presigned upload URL for report image
		/// </summary>
		[HttpPost("image-upload-url")]
		public async Task<ActionResult<ReportUploadResponse>> GetImageUploadUrl([FromBody] GetReportUploadUrlRequest request)
		{
			var result = await _service.GetReportImageUploadUrlAsync(request.FileName, request.ContentType);
			return Ok(result);
		}

		/// <summary>
		/// Get download URL for report image
		/// </summary>
		[HttpGet("image/{id:guid}")]
		public async Task<ActionResult<string>> GetReportImageUrl(Guid id)
		{
			try
			{
				var report = await _service.GetByIdAsync(id);
				if (report == null)
					return NotFound("Report not found");

				var downloadUrl = await _service.GetReportImageDownloadUrlAsync(id);
				
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



