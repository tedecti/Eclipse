namespace Eclipse.Exceptions;

public class AlreadyExistsException : Exception
{
    public string EntityType { get; }

    public AlreadyExistsException(string entityType)
        : base($"{entityType} already exists")
    {
        EntityType = entityType;
    }
}