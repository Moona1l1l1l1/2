using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Models;

public class Teacher : BaseEntity
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Department { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
