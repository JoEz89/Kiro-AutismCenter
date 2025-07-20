using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return new ChangePasswordResponse(false, "User not found");
        }

        // Check if user has a password (not Google-only account)
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return new ChangePasswordResponse(false, "Cannot change password for Google-authenticated accounts");
        }

        // Verify current password
        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return new ChangePasswordResponse(false, "Current password is incorrect");
        }

        // Validate new password
        if (!_passwordService.IsValidPassword(request.NewPassword))
        {
            return new ChangePasswordResponse(false, "New password does not meet complexity requirements");
        }

        // Hash and set new password
        var hashedNewPassword = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(hashedNewPassword);

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResponse(true, "Password changed successfully");
    }
}