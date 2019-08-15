using System.Threading.Tasks;



namespace CB.System.Threading {
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
}
