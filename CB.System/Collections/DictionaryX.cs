using System;
using System.Collections.Generic;
using System.Linq;



namespace CB.System.Collections {
  public static class DictionaryX {
    public static Dictionary<TSecond, TFirst> Swap<TFirst, TSecond>(this IDictionary<TFirst, TSecond> dictionary) {
      return dictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
    }



    public static Dictionary<TSecond, TFirst> Swap<TFirst, TSecond>(
      this IDictionary<TFirst, TSecond> dictionary,
      IEqualityComparer<TSecond> comparer) {
      return dictionary.ToDictionary(pair => pair.Value, pair => pair.Key, comparer);
    }



    public static T GetValueOrDefault<T, TDictKey, TDictValue>(
      this IDictionary<TDictKey, TDictValue> dictionary,
      TDictKey key,
      T defaultValue)
      where T : TDictValue {
      return dictionary.TryGetValue(key, out var value)
               ? (T)value
               : defaultValue;
    }



    public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dict,
                                                        TKey key)
      where TValue : new() {
      if (!dict.TryGetValue(key, out var val)) {
        val = new TValue();
        dict.Add(key, val);
      }

      return val;
    }



    public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dict,
                                                        TKey key,
                                                        Func<TValue> valueCtor) {
      if (!dict.TryGetValue(key, out var val)) {
        val = valueCtor();
        dict.Add(key, val);
      }

      return val;
    }



    public static Dictionary<TKey, TNewValue> CastValues<TKey, TOldValue, TNewValue>(
      this IDictionary<TKey, TOldValue> dictionary) {
      return dictionary.ToDictionary(entry => entry.Key, entry => (TNewValue)(object)entry.Value);
    }



    public static Dictionary<TKey, TNewValue> CastValues<TKey, TOldValue, TNewValue>(
      this IReadOnlyDictionary<TKey, TOldValue> dictionary) {
      return dictionary.ToDictionary(entry => entry.Key, entry => (TNewValue)(object)entry.Value);
    }
  }
}
