namespace RetailNexus.Application.Exceptions;

public class BusinessRuleException : Exception
{
    public string FieldName { get; }

    public BusinessRuleException(string fieldName, string message)
        : base(message)
    {
        FieldName = fieldName;
    }
}
