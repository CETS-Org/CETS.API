using Application.Interfaces.FIN;
using Application.Interfaces.CORE;
using Domain.Entities;
using DTOs.FIN.FIN_Payment.Requests;
using DTOs.FIN.FIN_Payment.Responses;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Interfaces.ACAD;
using DTOs.FIN.FIN_Invoice.Responses;

namespace CETS.API.Web.Controllers.FIN
{
    [Route("api/[controller]")]
    [ApiController]
    public class FIN_PaymentController : ControllerBase
    {
        private readonly IFIN_PaymentService _service;
        private readonly IFIN_InvoiceService _invoiceService;
        private readonly IFIN_InvoiceItemService _invoiceItemService;
        private readonly IACAD_ReservationItemService _reservationItemService;
        private readonly ICORE_LookUpService _lookUpService;
        public readonly IConfiguration _configuration;

        public FIN_PaymentController(IFIN_PaymentService service, IFIN_InvoiceService invoiceService, IFIN_InvoiceItemService invoiceItemService, IACAD_ReservationItemService reservationItemService, ICORE_LookUpService lookUpService, IConfiguration configuration)
        {
            _service = service;
            _invoiceService = invoiceService;
            _invoiceItemService = invoiceItemService;
            _reservationItemService = reservationItemService;
            _lookUpService = lookUpService;
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

        #region Monthly Payment Processing
        [HttpPost("monthlyPay")]
        public async Task<IActionResult> CreatePaymentsAsync([FromBody] PaymentRequest request)
        {
            if (request == null || request.ReservationItemId == null || request.StudentId == null) return BadRequest("Invalid request");

            // Get reservation item first to check current state
            var reservationItem = await _reservationItemService.GetReservationItemByIdAsync(Guid.Parse(request.ReservationItemId));
            if (reservationItem == null)
            {
                return BadRequest("Reservation item not found!");
            }
            
            InvoiceResponse invoice;
            decimal paymentAmount;
            bool isSecondPayment = false;
            
            // Check if reservation item already has an invoice
            if (reservationItem.InvoiceId != null)
            {
                // Get the existing invoice
                var existingInvoice = await _invoiceService.GetByIdAsync(reservationItem.InvoiceId.Value);
                if (existingInvoice == null)
                {
                    return BadRequest("Invoice not found!");
                }
                
                // Check invoice status to determine payment type
                var invoiceStatus = await _lookUpService.GetByIdAsync(existingInvoice.InvoiceStatusID);
                if (invoiceStatus == null)
                {
                    return BadRequest("Invalid invoice status!");
                }
                
                if (invoiceStatus.Code == "Pending")
                {
                    // First payment still pending - allow retry payment
                    invoice = existingInvoice;
                    var invoiceItems = await _invoiceItemService.GetByInvoiceIdAsync(invoice.Id);
                    var invoiceItemsList = invoiceItems.ToList();
                    
                    if (invoiceItemsList.Count < 1)
                    {
                        return BadRequest("Invoice does not have items for payment!");
                    }
                    
                    var firstInvoiceItem = invoiceItemsList[0]; // Get first item (index 0)
                    paymentAmount = firstInvoiceItem.Total;
                    isSecondPayment = false; // This is still first payment retry
                }
                else if (invoiceStatus.Code == "1stPaid")
                {
                    // Second payment - use existing invoice and get amount from second invoice item
                    invoice = existingInvoice;
                    var invoiceItems = await _invoiceItemService.GetByInvoiceIdAsync(invoice.Id);
                    var invoiceItemsList = invoiceItems.ToList();
                    
                    if (invoiceItemsList.Count < 2)
                    {
                        return BadRequest("Invoice does not have enough items for second payment!");
                    }
                    
                    var secondInvoiceItem = invoiceItemsList[1]; // Get second item (index 1)
                    paymentAmount = secondInvoiceItem.Total;
                    isSecondPayment = true;
                }
                else if (invoiceStatus.Code == "2ndPaid" || invoiceStatus.Code == "PaymentComplete")
                {
                    // All payments completed
                    return BadRequest("All payments for this reservation have been completed.");
                }
                else
                {
                    return BadRequest($"Invalid payment status: {invoiceStatus.Code}");
                }
            }
            else
            {
                // No invoice exists - create new invoice for first payment
                // This will automatically assign invoice to reservationItem
                invoice = await _invoiceService.CreateInvolcesToMonthlyPay(Guid.Parse(request.ReservationItemId), Guid.Parse(request.StudentId));
                if (invoice == null)
                {
                    return BadRequest("Invalid request, can not create invoice!");
                }
                
                // Get amount from first invoice item for first payment
                var invoiceItems = await _invoiceItemService.GetByInvoiceIdAsync(invoice.Id);
                var invoiceItemsList = invoiceItems.ToList();
                
                if (invoiceItemsList.Count < 1)
                {
                    return BadRequest("Invoice does not have items for payment!");
                }
                
                var firstInvoiceItem = invoiceItemsList[0]; // Get first item (index 0)
                paymentAmount = firstInvoiceItem.Total;
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
                price: (int)paymentAmount
            )
        };

            // Generate unique order code
            var orderCode = GenerateSixDigitCode();

            // Create payment data
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: 2000,
                description: $"Payment for CETS invoice",
                items: items,
                returnUrl: _configuration["PayOS:ReturnUrl"] ?? "https://localhost:8000/payment/success",
                cancelUrl: _configuration["PayOS:CancelUrl"] ?? "https://localhost:8000/payment/cancel"
            );

            // Initialize PayOS
            var payOS = new PayOS(clientId, apiKey, checksumKey);

