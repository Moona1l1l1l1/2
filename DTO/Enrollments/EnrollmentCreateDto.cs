using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Enrollments;

public class EnrollmentCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId is required.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CourseId is required.")]
    public int CourseId { get; set; }
}
