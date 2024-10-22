namespace Eclipse.Exceptions;

public class AlreadyExistingException : Exception
{
    public AlreadyExistingException(string message) : base(message)
    {
    }
}