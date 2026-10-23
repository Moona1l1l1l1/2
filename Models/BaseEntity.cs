namespace UniversityApi.Models;

/// <summary>Базовая сущность. Свойство Id становится Primary Key по конвенции EF Core.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
