using System;
using System.Threading.Tasks;



namespace CB.System {
  public static class DelegateX {
    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task InvokeAsync(this Action action)
      => Task.Factory.FromAsync(
        action.BeginInvoke,
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T>(this Action<T> action, T obj)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(obj, asyncCallback, @object),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(arg1, arg2, asyncCallback, @object),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(arg1, arg2, arg3, asyncCallback, @object),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action,
                                                   T1 arg1,
                                                   T2 arg2,
                                                   T3 arg3,
                                                   T4 arg4)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(arg1, arg2, arg3, arg4, asyncCallback, @object),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action,
                                                       T1 arg1,
                                                       T2 arg2,
                                                       T3 arg3,
                                                       T4 arg4,
                                                       T5 arg5)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(arg1, arg2, arg3, arg4, arg5, asyncCallback, @object),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> action,
                                                           T1 arg1,
                                                           T2 arg2,
                                                           T3 arg3,
                                                           T4 arg4,
                                                           T5 arg5,
                                                           T6 arg6)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> action,
                                                               T1 arg1,
                                                               T2 arg2,
                                                               T3 arg3,
                                                               T4 arg4,
                                                               T5 arg5,
                                                               T6 arg6,
                                                               T7 arg7)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> action,
                                                                   T1 arg1,
                                                                   T2 arg2,
                                                                   T3 arg3,
                                                                   T4 arg4,
                                                                   T5 arg5,
                                                                   T6 arg6,
                                                                   T7 arg7,
                                                                   T8 arg8)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <param name="arg11"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10,
      T11 arg11)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          arg11,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <typeparam name="T12"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <param name="arg11"></param>
    /// <param name="arg12"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10,
      T11 arg11,
      T12 arg12)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          arg11,
          arg12,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <typeparam name="T12"></typeparam>
    /// <typeparam name="T13"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <param name="arg11"></param>
    /// <param name="arg12"></param>
    /// <param name="arg13"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10,
      T11 arg11,
      T12 arg12,
      T13 arg13)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          arg11,
          arg12,
          arg13,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <typeparam name="T12"></typeparam>
    /// <typeparam name="T13"></typeparam>
    /// <typeparam name="T14"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <param name="arg11"></param>
    /// <param name="arg12"></param>
    /// <param name="arg13"></param>
    /// <param name="arg14"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10,
      T11 arg11,
      T12 arg12,
      T13 arg13,
      T14 arg14)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          arg11,
          arg12,
          arg13,
          arg14,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <typeparam name="T12"></typeparam>
    /// <typeparam name="T13"></typeparam>
    /// <typeparam name="T14"></typeparam>
    /// <typeparam name="T15"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <param name="arg11"></param>
    /// <param name="arg12"></param>
    /// <param name="arg13"></param>
    /// <param name="arg14"></param>
    /// <param name="arg15"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10,
      T11 arg11,
      T12 arg12,
      T13 arg13,
      T14 arg14,
      T15 arg15)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          arg11,
          arg12,
          arg13,
          arg14,
          arg15,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );



    /// <summary>
    ///   Invokes the action asynchronous.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <typeparam name="T12"></typeparam>
    /// <typeparam name="T13"></typeparam>
    /// <typeparam name="T14"></typeparam>
    /// <typeparam name="T15"></typeparam>
    /// <typeparam name="T16"></typeparam>
    /// <param name="action"></param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <param name="arg8"></param>
    /// <param name="arg9"></param>
    /// <param name="arg10"></param>
    /// <param name="arg11"></param>
    /// <param name="arg12"></param>
    /// <param name="arg13"></param>
    /// <param name="arg14"></param>
    /// <param name="arg15"></param>
    /// <param name="arg16"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
      this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action,
      T1 arg1,
      T2 arg2,
      T3 arg3,
      T4 arg4,
      T5 arg5,
      T6 arg6,
      T7 arg7,
      T8 arg8,
      T9 arg9,
      T10 arg10,
      T11 arg11,
      T12 arg12,
      T13 arg13,
      T14 arg14,
      T15 arg15,
      T16 arg16)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => action.BeginInvoke(
          arg1,
          arg2,
          arg3,
          arg4,
          arg5,
          arg6,
          arg7,
          arg8,
          arg9,
          arg10,
          arg11,
          arg12,
          arg13,
          arg14,
          arg15,
          arg16,
          asyncCallback,
          @object
        ),
        action.EndInvoke,
        null
      );
  }
}
