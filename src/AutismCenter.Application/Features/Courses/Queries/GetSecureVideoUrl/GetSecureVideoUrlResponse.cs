namespace AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;

public class GetSecureVideoUrlResponse
{
    public string StreamingUrl { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int DaysRemaining { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public int ModuleDuration { get; set; }
}