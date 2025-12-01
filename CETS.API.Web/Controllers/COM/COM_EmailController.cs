using Application.Interfaces.ACAD;
using Application.Interfaces.Common.Email;
using Application.Interfaces.CORE;
using Application.Interfaces.FIN;
using Domain.Constants;
using DTOs.ACAD.ACAD_AcademicRequest.Requests;
using DTOs.COM.COM_Email.Requests;
using Infrastructure.Implementations.Common.Email.EmailTemplates;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.COM
{
    [ApiController]
    [Route("api/postponed-class")]
    public class COM_EmailController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly EmailTemplateBuilder _templateBuilder;
        private readonly IACAD_AcademicRequestService _academicRequestService;
        private readonly IACAD_EnrollmentService _enrollmentService;
        private readonly ICORE_LookUpService _lookUpService;
        
        private readonly IFIN_PaymentService _paymentService;
        private readonly IConfiguration _configuration;
        

        public COM_EmailController(
            IMailService mailService,
            ICORE_LookUpService lookUpService,
            EmailTemplateBuilder templateBuilder,
            IACAD_AcademicRequestService academicRequestService,
            IACAD_EnrollmentService enrollmentService,
            IFIN_PaymentService paymentService,
        IConfiguration configuration)
        {
            _mailService = mailService;
            _templateBuilder = templateBuilder;
            _academicRequestService = academicRequestService;
            _enrollmentService = enrollmentService;
            _configuration = configuration;
            _lookUpService = lookUpService;
            _paymentService = paymentService;
        }

        // -------------------------------------------------------
        // 1. SEND POSTPONED EMAILS
        // -------------------------------------------------------
        [HttpPost("notify")]
        public async Task<IActionResult> SendPostponedClassEmail(
            [FromBody] PostponedClassNotifyRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (request.Students == null || request.Students.Count == 0)
                return BadRequest("Students list cannot be empty.");

            var baseUrl = _configuration["App:PublicBaseUrl"]
                          ?? $"{Request.Scheme}://{Request.Host}";

            var controllerBaseUrl = $"{baseUrl}/api/postponed-class";

            var results = new List<object>();

            foreach (var student in request.Students)
            {
                var encodedEmail = Uri.EscapeDataString(student.StudentEmail);
                var encodedName = Uri.EscapeDataString(student.StudentName);
                var encodedCourse = Uri.EscapeDataString(request.CourseName);
                var encodedDate = Uri.EscapeDataString(request.PlannedStartDate.ToString("yyyy-MM-dd"));

                var commonQuery =
                    $"enrollmentId={student.EnrollmentId}" +
                    $"&studentId={student.StudentId}" +
                    $"&studentEmail={encodedEmail}" +
                    $"&studentName={encodedName}" +
                    $"&courseName={encodedCourse}" +
                    $"&plannedStartDate={encodedDate}";

                var refundUrl = $"{controllerBaseUrl}/decision?{commonQuery}&decision=refund";
                var waitUrl = $"{controllerBaseUrl}/decision?{commonQuery}&decision=wait";

                var htmlBody = _templateBuilder.BuildClassPostponeEmail(
                    student.StudentName,
                    request.CourseName,
                    request.PlannedStartDate,
                    waitUrl,
                    refundUrl
                );

                await _mailService.SendEmailAsync(
                    student.StudentEmail,
                    $"[CETS] Class Postponed – {request.CourseName}",
                    htmlBody
                );

                results.Add(new
                {
                    student.StudentId,
                    student.StudentEmail,
                    student.EnrollmentId,
                    RefundUrl = refundUrl,
                    WaitUrl = waitUrl,
                    Status = "Email sent"
                });
            }

            return Ok(new
            {
                message = $"Postponed-class emails sent to {request.Students.Count} students.",
                details = results
            });
        }

        // -------------------------------------------------------
        // 2. HANDLE DECISION (REFUND / WAIT)
        // -------------------------------------------------------
        [HttpGet("decision")]
        public async Task<IActionResult> HandlePostponedClassDecision(
            [FromQuery] PostponedClassDecisionRequest request)
        {
            if (request.EnrollmentId == Guid.Empty)
                return BadRequest("enrollmentId is required.");

            if (string.IsNullOrWhiteSpace(request.Decision))
                return BadRequest("decision is required (refund | wait).");

            // 1) Lấy Enrollment + Check status
            var checkStatus = await _enrollmentService.GetDecisionStatusAsync(request.EnrollmentId);

            if (checkStatus == EmailDecisionStatus.Refund)
                return Content("You have already selected REFUND. This link is no longer valid.");

            if (checkStatus == EmailDecisionStatus.Waiting)
                return Content("You have already selected CONTINUE WAITING. This link is no longer valid.");

            // ===================== REFUND =====================
            if (request.Decision.Equals("refund", StringComparison.OrdinalIgnoreCase))
            {
                await _enrollmentService.UpdateDecisionStatusAsync(
                    request.EnrollmentId,
                    EmailDecisionStatus.Refund
                );

                // create academic request
                var requestType = await _lookUpService.GetByCodeAsync("AcademicRequestType", "Refund"); //Guid.Parse("019acdcc-7e3c-7958-a902-fa8db92acd9d");
                var priority = await _lookUpService.GetByCodeAsync("Priorty", "High");
                var enrollment = await _enrollmentService.GetEnrollmentForRefund(request.EnrollmentId);
                  var payment = await _paymentService.GetPaymentsByInvoiceIdAsync(enrollment!.InvoiceId);

                var createReq = new CreateAcademicRequest
                {
                    StudentID = request.StudentId,
                    RequestTypeID = requestType.LookUpId,
                    PriorityID = priority.LookUpId,
                    Reason = "Requested refund after class postponement.",
                    EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    EnrollmentID = request.EnrollmentId,
                    PaymentID = payment.FirstOrDefault()?.Id
                };

                var academicRequest = await _academicRequestService.SubmitRequestAsync(createReq);

               

                var refundHtml = _templateBuilder.BuildRefundConfirmationEmail(
                    request.StudentName,
                    request.CourseName,
                    payment.FirstOrDefault()?.Amount ?? 0,
                    enrollment?.FirstPaymentMethod ?? "Bank Transfer",
                    DateTime.UtcNow,
                    DateTime.UtcNow
                );

                await _mailService.SendEmailAsync(
                    request.StudentEmail,
                    $"[CETS] Refund Request Received – {request.CourseName}",
                    refundHtml
                );

                return Ok(new
                {
                    message = "Refund request created & email sent.",
                    academicRequestId = academicRequest.Id
                });
            }

            // ===================== WAIT =====================
            if (request.Decision.Equals("wait", StringComparison.OrdinalIgnoreCase))
            {
                await _enrollmentService.UpdateDecisionStatusAsync(
                    request.EnrollmentId,
                    EmailDecisionStatus.Waiting
                );

                var htmlBody = _templateBuilder.BuildContinueWaitingEmail(
                    request.StudentName,
                    request.CourseName,
                    request.PlannedStartDate
                );

                await _mailService.SendEmailAsync(
                    request.StudentEmail,
                    $"[CETS] Confirmation – You are waiting for {request.CourseName}",
                    htmlBody
                );

                return Ok(new { message = "Waiting confirmation email sent." });
            }

            return BadRequest("Invalid decision. Use 'refund' or 'wait'.");
        }
    }
}

