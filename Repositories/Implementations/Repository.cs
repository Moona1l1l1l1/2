using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // Сущность, полученная через GetByIdAsync, уже отслеживается — достаточно SaveChanges
        if (Context.Entry(entity).State == EntityState.Detached)
            Set.Update(entity);

        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        Set.Remove(entity);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await Set.AnyAsync(e => e.Id == id, ct);
}
