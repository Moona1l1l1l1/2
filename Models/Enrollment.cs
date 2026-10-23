namespace UniversityApi.Models;

public class Enrollment : BaseEntity
{
    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>Оценка 0–100. Null — оценка ещё не выставлена.</summary>
    public decimal? Grade { get; set; }

    public Student? Student { get; set; }

    public Course? Course { get; set; }
}
