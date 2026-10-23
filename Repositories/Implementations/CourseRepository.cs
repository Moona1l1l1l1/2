using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.DTO.Courses;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken ct = default)
        => await Set.Include(c => c.Teacher).AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public override async Task<Course?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Course>> GetFilteredAsync(CourseFilterDto filter, CancellationToken ct = default)
    {
        IQueryable<Course> query = Set.Include(c => c.Teacher).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(name));
        }

        if (filter.TeacherId.HasValue)
            query = query.Where(c => c.TeacherId == filter.TeacherId.Value);

        if (filter.MinCredits.HasValue)
            query = query.Where(c => c.Credits >= filter.MinCredits.Value);

        if (filter.MaxCredits.HasValue)
            query = query.Where(c => c.Credits <= filter.MaxCredits.Value);

        return await query.OrderBy(c => c.Id).ToListAsync(ct);
    }

    public async Task<Dictionary<int, int>> GetEnrollmentCountsAsync(IReadOnlyCollection<int> courseIds,
        CancellationToken ct = default)
        => await Context.Enrollments
            .Where(e => courseIds.Contains(e.CourseId))
            .GroupBy(e => e.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count, ct);

    public async Task<bool> HasEnrollmentsAsync(int courseId, CancellationToken ct = default)
        => await Context.Enrollments.AnyAsync(e => e.CourseId == courseId, ct);
}
