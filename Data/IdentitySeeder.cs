using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.Models;

namespace UniversityApi.Data;

/// <summary>
/// Создаёт роли и тестовых пользователей. Тестовые пользователи создаются только если в конфигурации задан
/// Seed:Password (в репозитории он есть только в appsettings.Development.json).
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var password = config["Seed:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("Seed:Password is not set - test users are not created");
            return;
        }

        var teachers = await db.Teachers.AsNoTracking().ToListAsync();
        var students = await db.Students.AsNoTracking().ToListAsync();

        int? TeacherIdByEmail(string email) => teachers.FirstOrDefault(t => t.Email == email)?.Id;
        int? StudentIdByEmail(string email) => students.FirstOrDefault(s => s.Email == email)?.Id;

        await EnsureUserAsync(userManager, logger, "admin", "admin@university.kz", password, Roles.Admin, null, null);
        await EnsureUserAsync(userManager, logger, "teacher.gertsen", "e.gertsen@satbayev.university", password,
            Roles.Teacher, null, TeacherIdByEmail("e.gertsen@satbayev.university"));
        await EnsureUserAsync(userManager, logger, "teacher.abdoldina", "f.abdoldina@satbayev.university", password,
            Roles.Teacher, null, TeacherIdByEmail("f.abdoldina@satbayev.university"));
        await EnsureUserAsync(userManager, logger, "student.alex", "alex.smith@student.kz", password,
            Roles.Student, StudentIdByEmail("alex.smith@student.kz"), null);
        await EnsureUserAsync(userManager, logger, "student.aigerim", "aigerim.bekova@student.kz", password,
            Roles.Student, StudentIdByEmail("aigerim.bekova@student.kz"), null);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> users, ILogger logger, string userName,
        string email, string password, string role, int? studentId, int? teacherId)
    {
        if (await users.FindByNameAsync(userName) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            StudentId = studentId,
            TeacherId = teacherId
        };

        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Seed user {UserName} was not created: {Errors}", userName,
                string.Join("; ", result.Errors.Select(e => e.Code)));
            return;
        }

        await users.AddToRoleAsync(user, role);
        logger.LogInformation("Seed user {UserName} created with role {Role}", userName, role);
    }
}
