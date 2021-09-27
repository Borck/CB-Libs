using System;
using System.Threading;



namespace IVI.Common {
  public static class LockX {
    #region methods and others

    /// <summary>
    ///   Invokes the action inside a enter and exit write lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="writer"></param>
    public static void WriteLocked(this ReaderWriterLockSlim @lock, Action writer) {
      try {
        @lock.EnterWriteLock();
        writer.Invoke();
      } finally {
        @lock.ExitWriteLock();
      }
    }



    /// <summary>
    ///   Invokes the function inside a enter and exit write lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="writer"></param>
    public static T WriteLocked<T>(this ReaderWriterLockSlim @lock, Func<T> writer) {
      try {
        @lock.EnterWriteLock();
        return writer.Invoke();
      } finally {
        @lock.ExitWriteLock();
      }
    }



    /// <summary>
    ///   Invokes the action inside a enter and exit read lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="reader"></param>
    public static void ReadLocked(this ReaderWriterLockSlim @lock, Action reader) {
      try {
        @lock.EnterReadLock();
        reader.Invoke();
      } finally {
        @lock.ExitReadLock();
      }
    }



    /// <summary>
    ///   Invokes the function inside a enter and exit read lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="reader"></param>
    public static T ReadLocked<T>(this ReaderWriterLockSlim @lock, Func<T> reader) {
      try {
        @lock.EnterReadLock();
        return reader.Invoke();
      } finally {
        @lock.ExitReadLock();
      }
    }



    /// <summary>
    ///   Invokes the action inside a enter and exit write lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="writer"></param>
    /// <param name="timeout"></param>
    public static void WriteLocked(this ReaderWriterLock @lock, Action writer, TimeSpan timeout) {
      try {
        @lock.AcquireWriterLock( timeout );
        writer.Invoke();
      } finally {
        @lock.ReleaseWriterLock();
      }
    }



    /// <summary>
    ///   Invokes the action inside a enter and exit write lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="writer"></param>
    /// <param name="timeout"></param>
    public static void WriteLocked(this ReaderWriterLock @lock, Action writer, int timeout) {
      try {
        @lock.AcquireWriterLock( timeout );
        writer.Invoke();
      } finally {
        @lock.ReleaseWriterLock();
      }
    }



    /// <summary>
    ///   Invokes the function inside a enter and exit write lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="writer"></param>
    /// <param name="timeout"></param>
    public static T WriteLocked<T>(this ReaderWriterLock @lock, Func<T> writer, TimeSpan timeout) {
      try {
        @lock.AcquireWriterLock( timeout );
        return writer.Invoke();
      } finally {
        @lock.ReleaseWriterLock();
      }
    }



    /// <summary>
    ///   Invokes the function inside a enter and exit write lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="writer"></param>
    /// <param name="timeout"></param>
    public static T WriteLocked<T>(this ReaderWriterLock @lock, Func<T> writer, int timeout) {
      try {
        @lock.AcquireWriterLock( timeout );
        return writer.Invoke();
      } finally {
        @lock.ReleaseWriterLock();
      }
    }



    /// <summary>
    ///   Invokes the action inside a enter and exit read lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="reader"></param>
    /// <param name="timeout"></param>
    public static void ReadLocked(this ReaderWriterLock @lock, Action reader, TimeSpan timeout) {
      try {
        @lock.AcquireReaderLock( timeout );
        reader.Invoke();
      } finally {
        @lock.ReleaseReaderLock();
      }
    }



    /// <summary>
    ///   Invokes the action inside a enter and exit read lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="reader"></param>
    /// <param name="timeout"></param>
    public static void ReadLocked(this ReaderWriterLock @lock, Action reader, int timeout) {
      try {
        @lock.AcquireReaderLock( timeout );
        reader.Invoke();
      } finally {
        @lock.ReleaseReaderLock();
      }
    }



    /// <summary>
    ///   Invokes the function inside a enter and exit read lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="reader"></param>
    /// <param name="timeout"></param>
    public static T ReadLocked<T>(this ReaderWriterLock @lock, Func<T> reader, TimeSpan timeout) {
      try {
        @lock.AcquireReaderLock( timeout );
        return reader.Invoke();
      } finally {
        @lock.ReleaseReaderLock();
      }
    }



    /// <summary>
    ///   Invokes the function inside a enter and exit read lock
    /// </summary>
    /// <param name="lock"></param>
    /// <param name="reader"></param>
    /// <param name="timeout"></param>
    public static T ReadLocked<T>(this ReaderWriterLock @lock, Func<T> reader, int timeout) {
      try {
        @lock.AcquireReaderLock( timeout );
        return reader.Invoke();
      } finally {
        @lock.ReleaseReaderLock();
      }
    }

    #endregion
  }
}
