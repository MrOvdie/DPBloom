namespace DPBloom.Infrastructure.Extensions;

public static class EntityCollectionSyncExtensions
{
    public static void SyncCollection<TChild, TKey>(
        this ICollection<TChild> dbCollection,
        IEnumerable<TChild> incomingCollection,
        Func<TChild, TKey> keySelector,
        Action<TChild, TChild> updateAction,
        Action<TChild>? addAction = null,
        Action<TChild>? onRemove = null,
        Func<TChild, bool>? filterForRemoval = null
    ) where TChild : class
    {
        var incomingDict = incomingCollection.ToDictionary(keySelector);
        var dbItems = dbCollection.ToList();

        // Update existing and mark for removal if missing
        foreach (var dbItem in dbItems)
        {
            var key = keySelector(dbItem);
            if (incomingDict.TryGetValue(key, out var incomingItem))
            {
                updateAction(dbItem, incomingItem);
            }
            else
            {
                if (filterForRemoval == null || filterForRemoval(dbItem))
                {
                    onRemove?.Invoke(dbItem);
                    dbCollection.Remove(dbItem);
                }
            }
        }
        
        foreach (var incomingItem in incomingCollection)
        {
            var key = keySelector(incomingItem);
            if (!dbItems.Any(x => keySelector(x)?.Equals(key) == true))
            {
                addAction?.Invoke(incomingItem);
                dbCollection.Add(incomingItem);
            }
        }
    }
}