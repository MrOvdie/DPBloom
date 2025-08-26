namespace DPBloom.Infrastructure.Extensions;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }

    /*public void Undo();
    {
        IsDeleted = false;
        DeletedOn = null;
    }*/
}