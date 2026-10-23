using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class TeacherRepository : Repository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLower();
        return await Set.AnyAsync(
            t => t.Email.ToLower() == normalized && (excludeId == null || t.Id != excludeId), ct);
    }

    public async Task<bool> HasCoursesAsync(int teacherId, CancellationToken ct = default)
        => await Context.Courses.AnyAsync(c => c.TeacherId == teacherId, ct);
}
