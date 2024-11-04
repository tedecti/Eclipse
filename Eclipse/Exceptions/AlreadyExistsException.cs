namespace Eclipse.Exceptions;

public class AlreadyExistsException : Exception
{
    public AlreadyExistsException(string entityType)
        : base($"{entityType} already exists")
    {
        EntityType = entityType;
    }

    public string EntityType { get; }
}