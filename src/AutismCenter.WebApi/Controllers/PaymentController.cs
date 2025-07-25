using AutismCenter.Application.Features.Orders.Commands.CreatePaymentIntent;
using AutismCenter.Application.Features.Orders.Commands.ProcessPayment;
using AutismCenter.Application.Features.Orders.Commands.RefundPayment;
using AutismCenter.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStripeWebhookService _webhookService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IMediator mediator,
        IStripeWebhookService webhookService,
        ILogger<PaymentController> logger)
    {
        _mediator = mediator;
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost("create-payment-intent")]
    [Authorize]
    public async Task<ActionResult<CreatePaymentIntentResult>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePaymentIntentCommand(request.OrderId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("process")]
    [Authorize]
    public async Task<ActionResult<ProcessPaymentResult>> ProcessPayment(
        [FromBody] ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(
            request.OrderId,
            request.PaymentMethodId,
            request.PaymentIntentId);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("refund")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RefundPaymentResult>> RefundPayment(
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefundPaymentCommand(
            request.OrderId,
            request.RefundAmount,
            request.Reason);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook signature is missing");
            return BadRequest("Missing signature");
        }

        var success = await _webhookService.HandleWebhookAsync(payload, signature, cancellationToken);

        if (success)
        {
            return Ok();
        }

        return BadRequest("Webhook processing failed");
    }
}

public record CreatePaymentIntentRequest(Guid OrderId);

public record ProcessPaymentRequest(
    Guid OrderId,
    string PaymentMethodId,
    string? PaymentIntentId = null);

public record RefundPaymentRequest(
    Guid OrderId,
    Domain.ValueObjects.Money? RefundAmount = null,
    string? Reason = null);