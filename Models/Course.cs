using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Models;

public class Course : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int Credits { get; set; }

    public int TeacherId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Teacher? Teacher { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
