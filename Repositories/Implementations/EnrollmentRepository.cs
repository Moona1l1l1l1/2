using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IReadOnlyList<Enrollment>> GetAllAsync(CancellationToken ct = default)
        => await GetFilteredAsync(null, null, ct);

    public override async Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Enrollment>> GetFilteredAsync(int? studentId, int? courseId, CancellationToken ct = default)
    {
        IQueryable<Enrollment> query = Set
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsNoTracking();

        if (studentId.HasValue)
            query = query.Where(e => e.StudentId == studentId.Value);

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        return await query.OrderBy(e => e.Id).ToListAsync(ct);
    }

    public async Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct = default)
        => await Set.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId, ct);
}
