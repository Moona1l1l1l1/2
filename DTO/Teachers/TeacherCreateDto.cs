using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Teachers;

public class TeacherCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string Department { get; set; } = string.Empty;
}
