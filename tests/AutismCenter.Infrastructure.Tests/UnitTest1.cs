using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Infrastructure.Data;
using AutismCenter.Infrastructure.Repositories;

namespace AutismCenter.Infrastructure.Tests;

public class RepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _userRepository;

    public RepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task UserRepository_AddAndRetrieve_ShouldWork()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", UserRole.Patient, Language.English);

        // Act
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();

        var retrievedUser = await _userRepository.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal("John", retrievedUser.FirstName);
        Assert.Equal("Doe", retrievedUser.LastName);
        Assert.Equal(email.Value, retrievedUser.Email.Value);
        Assert.Equal(UserRole.Patient, retrievedUser.Role);
    }

    [Fact]
    public async Task UserRepository_EmailExists_ShouldReturnCorrectResult()
    {
        // Arrange
        var email = Email.Create("exists@example.com");
        var user = User.Create(email, "Jane", "Smith", UserRole.Admin);

        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        var exists = await _userRepository.EmailExistsAsync(email);
        Assert.True(exists);

        var nonExistentEmail = Email.Create("notexists@example.com");
        var notExists = await _userRepository.EmailExistsAsync(nonExistentEmail);
        Assert.False(notExists);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
