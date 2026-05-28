using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

/// <summary>La fecha no puede ser anterior al día de hoy.</summary>
public class FechaNoAnteriorAttribute : ValidationAttribute
{
    public FechaNoAnteriorAttribute()
        => ErrorMessage = "La fecha no puede ser anterior a hoy.";

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return value switch
        {
            DateOnly d when d < today => new ValidationResult(ErrorMessage),
            _ => ValidationResult.Success
        };
    }
}

/// <summary>La fecha no puede ser posterior al día de hoy.</summary>
public class FechaNoFuturaAttribute : ValidationAttribute
{
    public FechaNoFuturaAttribute()
        => ErrorMessage = "La fecha no puede ser posterior a hoy.";

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return value switch
        {
            DateOnly d when d > today => new ValidationResult(ErrorMessage),
            _ => ValidationResult.Success
        };
    }
}
