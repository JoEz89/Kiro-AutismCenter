using AutismCenter.Application.Features.Courses.Queries.GetCourses;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Courses.Queries.GetCourses;

public class GetCoursesHandlerTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly GetCoursesHandler _handler;

    public GetCoursesHandlerTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _handler = new GetCoursesHandler(_courseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_GetActiveCoursesOnly_ShouldReturnActiveCourses()
    {
        // Arrange
        var activeCourses = new List<Course>
        {
            Course.Create("Course 1", "دورة 1", "Description 1", "وصف 1", 60, Money.Create(50, "USD"), "CRS-001"),
            Course.Create("Course 2", "دورة 2", "Description 2", "وصف 2", 90, Money.Create(75, "USD"), "CRS-002")
        };

        var query = new GetCoursesQuery(ActiveOnly: true);

        _courseRepositoryMock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCourses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Courses retrieved successfully");
        result.Courses.Should().HaveCount(2);
        result.Courses.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());

        _courseRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
        _courseRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        _courseRepositoryMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetAllCourses_ShouldReturnAllCourses()
    {
        // Arrange
        var allCourses = new List<Course>
        {
            Course.Create("Course 1", "دورة 1", "Description 1", "وصف 1", 60, Money.Create(50, "USD"), "CRS-001"),
            Course.Create("Course 2", "دورة 2", "Description 2", "وصف 2", 90, Money.Create(75, "USD"), "CRS-002")
        };
        allCourses[1].Deactivate(); // Make second course inactive

        var query = new GetCoursesQuery(ActiveOnly: false);

        _courseRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCourses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Courses retrieved successfully");
        result.Courses.Should().HaveCount(2);
        result.Courses.Should().Contain(c => c.IsActive);
        result.Courses.Should().Contain(c => !c.IsActive);

        _courseRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _courseRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _courseRepositoryMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SearchCourses_ShouldReturnMatchingCourses()
    {
        // Arrange
        var searchResults = new List<Course>
        {
            Course.Create("Autism Basics", "أساسيات التوحد", "Basic course", "دورة أساسية", 60, Money.Create(50, "USD"), "CRS-001")
        };

        var query = new GetCoursesQuery(ActiveOnly: true, SearchTerm: "autism");

        _courseRepositoryMock.Setup(x => x.SearchAsync("autism", It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Courses retrieved successfully");
        result.Courses.Should().HaveCount(1);
        result.Courses.First().TitleEn.Should().Contain("Autism");

        _courseRepositoryMock.Verify(x => x.SearchAsync("autism", It.IsAny<CancellationToken>()), Times.Once);
        _courseRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _courseRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SearchCoursesWithInactiveResults_ShouldFilterByActiveStatus()
    {
        // Arrange
        var searchResults = new List<Course>
        {
            Course.Create("Autism Basics", "أساسيات التوحد", "Basic course", "دورة أساسية", 60, Money.Create(50, "USD"), "CRS-001"),
            Course.Create("Advanced Autism", "التوحد المتقدم", "Advanced course", "دورة متقدمة", 120, Money.Create(100, "USD"), "CRS-002")
        };
        searchResults[1].Deactivate(); // Make second course inactive

        var query = new GetCoursesQuery(ActiveOnly: true, SearchTerm: "autism");

        _courseRepositoryMock.Setup(x => x.SearchAsync("autism", It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Courses.Should().HaveCount(1);
        result.Courses.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());

        _courseRepositoryMock.Verify(x => x.SearchAsync("autism", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetCoursesQuery(ActiveOnly: true);

        _courseRepositoryMock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Course>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Courses retrieved successfully");
        result.Courses.Should().BeEmpty();

        _courseRepositoryMock.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetCoursesQuery(ActiveOnly: true);

        _courseRepositoryMock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving courses");
        result.Courses.Should().BeEmpty();
        result.Errors.Should().ContainKey("Error");
        result.Errors!["Error"].Should().Contain("Database error");
    }
}