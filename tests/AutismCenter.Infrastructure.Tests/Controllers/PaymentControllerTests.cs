using AutismCenter.Application.Features.Orders.Commands.CreatePaymentIntent;
using AutismCenter.Application.Features.Orders.Commands.ProcessPayment;
using AutismCenter.Application.Features.Orders.Commands.RefundPayment;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Infrastructure.Services;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IStripeWebhookService> _webhookServiceMock;
    private readonly Mock<ILogger<PaymentController>> _loggerMock;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _webhookServiceMock = new Mock<IStripeWebhookService>();
        _loggerMock = new Mock<ILogger<PaymentController>>();

        _controller = new PaymentController(
            _mediatorMock.Object,
            _webhookServiceMock.Object,
            _loggerMock.Object);

        // Setup controller context for authorization
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User")
        }, "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreatePaymentIntent_ValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentIntentRequest(orderId);
        var expectedResult = new CreatePaymentIntentResult(
            true, 
            "pi_test_123_secret", 
            "pi_test_123", 
            null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePaymentIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreatePaymentIntent(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<CreatePaymentIntentResult>().Subject;
        
        returnValue.IsSuccess.Should().BeTrue();
        returnValue.ClientSecret.Should().Be("pi_test_123_secret");
        returnValue.PaymentIntentId.Should().Be("pi_test_123");
        returnValue.ErrorMessage.Should().BeNull();

        _mediatorMock.Verify(x => x.Send(
            It.Is<CreatePaymentIntentCommand>(c => c.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentIntent_FailedResult_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentIntentRequest(orderId);
        var expectedResult = new CreatePaymentIntentResult(
            false, 
            null, 
            null, 
            "Order not found");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePaymentIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreatePaymentIntent(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnValue = badRequestResult.Value.Should().BeOfType<CreatePaymentIntentResult>().Subject;
        
        returnValue.IsSuccess.Should().BeFalse();
        returnValue.ErrorMessage.Should().Be("Order not found");
    }

    [Fact]
    public async Task ProcessPayment_ValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentMethodId = "pm_test_123";
        var paymentIntentId = "pi_test_123";
        var request = new ProcessPaymentRequest(orderId, paymentMethodId, paymentIntentId);
        var expectedResult = new ProcessPaymentResult(
            true, 
            "pi_test_123", 
            null);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ProcessPayment(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<ProcessPaymentResult>().Subject;
        
        returnValue.IsSuccess.Should().BeTrue();
        returnValue.PaymentId.Should().Be("pi_test_123");
        returnValue.ErrorMessage.Should().BeNull();

        _mediatorMock.Verify(x => x.Send(
            It.Is<ProcessPaymentCommand>(c => 
                c.OrderId == orderId &&
                c.PaymentMethodId == paymentMethodId &&
                c.PaymentIntentId == paymentIntentId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_FailedResult_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentMethodId = "pm_test_123";
        var request = new ProcessPaymentRequest(orderId, paymentMethodId);
        var expectedResult = new ProcessPaymentResult(
            false, 
            null, 
            "Payment declined");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ProcessPayment(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnValue = badRequestResult.Value.Should().BeOfType<ProcessPaymentResult>().Subject;
        
        returnValue.IsSuccess.Should().BeFalse();
        returnValue.ErrorMessage.Should().Be("Payment declined");
    }

    [Fact]
    public async Task RefundPayment_ValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var refundAmount = Money.Create(25.00m, "BHD");
        var reason = "Customer request";
        var request = new RefundPaymentRequest(orderId, refundAmount, reason);
        var expectedResult = new RefundPaymentResult(
            true, 
            "re_test_123", 
            null, 
            refundAmount);

        // Setup admin user for refund endpoint
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "test"));

        _controller.ControllerContext.HttpContext.User = adminUser;

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RefundPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RefundPayment(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<RefundPaymentResult>().Subject;
        
        returnValue.IsSuccess.Should().BeTrue();
        returnValue.RefundId.Should().Be("re_test_123");
        returnValue.ErrorMessage.Should().BeNull();
        returnValue.RefundedAmount.Should().Be(refundAmount);

        _mediatorMock.Verify(x => x.Send(
            It.Is<RefundPaymentCommand>(c => 
                c.OrderId == orderId &&
                c.RefundAmount == refundAmount &&
                c.Reason == reason),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefundPayment_FailedResult_ShouldReturnBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new RefundPaymentRequest(orderId);
        var expectedResult = new RefundPaymentResult(
            false, 
            null, 
            "Cannot refund order that hasn't been paid", 
            Money.Create(0));

        // Setup admin user for refund endpoint
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "test"));

        _controller.ControllerContext.HttpContext.User = adminUser;

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RefundPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RefundPayment(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnValue = badRequestResult.Value.Should().BeOfType<RefundPaymentResult>().Subject;
        
        returnValue.IsSuccess.Should().BeFalse();
        returnValue.ErrorMessage.Should().Be("Cannot refund order that hasn't been paid");
    }

    [Fact]
    public async Task StripeWebhook_ValidSignature_ShouldReturnOk()
    {
        // Arrange
        var payload = "{\"id\":\"evt_test\",\"type\":\"payment_intent.succeeded\"}";
        var signature = "t=1609459200,v1=test_signature";

        // Setup request body
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = signature;

        _webhookServiceMock
            .Setup(x => x.HandleWebhookAsync(payload, signature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.StripeWebhook(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();

        _webhookServiceMock.Verify(x => x.HandleWebhookAsync(
            payload, 
            signature, 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StripeWebhook_MissingSignature_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = "{\"id\":\"evt_test\",\"type\":\"payment_intent.succeeded\"}";

        // Setup request body without signature header
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;

        // Act
        var result = await _controller.StripeWebhook(CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Missing signature");

        _webhookServiceMock.Verify(x => x.HandleWebhookAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StripeWebhook_WebhookProcessingFailed_ShouldReturnBadRequest()
    {
        // Arrange
        var payload = "{\"id\":\"evt_test\",\"type\":\"payment_intent.succeeded\"}";
        var signature = "t=1609459200,v1=invalid_signature";

        // Setup request body
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = signature;

        _webhookServiceMock
            .Setup(x => x.HandleWebhookAsync(payload, signature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.StripeWebhook(CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Webhook processing failed");
    }
}