using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Parse phone number if provided
        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        }

        // Update profile
        user.UpdateProfile(request.FirstName, request.LastName, phoneNumber);

        // Update preferred language if provided
        if (!string.IsNullOrEmpty(request.PreferredLanguage))
        {
            var language = request.PreferredLanguage.ToLower() == "ar" ? Language.Arabic : Language.English;
            user.ChangePreferredLanguage(language);
        }

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.PhoneNumber?.Value,
            user.PreferredLanguage == Language.Arabic ? "ar" : "en",
            user.UpdatedAt,
            "User updated successfully"
        );
    }
}