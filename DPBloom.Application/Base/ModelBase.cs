namespace DPBloom.Application.Base;

public class ModelBase<TId> : IModelBase<Guid>
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
}