using System.ComponentModel.DataAnnotations;
using UniversityApi.Common.Validation;

namespace UniversityApi.DTO.Students;

public class StudentUpdateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [BirthDate]
    public DateOnly BirthDate { get; set; }
}
