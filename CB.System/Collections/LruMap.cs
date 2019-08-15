using System.Collections.Generic;
using System.Runtime.CompilerServices;



namespace CB.System.Collections {
  public class LruMap<TKey, TValue> {
    private readonly Dictionary<TKey, LinkedListNode<LruMapItem<TKey, TValue>>> _cacheMap =
      new Dictionary<TKey, LinkedListNode<LruMapItem<TKey, TValue>>>();

    private readonly int _capacity;
    private readonly LinkedList<LruMapItem<TKey, TValue>> _lruList = new LinkedList<LruMapItem<TKey, TValue>>();



    public LruMap(int capacity) {
      _capacity = capacity;
    }



    [MethodImpl( MethodImplOptions.Synchronized )]
    public bool TryGetValue(TKey key, out TValue value) {
      if (!_cacheMap.TryGetValue( key, out var node )) {
        value = default(TValue);
        return false;
      }

      value = node.Value.Value;
      _lruList.Remove( node );
      _lruList.AddLast( node );
      return true;
    }



    [MethodImpl( MethodImplOptions.Synchronized )]
    public void Add(TKey key, TValue val) {
      if (_cacheMap.Count >= _capacity) {
        // Remove from LRUPriority and cache
        _cacheMap.Remove( _lruList.First.Value.Key );
        _lruList.RemoveFirst();
      }

      var node = new LinkedListNode<LruMapItem<TKey, TValue>>( new LruMapItem<TKey, TValue>( key, val ) );
      _lruList.AddLast( node );
      _cacheMap.Add( key, node );
    }



    internal class LruMapItem<TItemKey, TItemValue> {
      public TItemKey Key;
      public TItemValue Value;



      public LruMapItem(TItemKey k, TItemValue v) {
        Key = k;
        Value = v;
      }
    }
  }
}
