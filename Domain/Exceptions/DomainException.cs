namespace Domain.Exceptions;

public class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }   
    protected DomainException(string message, Exception inner) : base(message, inner)
    {
    }
}