namespace DPBloom.Application.Base;

public class ModelBase<TId> : IModelBase<TId>
{
    public TId Id { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}