namespace System.Threading.Tasks;

/// <summary>The non-generic <c>TaskCompletionSource</c> (.NET 5+) over <c>TaskCompletionSource&lt;bool&gt;</c>.</summary>
internal sealed class TaskCompletionSource {
  private readonly TaskCompletionSource<bool> _inner;

  public Task Task => _inner.Task;



  public TaskCompletionSource()
    : this(TaskCreationOptions.None) { }



  public TaskCompletionSource(TaskCreationOptions creationOptions) => _inner = new(creationOptions);



  public void SetResult() => _inner.SetResult(true);



  public bool TrySetResult() => _inner.TrySetResult(true);



  public void SetException(Exception exception) => _inner.SetException(exception);



  public bool TrySetException(Exception exception) => _inner.TrySetException(exception);



  public bool TrySetCanceled() => _inner.TrySetCanceled();
}
