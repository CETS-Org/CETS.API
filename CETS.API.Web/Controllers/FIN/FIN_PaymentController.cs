using Application.Interfaces.FIN;
using DTOs.FIN.FIN_Payment.Requests;
using DTOs.FIN.FIN_Payment.Responses;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CETS.API.Web.Controllers.FIN
{
	[Route("api/[controller]")]
	[ApiController]
	public class FIN_PaymentController : ControllerBase
	{
		private readonly IFIN_PaymentService _service;
		private readonly IFIN_InvoiceService _invoiceService;
		private readonly IFIN_InvoiceItemService _invoiceItemService;
		public readonly IConfiguration _configuration;

        public FIN_PaymentController(IFIN_PaymentService service, IFIN_InvoiceService invoiceService, IFIN_InvoiceItemService invoiceItemService,IConfiguration configuration)
		{
			_service = service;
			_invoiceService = invoiceService;
			_invoiceItemService = invoiceItemService;
			_configuration = configuration;
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

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreatePaymentRequest request)
		{
			var created = await _service.CreateAsync(request);
			return Created("Created", created);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdatePaymentRequest request)
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

        #region payment by invoice
		[HttpPost("monthlyPay")]
		public async Task<IActionResult> CreatePaymentsAsync([FromBody] PaymentRequest request)
		{
			if (request == null || request.ReservationItemId == null || request.StudentId == null) return BadRequest("Invalid request");

			var invoice = await _invoiceService.CreateInvolcesToMonthlyPay(Guid.Parse(request.ReservationItemId), Guid.Parse(request.StudentId));

			if(invoice == null)
			{
                return BadRequest("Invalid request, can not create invoice!");
            }
            // Get PayOS configuration
            var clientId = _configuration["PayOS:ClientId"];
            var apiKey = _configuration["PayOS:ApiKey"];
            var checksumKey = _configuration["PayOS:ChecksumKey"];

            // Validate PayOS configuration
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
            {
                return StatusCode(500, "PayOS configuration is missing");
            }

            // Create payment items based on invoice
            var items = new List<ItemData>
        {
            new ItemData(
                name: invoice.InvoiceNumber ?? "Monthly Course Payment",
                quantity: 1,
                price: (int)invoice.TotalAmount 
            )
        };

            // Generate unique order code
            var orderCode = GenerateSixDigitCode();

            // Create payment data
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: 2000,
                description: $"Payment for invoice ",
                items: items,
                returnUrl: _configuration["PayOS:ReturnUrl"] ?? "https://localhost:8000/payment/success",
                cancelUrl: _configuration["PayOS:CancelUrl"] ?? "https://localhost:8000/payment/cancel"
            );

            // Initialize PayOS
            var payOS = new PayOS(clientId, apiKey, checksumKey);

            // Create payment link
            var createPaymentResult = await payOS.createPaymentLink(paymentData);

            //await payOS.confirmWebhook("https://localhost:8000/api/FIN_Payment/webhook");

            return Ok(new
            {
                success = true,
                paymentUrl = createPaymentResult.checkoutUrl,
                orderCode = orderCode,
                amount = invoice.TotalAmount,
                invoiceId = invoice.Id
            });
        }
        #endregion

        #region PayOS Callback URLs
        [HttpGet("success")]
        public async Task<IActionResult> HandleReturnUrl([FromQuery] string? code, [FromQuery] string? id, [FromQuery] bool cancel, [FromQuery] string? status, [FromQuery] string? orderCode, [FromQuery] string? invoiceId)
        {
            try
            {
                var response = new PaymentReturnResponse
                {
                    Success = !cancel && status == "PAID",
                    Message = cancel ? "Payment was cancelled by user" : (status == "PAID" ? "Payment completed successfully" : "Payment failed"),
                    OrderCode = orderCode,
                    Status = status,
                    RedirectUrl = _configuration["Frontend:PaymentSuccessUrl"] ?? "http://localhost:3000/payment/success"
                };

                if (response.Success && !string.IsNullOrEmpty(orderCode))
                {
                    // Update payment status in database
                    await _service.CreateMonthlyPayment(Guid.Parse(invoiceId));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaymentReturnResponse
                {
                    Success = false,
                    Message = "An error occurred while processing payment return",
                    RedirectUrl = _configuration["Frontend:PaymentErrorUrl"] ?? "http://localhost:3000/payment/error"
                });
            }
        }

        [HttpGet("cancel")]
        public IActionResult HandleCancelUrl()
        {
            var response = new PaymentReturnResponse
            {
                Success = false,
                Message = "Payment was cancelled by user",
                RedirectUrl = _configuration["Frontend:PaymentCancelUrl"] ?? "http://localhost:3000/payment/cancel"
            };

            return Ok(response);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PayOSWebhookRequest webhookRequest)
        {
            try
            {
                // Verify webhook signature
                if (!VerifyWebhookSignature(webhookRequest))
                {
                    return Unauthorized("Invalid webhook signature");
                }

                // Process webhook data
                if (webhookRequest.Data != null)
                {
                    var orderCode = webhookRequest.Data.OrderCode;
                    var amount = webhookRequest.Data.Amount;
                    var status = webhookRequest.Data.Code;

                    // Update payment status in database
                    // await _service.UpdatePaymentStatusByOrderCodeAsync(orderCode, status);

                    // Log webhook data
                    // await _service.LogWebhookDataAsync(webhookRequest);

                    // Handle different payment statuses
                    switch (status)
                    {
                        case "00": // Success
                            // Payment successful - update invoice status, send confirmation email, etc.
                            break;
                        case "01": // Pending
                            // Payment pending - update status
                            break;
                        case "02": // Failed
                            // Payment failed - handle accordingly
                            break;
                        default:
                            // Unknown status
                            break;
                    }
                }

                return Ok(new { success = true, message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { success = false, message = "Error processing webhook" });
            }
        }

        private bool VerifyWebhookSignature(PayOSWebhookRequest webhookRequest)
        {
            try
            {
                var checksumKey = _configuration["PayOS:ChecksumKey"];
                if (string.IsNullOrEmpty(checksumKey))
                {
                    return false;
                }

                // Create the data string for signature verification
                var dataString = $"{webhookRequest.Code}|{webhookRequest.Description}|{JsonSerializer.Serialize(webhookRequest.Data)}";
                
                // Generate HMAC SHA256 signature
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
                var computedSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString)));

                return computedSignature.Equals(webhookRequest.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public static long GenerateSixDigitCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000);
        }
    }
}



