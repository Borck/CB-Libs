using System;
using System.Collections;
using System.Collections.Generic;



namespace CB.System.Collections {
  /// <summary>
  ///   This is a dictionary guaranteed to have only one of each value and key.
  ///   It may be searched either by TFirst or by TSecond, giving a unique answer because it is 1 to 1.
  /// </summary>
  /// <typeparam name="TFirst">The type of the "key"</typeparam>
  /// <typeparam name="TSecond">The type of the "value"</typeparam>
  public class BiDictionaryOneToOne<TFirst, TSecond>
    : IDictionary<TFirst, TSecond>,
      ICollection,
      IReadOnlyDictionary<TFirst, TSecond> {
    #region fields

    private readonly Dictionary<TFirst, TSecond> _firstToSecond;
    private readonly Dictionary<TSecond, TFirst> _secondToFirst;

    #endregion



    #region properties

    public ICollection<TFirst> Keys => _firstToSecond.Keys;

    IEnumerable<TSecond> IReadOnlyDictionary<TFirst, TSecond>.Values
      => Values;

    IEnumerable<TFirst> IReadOnlyDictionary<TFirst, TSecond>.Keys
      => Keys;

    public ICollection<TSecond> Values => _firstToSecond.Values;



    /// <summary>
    ///   The number of pairs stored in the dictionary
    /// </summary>
    public int Count => _firstToSecond.Count;


    public bool IsSynchronized => false;
    public object SyncRoot { get; } = new object();

    public bool IsReadOnly => false;

    #endregion


    #region constructors

    public BiDictionaryOneToOne() {
      _firstToSecond = new Dictionary<TFirst, TSecond>();
      _secondToFirst = new Dictionary<TSecond, TFirst>();
    }



    public BiDictionaryOneToOne(int capacity) {
      _firstToSecond = new Dictionary<TFirst, TSecond>(capacity);
      _secondToFirst = new Dictionary<TSecond, TFirst>(capacity);
    }



    public BiDictionaryOneToOne(IDictionary<TFirst, TSecond> dict) {
      _firstToSecond = new Dictionary<TFirst, TSecond>(dict);
      _secondToFirst = _firstToSecond.Swap();
    }



    public BiDictionaryOneToOne(
      IDictionary<TFirst, TSecond> dict,
      IEqualityComparer<TFirst> comparerFirst,
      IEqualityComparer<TSecond> comparerSecond) {
      _secondToFirst = dict.Swap(comparerSecond);
      _firstToSecond = new Dictionary<TFirst, TSecond>(dict, comparerFirst);
    }

    #endregion


    #region operators

    public TSecond this[TFirst key] {
      get => _firstToSecond[key];
      set => Add(key, value);
    }

    #endregion

    #region methods

    #region methods: add

    /// <summary>
    ///   Tries to add the pair to the dictionary.
    ///   Throws an exception if either element is already in the dictionary
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public void Add(TFirst first, TSecond second) {
      if (_firstToSecond.ContainsKey(first) ||
          _secondToFirst.ContainsKey(second)) {
        throw new ArgumentException("Duplicate first or second");
      }

      _firstToSecond.Add(first, second);
      _secondToFirst.Add(second, first);
    }



    public void Add(KeyValuePair<TFirst, TSecond> item) {
      Add(item.Key, item.Value);
    }



    /// <summary>
    ///   Tries to add the pair to the dictionary.
    ///   Returns false if either element is already in the dictionary
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
    public bool TryAdd(TFirst first, TSecond second) {
      if (_firstToSecond.ContainsKey(first) ||
          _secondToFirst.ContainsKey(second)) {
        return false;
      }

      _firstToSecond.Add(first, second);
      _secondToFirst.Add(second, first);
      return true;
    }

    #endregion

    #region methods: get

    /// <summary>
    ///   Find the TSecond corresponding to the TFirst first
    ///   Throws an exception if first is not in the dictionary.
    /// </summary>
    /// <param name="first">the key to search for</param>
    /// <returns>the value corresponding to first</returns>
    public TSecond Get(TFirst first) {
      if (!_firstToSecond.TryGetValue(first, out var second)) {
        throw new ArgumentException(nameof(first));
      }

      return second;
    }



    /// <summary>
    ///   Find the TFirst corresponing to the Second second.
    ///   Throws an exception if second is not in the dictionary.
    /// </summary>
    /// <param name="second">the key to search for</param>
    /// <returns>the value corresponding to second</returns>
    public TFirst GetFirst(TSecond second) {
      if (!_secondToFirst.TryGetValue(second, out var first)) {
        throw new ArgumentException(nameof(second));
      }

      return first;
    }



    /// <summary>
    ///   Find the TSecond corresponding to the TFirst first.
    ///   Returns false if first is not in the dictionary.
    /// </summary>
    /// <param name="first">the key to search for</param>
    /// <param name="second">the corresponding value</param>
    /// <returns>true if first is in the dictionary, false otherwise</returns>
    public bool TryGetValue(TFirst first, out TSecond second) {
      return _firstToSecond.TryGetValue(first, out second);
    }



    /// <summary>
    ///   Find the TFirst corresponding to the TSecond second.
    ///   Returns false if second is not in the dictionary.
    /// </summary>
    /// <param name="second">the key to search for</param>
    /// <param name="first">the corresponding value</param>
    /// <returns>true if second is in the dictionary, false otherwise</returns>
    public bool TryGetFirst(TSecond second, out TFirst first) {
      return _secondToFirst.TryGetValue(second, out first);
    }

    #endregion

    #region remove

    public bool Remove(KeyValuePair<TFirst, TSecond> item) {
      return _secondToFirst.ContainsKey(item.Value) &&
             _firstToSecond.Remove(item.Key) &&
             _secondToFirst.Remove(item.Value);
    }



    /// <summary>
    ///   Remove the record containing first.
    ///   If first is not in the dictionary, throws an Exception.
    /// </summary>
    /// <param name="first">the key of the record to delete</param>
    public bool Remove(TFirst first) {
      if (!_firstToSecond.TryGetValue(first, out var second)) {
        return false;
      }

      _firstToSecond.Remove(first);
      _secondToFirst.Remove(second);
      return true;
    }



    /// <summary>
    ///   Remove the record containing second.
    ///   If second is not in the dictionary, throws an Exception.
    /// </summary>
    /// <param name="second">the key of the record to delete</param>
    public bool RemoveBySecond(TSecond second) {
      return _secondToFirst.TryGetValue(second, out var first) &&
             _firstToSecond.Remove(first) &&
             _secondToFirst.Remove(second);
    }

    #endregion

    #region methods: contains

    public bool Contains(KeyValuePair<TFirst, TSecond> item) {
      return _firstToSecond.ContainsKey(item.Key) &&
             _firstToSecond.ContainsValue(item.Value);
    }



    public bool ContainsKey(TFirst first) {
      return _firstToSecond.ContainsKey(first);
    }



    public bool ContainsKeySecond(TSecond second) {
      return _secondToFirst.ContainsKey(second);
    }

    #endregion

    #region methods: Clear

    /// <summary>
    ///   Removes all items from the dictionary.
    /// </summary>
    public void Clear() {
      _firstToSecond.Clear();
      _secondToFirst.Clear();
    }

    #endregion

    #region methods: CopyTo

    public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex) {
      (_firstToSecond as IDictionary<TFirst, TSecond>).CopyTo(array, arrayIndex);
    }



    public void CopyTo(Array array, int index) {
      (_firstToSecond as ICollection).CopyTo(array, index);
    }

    #endregion

    #region methods: GetEnumerator

    public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator() {
      return _firstToSecond.GetEnumerator();
    }



    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    #endregion


    #region methods: Equals, GetHashCode

    public override bool Equals(object obj) {
      return GetType().IsInstanceOfType(obj) &&
             _firstToSecond.Equals((obj as BiDictionaryOneToOne<TFirst, TSecond>)?._firstToSecond);
    }



    public override int GetHashCode() {
      return _firstToSecond.GetHashCode();
    }

    #endregion

    #region methods: ToString

    public override string ToString() {
      return new ToStringBuilder<BiDictionaryOneToOne<TFirst, TSecond>>(this)
             .Append(dict => dict._firstToSecond.ToString())
             .ToString();
    }

    #endregion

    #endregion
  }
}
