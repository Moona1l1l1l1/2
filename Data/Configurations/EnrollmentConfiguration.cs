using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityApi.Models;

namespace UniversityApi.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        // Один студент — одна запись на конкретный курс (защита на уровне БД)
        builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();

        builder.Property(e => e.Grade).HasPrecision(5, 2);
    }
}
