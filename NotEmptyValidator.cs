public class NotEmptyValidator : IValidator
{
    public bool IsValid(Field field)
    {
        if (field.IsRequired)
        {
            return !string.IsNullOrEmpty(field.Value);
        }

        return true;
    }
}