namespace DPBloom.Core.Base;

public interface IEntityBase<TId>
{
    public TId Id { get; }
    public DateTime CreatedOn { get; }
    public DateTime UpdatedOn { get; }
}