namespace Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("You are not authorized to perform this action")
    {
        
    }

    public ForbiddenException(string message) : base(message){}
    
    public ForbiddenException(string resource, string action) : base($"you aren't authorized to perform this action {resource} in {action} "){}
}