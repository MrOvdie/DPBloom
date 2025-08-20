namespace DPBloom.Infrastructure.Extensions;

public static class SoftDeleteExtension
{
    public static void Undo(this ISoftDelete entity)
    {
        entity.IsDeleted = false;
        entity.DeletedOn = null;
    }

    public static void Delete(this ISoftDelete entity)
    {
        entity.IsDeleted = true;
        entity.DeletedOn = DateTime.UtcNow;
    }
}