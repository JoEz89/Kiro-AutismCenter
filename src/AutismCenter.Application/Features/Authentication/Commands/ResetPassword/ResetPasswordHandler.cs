using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordHandler(
        IUserRepository userRepository,
        IPasswordResetService passwordResetService,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new ResetPasswordResponse(false, "Invalid reset token");
        }

        // Validate token
        var isValidToken = await _passwordResetService.ValidateResetTokenAsync(request.Token);
        if (!isValidToken)
        {
            return new ResetPasswordResponse(false, "Invalid or expired reset token");
        }

        // Get user ID from token
        var userId = await _passwordResetService.GetUserIdFromTokenAsync(request.Token);
        if (!userId.HasValue)
        {
            return new ResetPasswordResponse(false, "Invalid reset token");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return new ResetPasswordResponse(false, "User not found");
        }

        // Validate password
        if (!_passwordService.IsValidPassword(request.NewPassword))
        {
            return new ResetPasswordResponse(false, "Password does not meet complexity requirements");
        }

        // Hash and set new password
        var hashedPassword = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(hashedPassword);

        // Update user
        await _userRepository.UpdateAsync(user);

        // Revoke reset token
        await _passwordResetService.RevokeTokenAsync(request.Token);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResponse(true, "Password has been reset successfully");
    }
}