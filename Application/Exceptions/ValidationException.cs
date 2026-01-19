using System.ComponentModel.DataAnnotations;

namespace Application.Exceptions;

public class ValidationException : Exception
{
    public Dictionary<string,string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base(error)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}