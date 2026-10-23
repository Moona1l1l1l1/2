using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Enrollments;

public class EnrollmentUpdateGradeDto
{
    /// <summary>Оценка от 0 до 100.</summary>
    [Required, Range(0, 100)]
    public decimal? Grade { get; set; }
}
