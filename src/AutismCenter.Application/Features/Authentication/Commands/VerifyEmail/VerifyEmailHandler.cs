using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.VerifyEmail;

public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailHandler(
        IUserRepository userRepository,
        IEmailVerificationService emailVerificationService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationService = emailVerificationService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyEmailResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new VerifyEmailResponse(false, "Invalid verification token");
        }

        // Validate token
        var isValidToken = await _emailVerificationService.ValidateVerificationTokenAsync(request.Token);
        if (!isValidToken)
        {
            return new VerifyEmailResponse(false, "Invalid or expired verification token");
        }

        // Get user ID from token
        var userId = await _emailVerificationService.GetUserIdFromTokenAsync(request.Token);
        if (!userId.HasValue)
        {
            return new VerifyEmailResponse(false, "Invalid verification token");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return new VerifyEmailResponse(false, "User not found");
        }

        // Verify email
        user.VerifyEmail();

        // Update user
        await _userRepository.UpdateAsync(user);

        // Revoke verification token
        await _emailVerificationService.RevokeTokenAsync(request.Token);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send welcome email
        var language = user.PreferredLanguage == Domain.Enums.Language.Arabic ? "ar" : "en";
        await _emailService.SendWelcomeEmailAsync(user.Email.Value, user.FirstName, language);

        return new VerifyEmailResponse(true, "Email verified successfully");
    }
}