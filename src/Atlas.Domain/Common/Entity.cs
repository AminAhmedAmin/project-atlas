namespace Atlas.Domain.Common;

/// <summary>Base type for entities identified by a sortable GUID (v7).</summary>
public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
}
