using AutismCenter.Application.Common.Interfaces;

namespace AutismCenter.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    // For now, this is a simple implementation that returns default values
    // In a real application, this would be injected with the current user context
    // from the HTTP context or authentication system
    
    public Guid? UserId => Guid.Parse("00000000-0000-0000-0000-000000000001"); // Default admin user for testing
    
    public string? Email => "admin@autismcenter.com";
    
    public string? Role => "Admin";
    
    public bool IsAuthenticated => true;
}