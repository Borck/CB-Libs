using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;



namespace CB.System.Threading {
  /// <summary>
  ///   Source: https://stackoverflow.com/questions/3836584/lowering-priority-of-task-factory-startnew-thread#9056702
  /// </summary>
  public class PriorityScheduler : TaskScheduler {
    public static PriorityScheduler AboveNormal = new PriorityScheduler( ThreadPriority.AboveNormal );
    public static PriorityScheduler BelowNormal = new PriorityScheduler( ThreadPriority.BelowNormal );
    public static PriorityScheduler Lowest = new PriorityScheduler( ThreadPriority.Lowest );

    private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
    private readonly object _threadsLock = new object();

    private Thread[] _threads;
    private readonly ThreadPriority _priority;
    private readonly int _maximumConcurrencyLevel;



    public PriorityScheduler(ThreadPriority priority, int maximumConcurrencyLevel) {
      _priority = priority;
      _maximumConcurrencyLevel = Math.Max( 1, maximumConcurrencyLevel );
    }



    public PriorityScheduler(ThreadPriority priority)
      : this( priority, Environment.ProcessorCount ) { }



    public override int MaximumConcurrencyLevel => _maximumConcurrencyLevel;



    protected override IEnumerable<Task> GetScheduledTasks() {
      return _tasks;
    }



    protected override void QueueTask(Task task) {
      _tasks.Add( task );

      if (_threads != null)
        return;

      lock (_threadsLock) {
        if (_threads != null)
          return;

        _threads = new Thread[_maximumConcurrencyLevel];
        for (var i = 0; i < _threads.Length; i++) {
          var thread = new Thread(
            () => {
              foreach (var t in _tasks.GetConsumingEnumerable())
                TryExecuteTask( t );
            }
          ) {
            Name = $"PriorityScheduler: Thread {i}",
            Priority = _priority,
            IsBackground = true
          };
          thread.Start();
          _threads[i] = thread;
        }
      }
    }



    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
      return false; // we might not want to execute task that should schedule as high or low priority inline
    }
  }
}
