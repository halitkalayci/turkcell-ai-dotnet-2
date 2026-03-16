namespace ProductService.Domain.Abstractions;

public abstract class BaseEntity<TId>
{
    public TId Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    protected BaseEntity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity() { }

    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;

    public void SoftDelete()
    {
        IsDeleted = true;
        MarkUpdated();
    }
}
