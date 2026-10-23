using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityApi.Models;

namespace UniversityApi.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(u => u.Student).WithMany().HasForeignKey(u => u.StudentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(u => u.Teacher).WithMany().HasForeignKey(u => u.TeacherId).OnDelete(DeleteBehavior.SetNull);

        // Один студент / преподаватель — максимум одна учётная запись (NULL-значения индекс не нарушают)
        builder.HasIndex(u => u.StudentId).IsUnique();
        builder.HasIndex(u => u.TeacherId).IsUnique();
    }
}
