namespace Eclipse.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityType) : base($"{entityType} not found")
    {
        EntityType = entityType;
    }

    public string EntityType { get; }
}