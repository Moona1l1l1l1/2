using Microsoft.AspNetCore.Identity;

namespace UniversityApi.Models;

/// <summary>Учётная запись Identity. Пароль хранится только как хеш (PasswordHash) средствами Identity.</summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Связь с сущностью Student (для роли Student).</summary>
    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    /// <summary>Связь с сущностью Teacher (для роли Teacher).</summary>
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
