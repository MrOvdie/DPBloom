namespace DPBloom.Infrastructure.Base;

public class EntityDaoBase<TId> : IEntityDaoBase<TId>
{
    public required TId Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}