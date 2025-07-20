using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordHandler(
        IUserRepository userRepository,
        IPasswordResetService passwordResetService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new ForgotPasswordResponse(false, "Email is required");
        }

        try
        {
            // Check if user exists
            var email = Email.Create(request.Email);
            var user = await _userRepository.GetByEmailAsync(email);

            // Always return success to prevent email enumeration attacks
            if (user == null)
            {
                return new ForgotPasswordResponse(true, "If an account with this email exists, a password reset link has been sent.");
            }

            // Generate reset token
            var resetToken = _passwordResetService.GenerateResetToken(user.Id);

            // Save changes (token is stored in the service)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send password reset email
            var language = user.PreferredLanguage == Domain.Enums.Language.Arabic ? "ar" : "en";
            await _emailService.SendPasswordResetEmailAsync(
                user.Email.Value,
                user.FirstName,
                resetToken,
                language);

            return new ForgotPasswordResponse(true, "If an account with this email exists, a password reset link has been sent.");
        }
        catch (ArgumentException)
        {
            return new ForgotPasswordResponse(false, "Invalid email format");
        }
        catch (Exception)
        {
            // Log the exception but don't expose details to the user
            return new ForgotPasswordResponse(false, "An error occurred while processing your request. Please try again later.");
        }
    }
}