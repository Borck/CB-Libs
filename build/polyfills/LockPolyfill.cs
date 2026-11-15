namespace System.Threading;

/// <summary>
///   <c>System.Threading.Lock</c> for the targets that predate it (net48, netstandard2.0), over <see cref="Monitor" />. The compiler
///   treats a type of this name specially: `lock (x)` calls <c>EnterScope</c> and disposes the scope it returns. See
///   <see cref="ArgumentNullExceptionPolyfill" />.
/// </summary>
internal sealed class Lock {
  private readonly object _monitor = new();



  public Scope EnterScope() {
    Monitor.Enter(_monitor);
    return new(this);
  }



  public void Enter() => Monitor.Enter(_monitor);



  public bool TryEnter() => Monitor.TryEnter(_monitor);



  public void Exit() => Monitor.Exit(_monitor);



  public readonly ref struct Scope {
    private readonly Lock _lock;

    internal Scope(Lock @lock) => _lock = @lock;



    public void Dispose() => _lock.Exit();
  }
}