            // Create payment link
            var createPaymentResult = await payOS.createPaymentLink(paymentData);

            //await payOS.confirmWebhook("https://localhost:8000/api/FIN_Payment/webhook");

            // Determine payment type and message
            string paymentType;
            string message;
            
            if (isSecondPayment)
            {
                paymentType = "Second Payment";
                message = "Proceeding with second installment payment";
            }
            else
            {
                // Check if this is a retry payment
                if (reservationItem.InvoiceId != null)
                {
                    paymentType = "First Payment (Retry)";
                    message = "Retrying first installment payment";
                }
                else
                {
                    paymentType = "First Payment";
                    message = "Proceeding with first installment payment";
                }
            }
            
            return Ok(new
            {
                success = true,
                paymentUrl = createPaymentResult.checkoutUrl,
                orderCode = orderCode,
                amount = paymentAmount,
                invoiceId = invoice.Id,
                paymentType = paymentType,
                message = message
            });
        }
        #endregion

        #region Full Payment Processing
        [HttpPost("fullPay")]
        public async Task<IActionResult> CreateFullPaymentAsync([FromBody] PaymentRequest request)
        {
            if (request == null || request.ReservationItemId == null || request.StudentId == null) return BadRequest("Invalid request");

            // Get reservation item first to check current state
            var reservationItem = await _reservationItemService.GetReservationItemByIdAsync(Guid.Parse(request.ReservationItemId));
            if (reservationItem == null)
            {
                return BadRequest("Reservation item not found!");
            }

            InvoiceResponse invoice;
            decimal paymentAmount;

            // Check if reservation item already has an invoice
            if (reservationItem.InvoiceId != null)
            {
                // Get the existing invoice
                var existingInvoice = await _invoiceService.GetByIdAsync(reservationItem.InvoiceId.Value);
                if (existingInvoice == null)
                {
                    return BadRequest("Invoice not found!");
                }

                // Check invoice status to determine if payment is allowed
                var invoiceStatus = await _lookUpService.GetByIdAsync(existingInvoice.InvoiceStatusID);
                if (invoiceStatus == null)
                {
                    return BadRequest("Invalid invoice status!");
                }

                if (invoiceStatus.Code == "Pending")
                {
                    // Payment still pending - allow retry payment
                    invoice = existingInvoice;
                    var invoiceItems = await _invoiceItemService.GetByInvoiceIdAsync(invoice.Id);
                    var invoiceItemsList = invoiceItems.ToList();

                    if (invoiceItemsList.Count < 1)
                    {
                        return BadRequest("Invoice does not have items for payment!");
                    }

                    var invoiceItem = invoiceItemsList[0]; // Get the only item
                    paymentAmount = invoiceItem.Total;
                }
                else if (invoiceStatus.Code == "PaymentComplete")
                {
                    // Payment already completed
                    return BadRequest("Payment for this reservation has been completed.");
                }
                else
                {
                    return BadRequest($"Invalid payment status: {invoiceStatus.Code}");
                }
            }
            else
            {
                // No invoice exists - create new invoice for full payment
                // This will automatically assign invoice to reservationItem
                invoice = await _invoiceService.CreateInvoiceForFullPayment(Guid.Parse(request.ReservationItemId), Guid.Parse(request.StudentId));
                if (invoice == null)
                {
                    return BadRequest("Invalid request, can not create invoice!");
                }

                // Get amount from the single invoice item for full payment
                var invoiceItems = await _invoiceItemService.GetByInvoiceIdAsync(invoice.Id);
                var invoiceItemsList = invoiceItems.ToList();

                if (invoiceItemsList.Count < 1)
                {
                    return BadRequest("Invoice does not have items for payment!");
                }

                var invoiceItem = invoiceItemsList[0]; // Get the only item
                paymentAmount = invoiceItem.Total;
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
                name: invoice.InvoiceNumber ?? "Full Course Payment",
                quantity: 1,
                price: (int)paymentAmount
            )
        };

            // Generate unique order code
            var orderCode = GenerateSixDigitCode();

            // Create payment data
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: 2000,
                description: $"Payment for CETS invoice",
                items: items,
                returnUrl: _configuration["PayOS:ReturnUrl"] ?? "https://localhost:8000/payment/success",
                cancelUrl: _configuration["PayOS:CancelUrl"] ?? "https://localhost:8000/payment/cancel"
            );

            // Initialize PayOS
            var payOS = new PayOS(clientId, apiKey, checksumKey);

            // Create payment link
            var createPaymentResult = await payOS.createPaymentLink(paymentData);

            // Determine payment type and message
            string paymentType;
            string message;

            if (reservationItem.InvoiceId != null)
            {
                paymentType = "Full Payment (Retry)";
                message = "Retrying full payment";
            }
            else
            {
                paymentType = "Full Payment";
                message = "Proceeding with full payment";
            }

            return Ok(new
            {
                success = true,
                paymentUrl = createPaymentResult.checkoutUrl,
                orderCode = orderCode,
                amount = paymentAmount,
                invoiceId = invoice.Id,
                paymentType = paymentType,
                message = message
            });
        }
        #endregion

        #region PayOS Callback URLs
        [HttpGet("success")]
        public async Task<IActionResult> HandleReturnUrl([FromQuery] string? code, [FromQuery] string? id, [FromQuery] bool cancel, [FromQuery] string? status,
            [FromQuery] string? orderCode, [FromQuery] string? invoiceId, [FromQuery] string? studentId, [FromQuery] string? reservationItemId)
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
                    await _service.CreateMonthlyPayment(Guid.Parse(invoiceId), Guid.Parse(studentId), Guid.Parse(reservationItemId));
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



