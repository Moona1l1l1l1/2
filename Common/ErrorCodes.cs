namespace UniversityApi.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string Conflict = "CONFLICT";
    public const string InvalidFilter = "INVALID_FILTER";

    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string StudentEmailExists = "STUDENT_EMAIL_ALREADY_EXISTS";

    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string TeacherEmailExists = "TEACHER_EMAIL_ALREADY_EXISTS";
    public const string TeacherHasCourses = "TEACHER_HAS_COURSES";

    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string CourseHasEnrollments = "COURSE_HAS_ENROLLMENTS";

    public const string EnrollmentNotFound = "ENROLLMENT_NOT_FOUND";
    public const string EnrollmentAlreadyExists = "ENROLLMENT_ALREADY_EXISTS";

    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string UserNameExists = "USERNAME_ALREADY_EXISTS";
    public const string RegistrationFailed = "REGISTRATION_FAILED";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string RoleInvalid = "ROLE_INVALID";
    public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";
}
