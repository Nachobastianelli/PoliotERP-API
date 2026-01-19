namespace Domain.Exceptions;

public class DuplicateEntityException : DomainException
{
    public string EntityName { get; }
    public string Field { get; }
    public string Value  { get; }


    public DuplicateEntityException(string entityName, string field, string value) : base(
        $"{entityName} with {field} '{value}' already exists")
    {
        EntityName = entityName;
        Field = field;
        Value = value;
    }
}