namespace DnDGame.Domain.Common;

/// <summary>
/// Base class for every Domain entity. Provides the one thing every entity needs:
/// a primary key.
///
/// Deliberately minimal — this is plain C#, not an EF Core base class. There are no
/// data annotations here (no [Key], no [Table]) and there won't be any until
/// DataAccessLayer (Phase 6) configures persistence separately, via Fluent API.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
