namespace CB.Reactive;

using global::System;
using global::System.Collections.Concurrent;
using global::System.Reactive.Linq;
using global::System.Reactive.Subjects;



/// <summary>
///   Represents a demultiplexing mechanism for splitting an observable stream
///   into multiple separate streams based on a specified key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify each stream.</typeparam>
/// <typeparam name="T">The type of the elements in the streams.</typeparam>
public interface IDemux<TKey, T> : IDisposable {
  #region fields and properties

  /// <summary>
  ///   Gets the stream of items that had no specific listeners registered.
  /// </summary>
  public IObservable<T> Fallback {
    get;
  }

  #endregion

  #region methods and others

  /// <summary>
  ///   Gets a stream for a specific key.
  /// </summary>
  public IObservable<T> GetStream(TKey key);

  #endregion
}



/// <summary>
///   Represents a demultiplexing mechanism for splitting an observable stream
///   of a base type into strongly-typed streams for specific subclasses or implementations.
/// </summary>
/// <typeparam name="TBase">The base type of elements in the stream.</typeparam>
public interface ITypeDemux<TBase> : IDisposable {
  #region fields and properties

  /// <summary>
  ///   Gets the stream of items that had no specific listeners registered.
  /// </summary>
  public IObservable<TBase> Fallback {
    get;
  }

  #endregion

  #region methods and others

  /// <summary>
  ///   Gets a strongly-typed stream for a specific subclass/implementation.
  /// </summary>
  public IObservable<TTarget> GetStream<TTarget>()
    where TTarget : TBase;

  #endregion
}



public static partial class ObservableX {
  #region nested types

  /// <param name="source">
  ///   The source observable sequence to be demultiplexed.
  /// </param>
  /// <typeparam name="T">
  ///   The type of the elements in the source sequence.
  /// </typeparam>
  extension<T>(IObservable<T> source) {
    #region methods and others

    /// <summary>
    ///   Demultiplexes an observable sequence into multiple streams based on a specified key.
    /// </summary>
    /// <param name="keySelector">
    ///   A function to extract the key from each element in the source sequence.
    /// </param>
    /// <typeparam name="TKey">
    ///   The type of the key used to identify separate streams. This type must be non-nullable.
    /// </typeparam>
    /// <returns>
    ///   An instance of <see cref="IDemux{TKey, T}" /> that provides access to streams associated with specific keys
    ///   and a fallback stream for elements that are not routed to any specific key.
    /// </returns>
    public IDemux<TKey, T> Demultiplex<TKey>(Func<T, TKey> keySelector)
      where TKey : notnull =>
      new Demuxer<T, TKey>(source, keySelector);



    /// <summary>
    ///   Splits an observable stream of a base type into strongly-typed streams
    ///   for specific subclasses or implementations.
    /// </summary>
    /// <typeparam name="TBase">
    ///   The base type of elements in the source sequence.
    /// </typeparam>
    /// <returns>
    ///   An instance of <see cref="ITypeDemux{TBase}" /> that provides access to streams
    ///   for specific types derived from <typeparamref name="TBase" /> and a fallback stream
    ///   for elements that do not match any specific type.
    /// </returns>
    public ITypeDemux<T> Demultiplex() =>
      new TypeDemuxer<T>(source);

    #endregion
  }



  private sealed class TypeDemuxer<TBase> : ITypeDemux<TBase> {
    #region fields and properties

    private readonly Subject<TBase> _fallback = new();
    private readonly ConcurrentDictionary<Type, object> _streams = new();
    private readonly IDisposable _subscription;

    public IObservable<TBase> Fallback => _fallback.AsObservable();

    #endregion

    #region constructors

