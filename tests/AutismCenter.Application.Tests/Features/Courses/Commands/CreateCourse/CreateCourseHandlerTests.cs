using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Courses.Commands.CreateCourse;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Courses.Commands.CreateCourse;

public class CreateCourseHandlerTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly CreateCourseHandler _handler;

    public CreateCourseHandlerTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _handler = new CreateCourseHandler(_courseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateCourseSuccessfully()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction to autism spectrum disorders",
            "مقدمة شاملة لاضطرابات طيف التوحد",
            120,
            99.99m,
            "USD",
            "CRS-001",
            "https://example.com/thumbnail.jpg"
        );

        _courseRepositoryMock.Setup(x => x.CodeExistsAsync(command.CourseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Course created successfully");
        result.Course.Should().NotBeNull();
        result.Course!.TitleEn.Should().Be(command.TitleEn);
        result.Course.TitleAr.Should().Be(command.TitleAr);
        result.Course.DescriptionEn.Should().Be(command.DescriptionEn);
        result.Course.DescriptionAr.Should().Be(command.DescriptionAr);
        result.Course.DurationInMinutes.Should().Be(command.DurationInMinutes);
        result.Course.Price.Should().Be(command.Price);
        result.Course.Currency.Should().Be(command.Currency);
        result.Course.CourseCode.Should().Be(command.CourseCode);
        result.Course.ThumbnailUrl.Should().Be(command.ThumbnailUrl);
        result.Course.IsActive.Should().BeTrue();

        _courseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingCourseCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction to autism spectrum disorders",
            "مقدمة شاملة لاضطرابات طيف التوحد",
            120,
            99.99m,
            "USD",
            "CRS-001"
        );

        _courseRepositoryMock.Setup(x => x.CodeExistsAsync(command.CourseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course creation failed");
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey(nameof(command.CourseCode));
        result.Errors![nameof(command.CourseCode)].Should().Contain("Course code already exists");

        _courseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutThumbnail_ShouldCreateCourseSuccessfully()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction to autism spectrum disorders",
            "مقدمة شاملة لاضطرابات طيف التوحد",
            120,
            99.99m,
            "USD",
            "CRS-001"
            // No thumbnail provided
        );

        _courseRepositoryMock.Setup(x => x.CodeExistsAsync(command.CourseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Course!.ThumbnailUrl.Should().BeNull();

        _courseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("BHD")]
    public async Task Handle_DifferentCurrencies_ShouldCreateCourseSuccessfully(string currency)
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction to autism spectrum disorders",
            "مقدمة شاملة لاضطرابات طيف التوحد",
            120,
            99.99m,
            currency,
            "CRS-001"
        );

        _courseRepositoryMock.Setup(x => x.CodeExistsAsync(command.CourseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Course!.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData("", "Valid Arabic", "Valid Description", "وصف صحيح")]
    [InlineData("Valid English", "", "Valid Description", "وصف صحيح")]
    [InlineData("Valid English", "Valid Arabic", "", "وصف صحيح")]
    [InlineData("Valid English", "Valid Arabic", "Valid Description", "")]
    public async Task Handle_InvalidInput_ShouldReturnFailure(string titleEn, string titleAr, string descriptionEn, string descriptionAr)
    {
        // Arrange
        var command = new CreateCourseCommand(
            titleEn,
            titleAr,
            descriptionEn,
            descriptionAr,
            120,
            99.99m,
            "USD",
            "CRS-001"
        );

        _courseRepositoryMock.Setup(x => x.CodeExistsAsync(command.CourseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey("ValidationError");

        _courseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_InvalidDuration_ShouldReturnFailure(int duration)
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction to autism spectrum disorders",
            "مقدمة شاملة لاضطرابات طيف التوحد",
            duration,
            99.99m,
            "USD",
            "CRS-001"
        );

        _courseRepositoryMock.Setup(x => x.CodeExistsAsync(command.CourseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey("ValidationError");

        _courseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}