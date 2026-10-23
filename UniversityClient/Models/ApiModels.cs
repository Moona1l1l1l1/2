namespace UniversityClient.Models;

public class ApiEnvelope<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CourseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
    public TeacherViewModel? Teacher { get; set; }
    public int EnrolledStudentsCount { get; set; }
}

public class TeacherViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

/// <summary>Результат вызова API: либо данные, либо понятное пользователю сообщение об ошибке.</summary>
public class ApiResult<T>
{
    public T? Data { get; init; }
    public string? Error { get; init; }
    public bool IsNotFound { get; init; }
    public bool IsSuccess => Error is null;

    public static ApiResult<T> Ok(T data) => new() { Data = data };
    public static ApiResult<T> Fail(string error, bool notFound = false) => new() { Error = error, IsNotFound = notFound };
}
