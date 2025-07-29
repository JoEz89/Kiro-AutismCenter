using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Integration;

public class UsersIntegrationTests
{
    [Fact]
    public void UsersController_ShouldHaveCorrectRouteAndAuthorizationAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(UsersController);
        
        // Assert
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/[controller]");

        var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault() as AuthorizeAttribute;
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public void UsersController_GetProfileEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(UsersController);
        var getProfileMethod = controllerType.GetMethod("GetProfile");

        // Assert
        getProfileMethod.Should().NotBeNull();
        
        var httpGetAttribute = getProfileMethod!.GetCustomAttributes(typeof(HttpGetAttribute), false)
            .FirstOrDefault() as HttpGetAttribute;
        httpGetAttribute.Should().NotBeNull();
        httpGetAttribute!.Template.Should().Be("profile");

        var producesResponseTypeAttributes = getProfileMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(3);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public void UsersController_GetUserEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(UsersController);
        var getUserMethod = controllerType.GetMethod("GetUser");

        // Assert
        getUserMethod.Should().NotBeNull();
        
        var httpGetAttribute = getUserMethod!.GetCustomAttributes(typeof(HttpGetAttribute), false)
            .FirstOrDefault() as HttpGetAttribute;
        httpGetAttribute.Should().NotBeNull();
        httpGetAttribute!.Template.Should().Be("{id:guid}");

        var producesResponseTypeAttributes = getUserMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(3);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public void UsersController_UpdateProfileEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(UsersController);
        var updateProfileMethod = controllerType.GetMethod("UpdateProfile");

        // Assert
        updateProfileMethod.Should().NotBeNull();
        
        var httpPutAttribute = updateProfileMethod!.GetCustomAttributes(typeof(HttpPutAttribute), false)
            .FirstOrDefault() as HttpPutAttribute;
        httpPutAttribute.Should().NotBeNull();
        httpPutAttribute!.Template.Should().Be("profile");

        var producesResponseTypeAttributes = updateProfileMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(3);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status400BadRequest);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void UsersController_ChangePasswordEndpoint_ShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var controllerType = typeof(UsersController);
        var changePasswordMethod = controllerType.GetMethod("ChangePassword");

        // Assert
        changePasswordMethod.Should().NotBeNull();
        
        var httpPostAttribute = changePasswordMethod!.GetCustomAttributes(typeof(HttpPostAttribute), false)
            .FirstOrDefault() as HttpPostAttribute;
        httpPostAttribute.Should().NotBeNull();
        httpPostAttribute!.Template.Should().Be("change-password");

        var producesResponseTypeAttributes = changePasswordMethod.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>().ToList();
        
        producesResponseTypeAttributes.Should().HaveCount(3);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status200OK);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status400BadRequest);
        producesResponseTypeAttributes.Should().Contain(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void UsersController_RequestModels_ShouldHaveCorrectProperties()
    {
        // Test UpdateUserProfileRequest
        var updateRequestType = typeof(UpdateUserProfileRequest);
        updateRequestType.Should().NotBeNull();
        
        var updateRequestProperties = updateRequestType.GetProperties();
        updateRequestProperties.Should().Contain(p => p.Name == "FirstName");
        updateRequestProperties.Should().Contain(p => p.Name == "LastName");
        updateRequestProperties.Should().Contain(p => p.Name == "PhoneNumber");
        updateRequestProperties.Should().Contain(p => p.Name == "PreferredLanguage");

        // Test ChangePasswordRequest
        var changePasswordRequestType = typeof(ChangePasswordRequest);
        changePasswordRequestType.Should().NotBeNull();
        
        var changePasswordProperties = changePasswordRequestType.GetProperties();
        changePasswordProperties.Should().Contain(p => p.Name == "CurrentPassword");
        changePasswordProperties.Should().Contain(p => p.Name == "NewPassword");
        changePasswordProperties.Should().Contain(p => p.Name == "ConfirmNewPassword");
    }

    [Fact]
    public void UsersController_ShouldInheritFromBaseController()
    {
        // Arrange & Act
        var controllerType = typeof(UsersController);

        // Assert
        controllerType.BaseType.Should().Be(typeof(BaseController));
    }
}