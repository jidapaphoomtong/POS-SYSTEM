using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object? _valueToMatch;

    public RequiredIfAttribute(string propertyName, object? valueToMatch = null)
    {
        _propertyName = propertyName;
        _valueToMatch = valueToMatch;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Get the target property
        var property = validationContext.ObjectType.GetProperty(_propertyName);
        if (property == null)
        {
            return new ValidationResult($"Property '{_propertyName}' is not found.");
        }

        // Get the value of the target property
        var propertyValue = property.GetValue(validationContext.ObjectInstance);

        // Check condition: If target property matches the required condition and value is null/empty
        if ((propertyValue == null || propertyValue.Equals(_valueToMatch)) && string.IsNullOrEmpty(value?.ToString()))
        {
            // Validation failed
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
        }

        // Validation succeeded
        return ValidationResult.Success;
    }
}