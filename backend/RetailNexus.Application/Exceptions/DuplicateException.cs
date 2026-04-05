namespace RetailNexus.Application.Exceptions;

public class DuplicateException : Exception
{
    public string FieldName { get; }

    public DuplicateException(string fieldName, string message)
        : base(message)
    {
        FieldName = fieldName;
    }
}
