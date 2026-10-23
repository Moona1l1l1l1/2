using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;

namespace UniversityApi.Data;

/// <summary>Тестовые данные. Выполняется только если таблица преподавателей пуста.</summary>
public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Teachers.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var teachers = new List<Teacher>
        {
            new() { FirstName = "Elena", LastName = "Gertsen", Email = "e.gertsen@satbayev.university", Department = "Software Engineering" },
            new() { FirstName = "Farida", LastName = "Abdoldina", Email = "f.abdoldina@satbayev.university", Department = "Software Engineering" },
            new() { FirstName = "Marat", LastName = "Nurlanov", Email = "m.nurlanov@satbayev.university", Department = "Mathematics" }
        };
        db.Teachers.AddRange(teachers);
        await db.SaveChangesAsync();

        var students = new List<Student>
        {
            new() { FirstName = "Alex", LastName = "Smith", Email = "alex.smith@student.kz", BirthDate = new DateOnly(2004, 3, 14), CreatedAt = now },
            new() { FirstName = "Aigerim", LastName = "Bekova", Email = "aigerim.bekova@student.kz", BirthDate = new DateOnly(2003, 7, 2), CreatedAt = now },
            new() { FirstName = "Daniyar", LastName = "Sarsenov", Email = "daniyar.sarsenov@student.kz", BirthDate = new DateOnly(2004, 11, 21), CreatedAt = now },
            new() { FirstName = "Madina", LastName = "Omarova", Email = "madina.omarova@student.kz", BirthDate = new DateOnly(2005, 1, 9), CreatedAt = now },
            new() { FirstName = "Timur", LastName = "Kassymov", Email = "timur.kassymov@student.kz", BirthDate = new DateOnly(2003, 5, 30), CreatedAt = now }
        };
        db.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Name = "Web Services Development", Description = "REST API on ASP.NET Core", Credits = 5, TeacherId = teachers[0].Id, CreatedAt = now },
            new() { Name = "Databases", Description = "Relational databases and SQL", Credits = 4, TeacherId = teachers[1].Id, CreatedAt = now },
            new() { Name = "Software Architecture", Description = "Layers, patterns, DI", Credits = 4, TeacherId = teachers[0].Id, CreatedAt = now },
            new() { Name = "Discrete Mathematics", Description = "Sets, graphs, logic", Credits = 3, TeacherId = teachers[2].Id, CreatedAt = now }
        };
        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();

        db.Enrollments.AddRange(
            new Enrollment { StudentId = students[0].Id, CourseId = courses[0].Id, EnrollmentDate = now, Grade = 92.5m },
            new Enrollment { StudentId = students[0].Id, CourseId = courses[1].Id, EnrollmentDate = now, Grade = 85m },
            new Enrollment { StudentId = students[1].Id, CourseId = courses[0].Id, EnrollmentDate = now, Grade = 78m },
            new Enrollment { StudentId = students[1].Id, CourseId = courses[3].Id, EnrollmentDate = now },
            new Enrollment { StudentId = students[2].Id, CourseId = courses[2].Id, EnrollmentDate = now, Grade = 88m },
            new Enrollment { StudentId = students[3].Id, CourseId = courses[1].Id, EnrollmentDate = now },
            new Enrollment { StudentId = students[4].Id, CourseId = courses[0].Id, EnrollmentDate = now, Grade = 95m });
        await db.SaveChangesAsync();
    }
}
