namespace DPBloom.Infrastructure.Base;

public interface IEntityDaoBase<TId>
{
    public TId Id { get; }
    public DateTime CreatedOn { get; }
    public DateTime UpdatedOn { get; }
}