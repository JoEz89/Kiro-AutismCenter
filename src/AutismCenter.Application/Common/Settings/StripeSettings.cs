namespace AutismCenter.Application.Common.Settings;

public class StripeSettings
{
    public const string SectionName = "Stripe";
    
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "bhd";
    public bool CaptureMethod { get; set; } = true;
}