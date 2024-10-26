namespace Eclipse.Exceptions;

public class NotFoundException : Exception
{
    public string EntityType { get; }
    public NotFoundException(string entityType) : base($"{entityType} not found")
    {
        EntityType = entityType;
    }
}