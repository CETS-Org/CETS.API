using Application.Interfaces.FIN;
using DTOs.FIN.FIN_Payment.Requests;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Cryptography;

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
                price: (int)invoice.TotalAmount // PayOS expects amount in VND (integer)
            )
        };

            // Generate unique order code
            var orderCode = GenerateSixDigitCode();

            // Create payment data
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: (int)invoice.TotalAmount,
                description: $"Payment for invoice ",
                items: items,
                returnUrl: _configuration["PayOS:ReturnUrl"] ?? "https://localhost:3000/payment/success",
                cancelUrl: _configuration["PayOS:CancelUrl"] ?? "https://localhost:3000/payment/cancel"
            );

            // Initialize PayOS
            var payOS = new PayOS(clientId, apiKey, checksumKey);

            // Create payment link
            var createPaymentResult = await payOS.createPaymentLink(paymentData);

            // Save payment info to database (recommended)
            //await SavePaymentInfo(invoice.Id, orderCode, createPaymentResult);

            // Return payment link to client
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

        public static long GenerateSixDigitCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000);
        }
    }
}



