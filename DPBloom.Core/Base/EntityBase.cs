using System.ComponentModel.DataAnnotations;

namespace DPBloom.Core;

public class EntityBase<TId> : IEntityBase<TId>
{
    public required TId Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}