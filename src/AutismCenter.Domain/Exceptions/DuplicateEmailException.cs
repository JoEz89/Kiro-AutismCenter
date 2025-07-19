namespace AutismCenter.Domain.Exceptions;

public class DuplicateEmailException : DomainException
{
    public string Email { get; }

    public DuplicateEmailException(string email) 
        : base($"Email '{email}' is already in use.")
    {
        Email = email;
    }
}