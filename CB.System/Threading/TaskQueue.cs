using System;
using System.Threading;
using System.Threading.Tasks;



namespace CB.System.Threading {
  public class TaskQueue : IDisposable {
    private readonly CancellationToken _cancellationToken;

    private readonly object _lock = new object();
    private Task _pauseTask;
    private Task _previousTask = new Task(() => { });



    public TaskQueue(CancellationToken cancellationToken, bool paused) {
      _cancellationToken = cancellationToken;
      _pauseTask = _previousTask;
      if (!paused) {
        Start();
      }
    }



    public TaskQueue(CancellationToken cancellationToken)
      : this(cancellationToken, false) { }



    public TaskQueue()
      : this(CancellationToken.None, false) { }



    public bool Disposed { get; private set; }



    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }



    public Task Execute(Action action) {
      lock (_lock) {
        return _previousTask = _previousTask.ContinueWith(
                 t => action(),
                 _cancellationToken,
                 TaskContinuationOptions.None,
                 TaskScheduler.Default
               );
      }
    }



    public Task<T> Execute<T>(Func<T> work) {
      lock (_lock) {
        var task = _previousTask.ContinueWith(
          t => work(),
          _cancellationToken,
          TaskContinuationOptions.None,
          TaskScheduler.Default
        );
        _previousTask = task;
        return task;
      }
    }



    public void Start() {
      Task pauseTask;
      lock (_lock) {
        pauseTask = _pauseTask;
        _pauseTask = null;
      }

      pauseTask?.Start();
    }



    public void Stop() {
      Task pauseTask;
      lock (_lock) {
        if (_pauseTask != null) {
          return;
        }

        pauseTask = _pauseTask = _previousTask;
      }

      pauseTask.Wait(_cancellationToken);
    }



    protected virtual void Dispose(bool disposing) {
      // If you need thread safety, use a lock around these  
      // operations, as well as in your methods that use the resource.
      if (Disposed) {
        return;
      }

      //TODO: stop
      if (disposing) {
        _previousTask?.Dispose();
        _previousTask = null;
        _pauseTask?.Dispose();
        _pauseTask = null;
      }

      // Indicate that the instance has been disposed.
      Disposed = true;
    }



    ~TaskQueue() {
      Dispose(false);
    }
  }
}
