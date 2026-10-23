namespace UniversityApi.DTO.Courses.V2;

/// <summary>Контракт курса версии 2: добавлены сведения о преподавателе и число записанных студентов.</summary>
public class CourseV2Dto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
    public CourseTeacherV2Dto? Teacher { get; set; }
    public int EnrolledStudentsCount { get; set; }
}

public class CourseTeacherV2Dto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
