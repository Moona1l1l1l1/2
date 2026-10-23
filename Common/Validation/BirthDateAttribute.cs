using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Common.Validation;

/// <summary>Дата рождения: не раньше 1900-01-01 и не в будущем.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BirthDateAttribute : ValidationAttribute
{
    private static readonly DateOnly MinDate = new(1900, 1, 1);

    public BirthDateAttribute() : base("BirthDate must be between 1900-01-01 and today.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateOnly date)
            return false;

        return date >= MinDate && date <= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
