namespace UniversityApi.Common;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static readonly string[] All = { Admin, Teacher, Student };
}

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string TeacherOrAdmin = "TeacherOrAdmin";
}

/// <summary>Имена claims в JWT (MapInboundClaims отключён, поэтому имена остаются «как в токене»).</summary>
public static class AppClaims
{
    public const string Sub = "sub";
    public const string Name = "unique_name";
    public const string Role = "role";
    public const string Jti = "jti";
    public const string StudentId = "student_id";
    public const string TeacherId = "teacher_id";
}
