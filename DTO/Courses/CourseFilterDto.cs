using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Courses;

/// <summary>Параметры фильтрации списка курсов (query string).</summary>
public class CourseFilterDto
{
    /// <summary>Часть названия курса (без учёта регистра).</summary>
    public string? Name { get; set; }

    public int? TeacherId { get; set; }

    [Range(1, 10)]
    public int? MinCredits { get; set; }

    [Range(1, 10)]
    public int? MaxCredits { get; set; }
}
