using Application.Interfaces.ACAD;
using Application.Interfaces.Common.Storage;
using Application.Interfaces.Common.Email;
using DTOs;
using DTOs.ACAD.ACAD_AcademicRequest.Requests;
using DTOs.Message.Requests;
using Infrastructure.Implementations.Common.Email.EmailTemplates;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using Utils.Helpers;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //private readonly IPublishEndpoint _publishEndpoint;
        private readonly IdGenerator _idGenerator;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMailService _mailService;
        private readonly EmailTemplateBuilder _templateBuilder;
        private readonly IACAD_AcademicRequestService _academicRequestService;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IdGenerator idGenerator, IFileStorageService fileStorageService, IMailService mailService,
            EmailTemplateBuilder templateBuilder,
            IACAD_AcademicRequestService academicRequestService)
        {
            _logger = logger;
            //_publishEndpoint = publishEndpoint;
            _idGenerator = idGenerator;
            _fileStorageService = fileStorageService;
            _mailService = mailService;
            _templateBuilder = templateBuilder;
            _academicRequestService = academicRequestService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        //[HttpPost("SendMessage")]
        //public async Task<IActionResult> SendEmail([FromBody] CreateMessageRequest message)
        //{
        //    await _publishEndpoint.Publish(message);
        //    return Ok("Message queued!");
        //}

        [HttpGet("GenerateId")]
        public IActionResult GenerateId()
        {
            var id = _idGenerator.GenerateId();
            return Ok(new { Id = id });
        }

        /// <summary>
        /// Test R2 connection
        /// </summary>
        [HttpGet("test-r2")]
        public async Task<ActionResult> TestR2Connection()
        {
            try
            {
                // Generate a simple presigned URL to test connection
                var testUrl = await _fileStorageService.GetTestPresignedUrlAsync();
                return Ok(new { message = "R2 connection successful", testUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "R2 connection test failed");
                return StatusCode(500, $"R2 connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi email hoãn (hoặc không mở được) lớp, cho chọn Refund / Đợi lớp
        /// GET: /api/test-email/postpone
        /// </summary>
        [HttpGet("postpone")]
        public async Task<IActionResult> SendClassPostponeEmail()
        {
            // ====== FAKE DATA – sau này lấy từ DB (reservation, course, student) ======
            var studentId = Guid.Parse("cd963609-83a1-827c-9de3-019a5920e939");

            string studentName = "Nguyễn Văn A";
            string studentEmail = "trungnmde170101@fpt.edu.vn";
            // Dù chưa xếp lớp, bạn vẫn có thể show tên khóa / lớp dự kiến:
            string className = "English Foundation A1 (planned)";
            DateTime originalPlannedStartDate = new DateTime(2025, 2, 10);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // 2 link quyết định, KHÔNG chứa classId
            var refundUrl =
                $"{baseUrl}/api/WeatherForecast/postpone-decision?studentId={studentId}&decision=refund";
            var continueUrl =
                $"{baseUrl}/api/WeatherForecast/postpone-decision?studentId={studentId}&decision=wait";

            string htmlBody = _templateBuilder.BuildClassPostponeEmail(
                studentName,
                className,
                originalPlannedStartDate,
                continueUrl,
                refundUrl
            );

            await _mailService.SendEmailAsync(
                studentEmail,
                $"[TEST] Class Postponed – {className}",
                htmlBody
            );

            return Ok(new
            {
                message = "Postpone email sent.",
                studentId,
                refundUrl,
                continueUrl
            });
        }

        /// <summary>
        /// Xử lý khi user bấm link trong email:
        /// - decision=refund => tạo AcademicRequest refund (không có ClassID)
        /// - decision=wait   => gửi email xác nhận đợi lớp
        /// GET: /api/test-email/postpone-decision?studentId=...&decision=...
        /// </summary>
        [HttpGet("postpone-decision")]
        public async Task<IActionResult> HandlePostponeDecision(
            [FromQuery] Guid studentId,
            [FromQuery] string decision)
        {
            if (string.IsNullOrWhiteSpace(decision))
                return BadRequest("decision is required (refund | wait).");

            // ====== CASE 1: REFUND => TẠO ACADEMIC REQUEST (không FromClassID) ======
            if (decision.Equals("refund", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: ID loại yêu cầu Refund trong lookup của bạn
                Guid refundRequestTypeId = Guid.Parse("019acdcc-7e3c-7958-a902-fa8db92acd9d");
                Guid? defaultPriorityId = Guid.Parse("019abb05-4d9f-7b4e-9293-f37b7c61dae9");

                var createRequest = new CreateAcademicRequest
                {
                    StudentID = studentId,
                    RequestTypeID = refundRequestTypeId,
                    PriorityID = defaultPriorityId,

                    Reason = "Student requested refund because the planned class could not be opened.",

                    // Chưa xếp lớp => để null hết các field liên quan lớp/buổi
                    FromClassID = null,
                    ToClassID = null,
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Now),

                    FromMeetingDate = null,
                    FromSlotID = null,
                    ToMeetingDate = null,
                    ToSlotID = null,
                    AttachmentUrl = null,
                    ClassMeetingID = null,
                    NewRoomID = null
                };

                var created = await _academicRequestService.SubmitRequestAsync(createRequest);

                return Ok(new
                {
                    message = "Refund academic request created successfully (no class assigned yet).",
                    academicRequestId = created.Id
                });
            }

            // ====== CASE 2: ĐỢI LỚP => GỬI MAIL XÁC NHẬN ======
            if (decision.Equals("wait", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: production thì lấy thật từ DB bằng studentId
                string studentName = "Nguyễn Văn A";
                string studentEmail = "trungnmde170101@fpt.edu.vn";
                string courseName = "English Foundation A1";
                DateTime originalPlannedStartDate = new DateTime(2025, 2, 10);

                var htmlBody = _templateBuilder.BuildContinueWaitingEmail(
                    studentName,
                    courseName,
                    originalPlannedStartDate
                );

                await _mailService.SendEmailAsync(
                    studentEmail,
                    $"Confirmation – You are waiting for {courseName}",
                    htmlBody
                );

                return Ok(new { message = "Waiting confirmation email sent." });
            }

            return BadRequest("Invalid decision. Use 'refund' or 'wait'.");
        }





    }
}
