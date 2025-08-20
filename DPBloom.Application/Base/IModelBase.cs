namespace DPBloom.Application.Base;

public interface IModelBase<TId>
{
    public TId Id { get; set; }
    public DateTime CreatedOn { get; }
}