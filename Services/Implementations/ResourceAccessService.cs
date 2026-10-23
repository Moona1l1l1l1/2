using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class ResourceAccessService : IResourceAccessService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _user;

    public ResourceAccessService(ApplicationDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<bool> CanViewStudentAsync(int studentId, CancellationToken ct = default)
    {
        if (_user.IsAdmin)
            return true;

        if (_user.IsTeacher && _user.TeacherId is int teacherId)
        {
            return await _db.Enrollments.AnyAsync(
                e => e.StudentId == studentId && e.Course!.TeacherId == teacherId, ct);
        }

        return _user.IsStudent && _user.StudentId == studentId;
    }

    public async Task<bool> CanManageCourseGradesAsync(int courseId, CancellationToken ct = default)
    {
        if (_user.IsAdmin)
            return true;

        if (_user.IsTeacher && _user.TeacherId is int teacherId)
            return await _db.Courses.AnyAsync(c => c.Id == courseId && c.TeacherId == teacherId, ct);

        return false;
    }

    public async Task<bool> CanViewEnrollmentAsync(int studentId, int courseId, CancellationToken ct = default)
    {
        if (_user.IsAdmin)
            return true;

        if (_user.IsTeacher && await CanManageCourseGradesAsync(courseId, ct))
            return true;

        return _user.IsStudent && _user.StudentId == studentId;
    }

    public async Task<List<int>> GetOwnedCourseIdsAsync(CancellationToken ct = default)
    {
        if (_user.TeacherId is not int teacherId)
            return new List<int>();

        return await _db.Courses.Where(c => c.TeacherId == teacherId).Select(c => c.Id).ToListAsync(ct);
    }
}
