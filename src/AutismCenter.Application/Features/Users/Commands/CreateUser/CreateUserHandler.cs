using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Users.Commands.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email address already exists");
        }

        // Parse role and language
        var role = Enum.Parse<UserRole>(request.Role, true);
        var preferredLanguage = request.PreferredLanguage.ToLower() == "ar" ? Language.Arabic : Language.English;

        // Parse phone number if provided
        PhoneNumber? phoneNumber = !string.IsNullOrEmpty(request.PhoneNumber) 
            ? PhoneNumber.Create(request.PhoneNumber) 
            : null;

        // Create user
        var user = User.Create(email, request.FirstName, request.LastName, role, preferredLanguage);

        // Set password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            var hashedPassword = _passwordService.HashPassword(request.Password);
            user.SetPassword(hashedPassword);
        }

        // Update profile with phone number if provided
        if (phoneNumber != null)
        {
            user.UpdateProfile(request.FirstName, request.LastName, phoneNumber);
        }

        // Save user
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.PreferredLanguage == Language.Arabic ? "ar" : "en",
            user.PhoneNumber?.Value,
            user.IsEmailVerified,
            user.CreatedAt,
            "User created successfully"
        );
    }
}