    public TypeDemuxer(IObservable<TBase> source) =>
      _subscription = source.Subscribe(
        item => {
          if (item == null) {
            return;
          }

          var type = item.GetType();
          var handled = false;

          // Check for exact type match first (Optimization)
          if (_streams.TryGetValue(type, out var exactSubject)) {
            ((ISubject<TBase>)exactSubject).OnNext(item);
            handled = true;
          }

          // Also need to check if we have subscribers for base classes or interfaces?
          // Usually Type Demux implies exact match or "OfType" logic.
          // Standard Demuxer logic is usually EXACT key match.
          // However, Stream<ISomeInterface>() should probably get items implementing it.

          // COMPLEXITY NOTE:
          // If we support inheritance (polymorphic routing), a single item might go to MULTIPLE streams.
          // If we strictly follow the previous pattern "Select Key -> Route", it implies 1-to-1.
          // For types, 1-to-1 means "Exact Type".

          // IF you want polymorphic routing (item goes to all matching streams):
          // We would iterate _streams. But that's slow.

          // IF you want simple Demux (exact type match):
          if (!handled) {
            if (_fallback.HasObservers) {
              _fallback.OnNext(item);
            }
          }
        },
        ex => {
          _fallback.OnError(ex);
          foreach (var s in _streams.Values) {
            ((ISubject<TBase>)s).OnError(ex);
          }
        },
        () => {
          _fallback.OnCompleted();
          foreach (var s in _streams.Values) {
            ((ISubject<TBase>)s).OnCompleted();
          }
        }
      );

    #endregion

    #region methods and others

    public IObservable<TTarget> GetStream<TTarget>()
      where TTarget : TBase {
      // We store Subject<TBase> but cast to Subject<TTarget> for the return?
      // No, Subject<TBase> cannot be cast to Subject<TTarget> safely if we want to push TBase into it.
      // Actually, we need to store Subject<TBase> to allow the Demuxer to push 'item' (TBase) into it.
      // But we expose it as IObservable<TTarget>.
      // This requires filtering/casting at the output.

      var type = typeof(TTarget);

      var subject = (Subject<TBase>)_streams.GetOrAdd(type, _ => new Subject<TBase>());

      // Return safe cast stream. The demuxer drops nulls and routes strictly by item.GetType(), so
      // everything on this subject really is a non-null TTarget.
      return subject.Select(x => (TTarget)x!);
    }



    public void Dispose() {
      _subscription.Dispose();
      _fallback.Dispose();
      foreach (var s in _streams.Values) {
        ((IDisposable)s).Dispose();
      }

      _streams.Clear();
    }

    #endregion
  }



  private sealed class Demuxer<T, TKey> : IDemux<TKey, T>
    where TKey : notnull {
    #region fields and properties

    private readonly Subject<T> _fallback = new();
    private readonly ConcurrentDictionary<TKey, Subject<T>> _streams = new();
    private readonly IDisposable _subscription;

    public IObservable<T> Fallback => _fallback.AsObservable();

    #endregion

    #region constructors

    public Demuxer(IObservable<T> source, Func<T, TKey> keySelector) =>
      _subscription = source.Subscribe(
        item => {
          try {
            var key = keySelector(item);

            if (_streams.TryGetValue(key, out var subject) && subject.HasObservers) {
              subject.OnNext(item);
            } else {
              if (_fallback.HasObservers) {
                _fallback.OnNext(item);
              }
            }
          } catch (Exception ex) {
            _fallback.OnError(ex);
            foreach (var s in _streams.Values) {
              s.OnError(ex);
            }
          }
        },
        ex => {
          _fallback.OnError(ex);
          foreach (var s in _streams.Values) {
            s.OnError(ex);
          }
        },
        () => {
          _fallback.OnCompleted();
          foreach (var s in _streams.Values) {
            s.OnCompleted();
          }
        }
      );

    #endregion

    #region methods and others

    public IObservable<T> GetStream(TKey key) =>
      _streams.GetOrAdd(key, _ => new()).AsObservable();



    public void Dispose() {
      _subscription.Dispose();
      _fallback.Dispose();
      foreach (var s in _streams.Values) {
        s.Dispose();
      }

      _streams.Clear();
    }

    #endregion
  }

  #endregion
}
