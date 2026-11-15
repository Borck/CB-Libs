namespace CB.System.Threading;

using global::System.Threading.Tasks;

public static class TaskX {
  public static Task AsStarted(this Task task) {
    task.Start();
    return task;
  }



  public static Task<T> AsStarted<T>(this Task<T> task) {
    task.Start();
    return task;
  }
}
