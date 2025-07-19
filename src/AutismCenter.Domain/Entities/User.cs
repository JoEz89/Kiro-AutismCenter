using AutismCenter.Domain.Common;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class User : BaseEntity
{
    public Email Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public Language PreferredLanguage { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public string? GoogleId { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }

    // Navigation properties
    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    private readonly List<Enrollment> _enrollments = new();
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    private readonly List<Appointment> _appointments = new();
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private User() { } // For EF Core

    private User(Email email, string firstName, string lastName, UserRole role, Language preferredLanguage)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        PreferredLanguage = preferredLanguage;
        IsEmailVerified = false;
    }

    public static User Create(Email email, string firstName, string lastName, UserRole role = UserRole.Patient, Language preferredLanguage = Language.English)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        var user = new User(email, firstName.Trim(), lastName.Trim(), role, preferredLanguage);
        
        // Add domain event for user creation
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        
        return user;
    }

    public static User CreateWithGoogle(Email email, string firstName, string lastName, string googleId, UserRole role = UserRole.Patient, Language preferredLanguage = Language.English)
    {
        var user = Create(email, firstName, lastName, role, preferredLanguage);
        user.GoogleId = googleId;
        user.IsEmailVerified = true; // Google accounts are pre-verified
        
        return user;
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdateTimestamp();
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
            return;

        IsEmailVerified = true;
        UpdateTimestamp();
        
        AddDomainEvent(new UserEmailVerifiedEvent(Id, Email));
    }

    public void ChangeEmail(Email newEmail)
    {
        if (Email == newEmail)
            return;

        var oldEmail = Email;
        Email = newEmail;
        IsEmailVerified = false; // Require re-verification
        UpdateTimestamp();
        
        AddDomainEvent(new UserEmailChangedEvent(Id, oldEmail, newEmail));
    }

    public void UpdateProfile(string firstName, string lastName, PhoneNumber? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber;
        UpdateTimestamp();
        
        AddDomainEvent(new UserProfileUpdatedEvent(Id));
    }

    public void ChangePreferredLanguage(Language language)
    {
        if (PreferredLanguage == language)
            return;

        PreferredLanguage = language;
        UpdateTimestamp();
    }

    public void ChangeRole(UserRole newRole)
    {
        if (Role == newRole)
            return;

        var oldRole = Role;
        Role = newRole;
        UpdateTimestamp();
        
        AddDomainEvent(new UserRoleChangedEvent(Id, oldRole, newRole));
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public bool HasGoogleAccount() => !string.IsNullOrEmpty(GoogleId);

    public bool CanLogin() => IsEmailVerified && (HasGoogleAccount() || !string.IsNullOrEmpty(PasswordHash));
}