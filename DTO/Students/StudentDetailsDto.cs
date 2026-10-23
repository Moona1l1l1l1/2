namespace UniversityApi.DTO.Students;

/// <summary>Студент вместе с его курсами.</summary>
public class StudentDetailsDto : StudentDto
{
    public List<StudentCourseDto> Courses { get; set; } = new();
}

public class StudentCourseDto
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int CourseCredits { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public decimal? Grade { get; set; }
}
