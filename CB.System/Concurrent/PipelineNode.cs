using System;
using System.Collections.Concurrent;
using System.Threading;



namespace CB.System.Concurrent {
  public class PipelineNode<TIn, TOut> : IDisposable {
    private readonly Thread _thread;
    private readonly BlockingCollection<TIn> _inBuffer;
    private readonly Action<TOut> _consumer;
    private readonly Func<TIn, TOut> _execution;
    private bool _running;
    public readonly CancellationToken CancellationToken;



    private PipelineNode(
      BlockingCollection<TIn> inBuffer,
      Func<TIn, TOut> execution,
      Action<TOut> consumer,
      CancellationToken ct = default(CancellationToken)) {
      _running = false;
      _inBuffer = inBuffer;
      _consumer = consumer;
      CancellationToken = ct;
      _execution = execution;
      _thread = new Thread( LoopHandle ) {
        Name = GetType().Name,
        IsBackground = true
      };
    }



    public PipelineNode(
      Func<TIn, TOut> execution,
      Action<TOut> consumer,
      CancellationToken ct)
      : this(
        new BlockingCollection<TIn>(),
        execution,
        consumer,
        ct
      ) { }



    public PipelineNode(
      Func<TIn, TOut> execution,
      Action<TOut> consumer,
      int inBufferBoundedCapacity,
      CancellationToken ct)
      : this(
        new BlockingCollection<TIn>( inBufferBoundedCapacity ),
        execution,
        consumer,
        ct
      ) { }



    private void LoopHandle() {
      while (_running) {
        var input = _inBuffer.Take( CancellationToken );
        var output = _execution( input );
        _consumer( output );
      }
    }



    public void Start() {
      _running = true;
      _thread.Start();
    }



    public void Shutdown() {
      _running = false;
    }



    public void Add(TIn item) {
      _inBuffer.Add( item, CancellationToken );
    }



    public bool TryAdd(TIn item) {
      return _inBuffer.TryAdd( item );
    }



    public void Dispose() {
      _inBuffer?.Dispose();
    }
  }
}
