using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Authentication.Commands.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IEmailVerificationService emailVerificationService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _emailVerificationService = emailVerificationService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var email = Email.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email address already exists");
        }

        // Validate password
        if (!_passwordService.IsValidPassword(request.Password))
        {
            throw new ArgumentException("Password does not meet complexity requirements");
        }

        // Parse preferred language
        var preferredLanguage = request.PreferredLanguage.ToLower() == "ar" ? Language.Arabic : Language.English;

        // Create user
        var user = User.Create(email, request.FirstName, request.LastName, UserRole.Patient, preferredLanguage);
        
        // Hash and set password
        var hashedPassword = _passwordService.HashPassword(request.Password);
        user.SetPassword(hashedPassword);

        // Save user
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate verification token
        var verificationToken = _emailVerificationService.GenerateVerificationToken(user.Id);

        // Send verification email
        await _emailService.SendEmailVerificationAsync(
            user.Email.Value, 
            user.FirstName, 
            verificationToken, 
            request.PreferredLanguage);

        return new RegisterUserResponse(
            user.Id,
            user.Email.Value,
            "Registration successful. Please check your email to verify your account."
        );
    }
}