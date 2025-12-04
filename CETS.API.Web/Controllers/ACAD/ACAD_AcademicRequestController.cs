using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_AcademicRequest.Requests;
using DTOs.ACAD.ACAD_AcademicRequest.Responses;
using DTOs.ACAD.ACAD_AcademicRequestHistory.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_AcademicRequestController : ControllerBase
    {
        private readonly IACAD_AcademicRequestService _academicRequestService;
        private readonly IACAD_SuspensionValidationService? _suspensionValidationService;
        private readonly IACAD_DropoutValidationService? _dropoutValidationService;

        public ACAD_AcademicRequestController(
            IACAD_AcademicRequestService academicRequestService,
            IACAD_SuspensionValidationService? suspensionValidationService = null,
            IACAD_DropoutValidationService? dropoutValidationService = null)
        {
            _academicRequestService = academicRequestService;
            _suspensionValidationService = suspensionValidationService;
            _dropoutValidationService = dropoutValidationService;
        }

        // POST api/academicrequest - Submit a new academic request
        [HttpPost]
        public async Task<ActionResult<AcademicRequestResponse>> SubmitRequest([FromBody] CreateAcademicRequest requestDto)
        {
            var result = await _academicRequestService.SubmitRequestAsync(requestDto);
            return Ok(result);
        }

        // GET api/academicrequest/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetByStudent(Guid studentId)
        {
            var result = await _academicRequestService.GetRequestsByStudentAsync(studentId);
            return Ok(result);
        }

        // GET api/academicrequest - Get all requests (for staff)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetAll()
        {
            var result = await _academicRequestService.GetAllRequestsAsync();
            return Ok(result);
        }

        // GET api/academicrequest/status/{statusId} - Get requests by status (for staff)
        [HttpGet("status/{statusId}")]
        public async Task<ActionResult<IEnumerable<AcademicRequestResponse>>> GetByStatus(Guid statusId)
        {
            var result = await _academicRequestService.GetRequestsByStatusAsync(statusId);
            return Ok(result);
        }

        // GET api/academicrequest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AcademicRequestResponse>> GetDetails(Guid id)
        {
            var result = await _academicRequestService.GetDetailsAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET api/academicrequest/{id}/history - Get request history (for staff)
        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<AcademicRequestHistoryResponse>>> GetHistory(Guid id)
        {
            var history = await _academicRequestService.GetRequestHistoryAsync(id);
            return Ok(history);
        }

        // PUT api/academicrequest/process - Process (approve/reject) a request (for staff)
        [HttpPut("process")]
        public async Task<IActionResult> ProcessRequest([FromBody] ProcessAcademicRequest requestDto)
        {
            await _academicRequestService.ProcessRequestAsync(requestDto);
            return Ok(new { message = "Request processed successfully" });
        }

        // POST api/academicrequest/upload-url - Get presigned upload URL for attachment
        [HttpPost("upload-url")]
        public async Task<ActionResult<AcademicRequestUploadResponse>> GetUploadUrl([FromBody] GetUploadUrlRequest request)
        {
            var result = await _academicRequestService.GetAttachmentUploadUrlAsync(request.FileName, request.ContentType);
            return Ok(result);
        }

        // GET api/academicrequest/download-url?filePath={filePath} - Get presigned download URL for attachment
        [HttpGet("download-url")]
        public async Task<ActionResult<string>> GetDownloadUrl([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest("File path is required");
            }

            var result = await _academicRequestService.GetAttachmentDownloadUrlAsync(filePath);
            return Ok(new { downloadUrl = result });
        }

        // POST api/academicrequest/suspension/validate - Validate a suspension request before submission
        [HttpPost("suspension/validate")]
        public async Task<ActionResult<SuspensionValidationResult>> ValidateSuspension([FromBody] CreateSuspensionRequest requestDto)
        {
            if (_suspensionValidationService == null)
            {
                return StatusCode(500, new { message = "Suspension validation service is not available" });
            }

            var result = await _suspensionValidationService.ValidateSuspensionRequestAsync(requestDto);
            return Ok(result);
        }

        // POST api/academicrequest/suspension - Submit a suspension request with specialized DTO
        [HttpPost("suspension")]
        public async Task<ActionResult<AcademicRequestResponse>> SubmitSuspension([FromBody] CreateSuspensionRequest requestDto)
        {
            // Convert to generic academic request
            var academicRequest = new CreateAcademicRequest
            {
                StudentID = requestDto.StudentID,
                RequestTypeID = requestDto.RequestTypeID,
                Reason = requestDto.ReasonDetail,
                SuspensionStartDate = requestDto.StartDate,
                SuspensionEndDate = requestDto.EndDate,
                ReasonCategory = requestDto.ReasonCategory,
                AttachmentUrl = requestDto.AttachmentUrl
            };

            var result = await _academicRequestService.SubmitRequestAsync(academicRequest);
            return Ok(result);
        }

        // POST api/academicrequest/dropout/validate - Validate a dropout request before submission
        [HttpPost("dropout/validate")]
        public async Task<ActionResult<DropoutValidationResult>> ValidateDropout([FromBody] CreateDropoutRequest requestDto)
        {
            if (_dropoutValidationService == null)
            {
                return StatusCode(500, new { message = "Dropout validation service is not available" });
            }

            var result = await _dropoutValidationService.ValidateDropoutRequestAsync(requestDto);
            return Ok(result);
        }

        // POST api/academicrequest/dropout - Submit a dropout request with specialized DTO
        [HttpPost("dropout")]
        public async Task<ActionResult<AcademicRequestResponse>> SubmitDropout([FromBody] CreateDropoutRequest requestDto)
        {
            // Convert to generic academic request
            var academicRequest = new CreateAcademicRequest
            {
                StudentID = requestDto.StudentID,
                RequestTypeID = requestDto.RequestTypeID,
                Reason = requestDto.ReasonDetail,
                EffectiveDate = requestDto.EffectiveDate,
                ReasonCategory = requestDto.ReasonCategory,
                AttachmentUrl = requestDto.AttachmentUrl,
                CompletedExitSurvey = requestDto.CompletedExitSurvey,
                ExitSurveyId = requestDto.ExitSurveyId
            };

            var result = await _academicRequestService.SubmitRequestAsync(academicRequest);
            return Ok(result);
        }

        // PUT api/academicrequest/update-attachment - Update attachment for request with NeedInfo status
        [HttpPut("update-attachment")]
        public async Task<IActionResult> UpdateAttachment([FromBody] UpdateAcademicRequestAttachment requestDto)
        {
            try
            {
                await _academicRequestService.UpdateAttachmentAsync(requestDto);
                return Ok(new { message = "Attachment updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
