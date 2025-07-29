using AutismCenter.Application.Features.Authentication.Commands.Login;
using AutismCenter.Application.Features.Authentication.Commands.RegisterUser;
using AutismCenter.Application.Common.Models;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Integration;

public class AuthenticationIntegrationTests
{
    [Fact]
    public void AuthController_ShouldHaveCorrectRouteAttribute()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/[controller]");
    }

    [Fact]
    public void AuthController_LoginEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var loginMethod = controllerType.GetMethod("Login");

        // Assert
        loginMethod.Should().NotBeNull();
        
        var httpPostAttribute = loginMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
        httpPostAttribute!.Template.Should().Be("login");

        var producesResponseTypeAttributes = loginMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(3);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status400BadRequest);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void AuthController_RegisterEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var registerMethod = controllerType.GetMethod("Register");

        // Assert
        registerMethod.Should().NotBeNull();
        
        var httpPostAttribute = registerMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
        httpPostAttribute!.Template.Should().Be("register");

        var producesResponseTypeAttributes = registerMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(2);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void AuthController_VerifyEmailEndpoint_ShouldHaveBothGetAndPostMethods()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var verifyEmailPostMethod = controllerType.GetMethod("VerifyEmail");
        var verifyEmailGetMethod = controllerType.GetMethod("VerifyEmailGet");

        // Assert
        verifyEmailPostMethod.Should().NotBeNull();
        verifyEmailGetMethod.Should().NotBeNull();

        // Check POST method
        var httpPostAttribute = verifyEmailPostMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
        httpPostAttribute!.Template.Should().Be("verify-email");

        // Check GET method
        var httpGetAttribute = verifyEmailGetMethod!.GetCustomAttributes(typeof(HttpGetAttribute), false)
            .FirstOrDefault() as HttpGetAttribute;
        httpGetAttribute.Should().NotBeNull();
        httpGetAttribute!.Template.Should().Be("verify-email");
    }

    [Fact]
    public void AuthController_GoogleLoginEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var googleLoginMethod = controllerType.GetMethod("GoogleLogin");

        // Assert
        googleLoginMethod.Should().NotBeNull();
        
        var httpPostAttribute = googleLoginMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
        httpPostAttribute!.Template.Should().Be("google-login");

        var producesResponseTypeAttributes = googleLoginMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(3);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status400BadRequest);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void AuthController_LogoutEndpoint_ShouldRequireAuthorization()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var logoutMethod = controllerType.GetMethod("Logout");

        // Assert
        logoutMethod.Should().NotBeNull();
        
        var authorizeAttribute = logoutMethod!.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault() as AuthorizeAttribute;
        authorizeAttribute.Should().NotBeNull();

        var httpPostAttribute = logoutMethod.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
        httpPostAttribute!.Template.Should().Be("logout");
    }

    [Fact]
    public void AuthController_PasswordResetEndpoints_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(AuthController);
        var forgotPasswordMethod = controllerType.GetMethod("ForgotPassword");
        var resetPasswordMethod = controllerType.GetMethod("ResetPassword");

        // Assert
        forgotPasswordMethod.Should().NotBeNull();
        resetPasswordMethod.Should().NotBeNull();

        // Check ForgotPassword method
        var forgotPasswordHttpPost = forgotPasswordMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        forgotPasswordHttpPost.Should().NotBeNull();
        forgotPasswordHttpPost!.Template.Should().Be("forgot-password");

        // Check ResetPassword method
        var resetPasswordHttpPost = resetPasswordMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        resetPasswordHttpPost.Should().NotBeNull();
        resetPasswordHttpPost!.Template.Should().Be("reset-password");
    }
}