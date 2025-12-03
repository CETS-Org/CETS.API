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
    [Route("api/email")]
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

            var baseUrl = _configuration["VerificationSettings:ApiBaseUrl"]
                          ?? $"{Request.Scheme}://{Request.Host}";

            var controllerBaseUrl = $"{baseUrl}/api/email";

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
          
            var frontendUrl = _configuration["VerificationSettings:FrontendPostponeNoti"] ?? "http://localhost:5173";
            var redirectBaseUrl = $"{frontendUrl}/postpone-confirmation";

            // Helper để tạo URL chuyển hướng kèm query params
            string BuildRedirectUrl(string status, string type = "", string code = "", string courseName = "")
            {
                var query = $"status={status}";
                if (!string.IsNullOrEmpty(type)) query += $"&type={type}";
                if (!string.IsNullOrEmpty(code)) query += $"&code={code}";
                if (!string.IsNullOrEmpty(courseName)) query += $"&course={Uri.EscapeDataString(courseName)}";

                return $"{redirectBaseUrl}?{query}";
            }

            // 2. Validate Request
            if (request.EnrollmentId == Guid.Empty || string.IsNullOrWhiteSpace(request.Decision))
            {
                return Redirect(BuildRedirectUrl("error", "", "invalid_request"));
            }

            try
            {
                // 3. Kiểm tra trạng thái hiện tại (Tránh click 2 lần)
                var checkStatus = await _enrollmentService.GetDecisionStatusAsync(request.EnrollmentId);

                if (checkStatus == EmailDecisionStatus.Refund)
                    return Redirect(BuildRedirectUrl("error", "", "already_refunded"));

                if (checkStatus == EmailDecisionStatus.Waiting)
                    return Redirect(BuildRedirectUrl("error", "", "already_waiting"));

                // ===================== CASE 1: REFUND =====================
                if (request.Decision.Equals("refund", StringComparison.OrdinalIgnoreCase))
                {
                    // A. Cập nhật trạng thái Enrollment
                    await _enrollmentService.UpdateDecisionStatusAsync(
                        request.EnrollmentId,
                        EmailDecisionStatus.Refund
                    );

                    // B. Tạo Academic Request (Yêu cầu hoàn tiền)
                    var requestType = await _lookUpService.GetByCodeAsync("AcademicRequestType", "Refund");
                    var priority = await _lookUpService.GetByCodeAsync("Priorty", "High");
                    var enrollment = await _enrollmentService.GetEnrollmentForRefund(request.EnrollmentId);
                    var payment = await _paymentService.GetPaymentsByInvoiceIdAsync(enrollment!.InvoiceId);

                    var createReq = new CreateAcademicRequest
                    {
                        StudentID = request.StudentId,
                        RequestTypeID = requestType.LookUpId,
                        PriorityID = priority.LookUpId,
                        Reason = "Requested refund after class postponement (Auto-generated).",
                        EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        EnrollmentID = request.EnrollmentId,
                        PaymentID = payment.FirstOrDefault()?.Id
                    };

                    await _academicRequestService.SubmitRequestAsync(createReq);

                    // C. Gửi mail xác nhận
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

                    // D. Chuyển hướng về trang Thành công (Refund)
                    return Redirect(BuildRedirectUrl("success", "refund", "", request.CourseName));
                }

                // ===================== CASE 2: WAIT =====================
                if (request.Decision.Equals("wait", StringComparison.OrdinalIgnoreCase))
                {
                    // A. Cập nhật trạng thái
                    await _enrollmentService.UpdateDecisionStatusAsync(
                        request.EnrollmentId,
                        EmailDecisionStatus.Waiting
                    );

                    // B. Gửi mail xác nhận
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

                    // C. Chuyển hướng về trang Thành công (Wait)
                    return Redirect(BuildRedirectUrl("success", "wait", "", request.CourseName));
                }

                return Redirect(BuildRedirectUrl("error", "", "invalid_decision"));
            }
            catch (Exception ex)
            {
                // Log error here
                return Redirect(BuildRedirectUrl("error", "", "server_error"));
            }
        }
    }
}

