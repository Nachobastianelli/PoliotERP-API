namespace Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, int id) : base($"{entityName} with the id: {id} not found")
    {
        EntityName = entityName;
        EntityId = id;
    }

    public EntityNotFoundException(string entityName, string field, string value) : base(
        $"{entityName} with {field} '{value}' not found")
    {
        EntityName = entityName;
        EntityId = value;
        
    }
}