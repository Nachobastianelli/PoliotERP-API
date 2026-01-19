namespace Domain.Exceptions;

public class BusinessRuleValidationException : DomainException
{
    public string Rule { get; }

    public BusinessRuleValidationException(string rule, string message) : base(message)
    {
        Rule = rule;
    }

    public BusinessRuleValidationException(string message) : base(message)
    {
        Rule = string.Empty;
    }
}