using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Courses;

public class CourseCreateDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Количество кредитов: от 1 до 10.</summary>
    [Range(1, 10)]
    public int Credits { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "TeacherId is required.")]
    public int TeacherId { get; set; }
}
