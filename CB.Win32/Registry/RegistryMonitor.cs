using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CB.System;
using Microsoft.Win32;



namespace CB.Win32.Registry {
  /// <summary>
  ///   <b>RegistryMonitor</b> allows you to monitor specific registry key.
  /// </summary>
  /// <remarks>
  ///   If a monitored registry key changes, an event is fired. You can subscribe to these
  ///   events by adding a delegate to <see cref="RegChanged" />.
  ///   <para>
  ///     The Windows API provides a function
  ///     <a href="http://msdn.microsoft.com/library/en-us/sysinfo/base/regnotifychangekeyvalue.asp">
  ///       RegNotifyChangeKeyValue
  ///     </a>
  ///     , which is not covered by the
  ///     <see cref="RegistryKey" /> class. <see cref="RegistryMonitor" /> imports
  ///     that function and encapsulates it in a convenient manner.
  ///   </para>
  /// </remarks>
  /// <example>
  ///   This sample shows how to monitor <c>HKEY_CURRENT_USER\Environment</c> for changes:
  ///   <code>
  ///  public class MonitorSample
  ///  {
  ///      static void Main() 
  ///      {
  ///          RegistryMonitor monitor = new RegistryMonitor(RegistryHive.CurrentUser, "Environment");
  ///          monitor.RegChanged += new EventHandler(OnRegChanged);
  ///          monitor.Start();
  /// 
  ///          while(true);
  ///  
  /// 			monitor.Stop();
  ///      }
  /// 
  ///      private void OnRegChanged(object sender, EventArgs e)
  ///      {
  ///          Console.WriteLine("registry key has changed");
  ///      }
  ///  }
  ///  </code>
  /// </example>
  public class RegistryMonitor {
    /// <summary>
    ///   Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="registryKey">The registry key to monitor.</param>
    public RegistryMonitor(RegistryKey registryKey) {
      InitRegistryKey(registryKey.Name);
    }



    /// <summary>
    ///   Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public RegistryMonitor(string name) {
      if (string.IsNullOrEmpty(name)) {
        throw new ArgumentNullException(nameof(name));
      }

      InitRegistryKey(name);
    }



    /// <summary>
    ///   Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="registryHive">The registry hive.</param>
    /// <param name="subKey">The sub key.</param>
    public RegistryMonitor(RegistryHive registryHive, string subKey) {
      InitRegistryKey(registryHive, subKey);
    }



    /// <summary>
    ///   Gets or sets the <see cref="RegChangeNotifyFilter">RegChangeNotifyFilter</see>.
    /// </summary>
    public RegChangeNotifyFilter RegChangeNotifyFilter {
      get { return _regFilter; }
      set {
        lock (_threadLock) {
          if (IsMonitoring) {
            throw new InvalidOperationException("Monitoring thread is already running");
          }

          _regFilter = value;
        }
      }
    }

    /// <summary>
    ///   <b>true</b> if this <see cref="RegistryMonitor" /> object is currently monitoring;
    ///   otherwise, <b>false</b>.
    /// </summary>
    public bool IsMonitoring => _thread != null;



    /// <summary>
    ///   Start monitoring.
    /// </summary>
    public void Start() {
      lock (_threadLock) {
        if (IsMonitoring) {
          return;
        }

        _eventTerminate.Reset();
        _thread = new Thread(MonitorThread) {IsBackground = true};
        _thread.Start();
      }
    }



    /// <summary>
    ///   Stops the monitoring thread.
    /// </summary>
    public void Stop() {
      lock (_threadLock) {
        var thread = _thread;
        if (thread == null) {
          return;
        }

        _eventTerminate.Set();
        thread.Join();
      }
    }



    private void MonitorThread() {
      try {
        ThreadLoop();
      } catch (Exception e) {
        OnError(e);
      }

      _thread = null;
    }



    private void ThreadLoop() {
      var result = UnsafeNativeMethods.RegOpenKeyEx(
        _registryHive,
        _registrySubName,
        0,
        STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY,
        out var registryKey
      );
      if (result != 0) {
        throw new Win32Exception(result);
      }

      try {
        var eventNotify = new AutoResetEvent(false);
        WaitHandle[] waitHandles = {eventNotify, _eventTerminate};
        while (!_eventTerminate.WaitOne(0, true)) {
          result = UnsafeNativeMethods.RegNotifyChangeKeyValue(
            registryKey,
            true,
            _regFilter,
            eventNotify.SafeWaitHandle.DangerousGetHandle(),
            true
          );
          if (result != 0) {
            throw new Win32Exception(result);
          }

          if (WaitHandle.WaitAny(waitHandles) == 0) {
            OnRegChanged();
          }
        }
      } finally {
        if (registryKey != IntPtr.Zero) {
          UnsafeNativeMethods.RegCloseKey(registryKey);
        }
      }
    }



    #region P/Invoke

    private static class UnsafeNativeMethods {
      [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
      internal static extern int RegOpenKeyEx(IntPtr hKey,
                                              string subKey,
                                              uint options,
                                              int samDesired,
                                              out IntPtr phkResult);



      [DllImport("advapi32.dll", SetLastError = true)]
      internal static extern int RegNotifyChangeKeyValue(IntPtr hKey,
                                                         bool bWatchSubtree,
                                                         RegChangeNotifyFilter dwNotifyFilter,
                                                         IntPtr hEvent,
                                                         bool fAsynchronous);



      [DllImport("advapi32.dll", SetLastError = true)]
      internal static extern int RegCloseKey(IntPtr hKey);
    }



    private const int KEY_QUERY_VALUE = 0x0001;
    private const int KEY_NOTIFY = 0x0010;
    private const int STANDARD_RIGHTS_READ = 0x00020000;

    private static readonly IntPtr HkeyClassesRoot = new IntPtr(unchecked((int)0x80000000));
    private static readonly IntPtr HkeyCurrentUser = new IntPtr(unchecked((int)0x80000001));
    private static readonly IntPtr HkeyLocalMachine = new IntPtr(unchecked((int)0x80000002));
    private static readonly IntPtr HkeyUsers = new IntPtr(unchecked((int)0x80000003));
    private static readonly IntPtr HkeyPerformanceData = new IntPtr(unchecked((int)0x80000004));
    private static readonly IntPtr HkeyCurrentConfig = new IntPtr(unchecked((int)0x80000005));
    private static readonly IntPtr HkeyDynData = new IntPtr(unchecked((int)0x80000006));

    private static readonly Dictionary<RegistryHive, IntPtr> HivePtrMap = new Dictionary<RegistryHive, IntPtr> {
      {RegistryHive.ClassesRoot, HkeyClassesRoot},
      {RegistryHive.CurrentUser, HkeyCurrentUser},
      {RegistryHive.LocalMachine, HkeyLocalMachine},
      {RegistryHive.Users, HkeyUsers},
      {RegistryHive.PerformanceData, HkeyPerformanceData},
      {RegistryHive.CurrentConfig, HkeyCurrentConfig},
    };

    private static readonly Dictionary<string, IntPtr> KeyPtrMap = new Dictionary<string, IntPtr> {
      {Registry.HKCR, HkeyClassesRoot},
      {Registry.HKCR2, HkeyClassesRoot},
      {Registry.HKCU, HkeyCurrentUser},
      {Registry.HKCU2, HkeyCurrentUser},
      {Registry.HKLM, HkeyLocalMachine},
      {Registry.HKLM2, HkeyLocalMachine},
      {Registry.HKU, HkeyUsers},
      {Registry.HKPD, HkeyPerformanceData},
      {Registry.HKCC, HkeyCurrentConfig},
      {Registry.HKDD, HkeyDynData},
      {Registry.HKDD2, HkeyDynData}
    };

    #endregion

    #region Event handling

    /// <summary>
    ///   Occurs when the specified registry key has changed.
    /// </summary>
    public event EventHandler RegChanged;



    /// <summary>
    ///   Raises the <see cref="RegChanged" /> event.
    /// </summary>
    /// <remarks>
    ///   <p>
    ///     <b>OnRegChanged</b> is called when the specified registry key has changed.
    ///   </p>
    ///   <note type="inheritinfo">
    ///     When overriding <see cref="OnRegChanged" /> in a derived class, be sure to call
    ///     the base class's <see cref="OnRegChanged" /> method.
    ///   </note>
    /// </remarks>
    protected virtual void OnRegChanged() {
      var handler = RegChanged;
      handler?.Invoke(this, null);
    }



    /// <summary>
    ///   Occurs when the access to the registry fails.
    /// </summary>
    public event ErrorEventHandler Error;



    /// <summary>
    ///   Raises the <see cref="Error" /> event.
    /// </summary>
    /// <param name="e">The <see cref="Exception" /> which occured while watching the registry.</param>
    /// <remarks>
    ///   <p>
    ///     <b>OnError</b> is called when an exception occurs while watching the registry.
    ///   </p>
    ///   <note type="inheritinfo">
    ///     When overriding <see cref="OnError" /> in a derived class, be sure to call
    ///     the base class's <see cref="OnError" /> method.
    ///   </note>
    /// </remarks>
    protected virtual void OnError(Exception e) {
      var handler = Error;
      handler?.Invoke(this, new ErrorEventArgs(e));
    }

    #endregion

    #region Private member variables

    private IntPtr _registryHive;
    private string _registrySubName;
    private readonly object _threadLock = new object();
    private Thread _thread;
    private readonly ManualResetEvent _eventTerminate = new ManualResetEvent(false);

    private RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key |
                                               RegChangeNotifyFilter.Attribute |
                                               RegChangeNotifyFilter.Value |
                                               RegChangeNotifyFilter.Security;

    #endregion

    #region Initialization

    private void InitRegistryKey(RegistryHive hive, string name) {
      _registrySubName = name;
      if (!HivePtrMap.TryGetValue(hive, out _registryHive)) {
        throw new InvalidEnumArgumentException(nameof(hive), (int)hive, typeof(RegistryHive));
      }
    }



    private void InitRegistryKey(string name) {
      var (hive, _) = name.Separate('\\');
      if (!KeyPtrMap.TryGetValue(hive, out _registryHive)) {
        throw new ArgumentException("The registry hive '" + hive + "' is not supported", nameof(name));
      }
    }

    #endregion
  }



  /// <summary>
  ///   Filter for notifications reported by <see cref="RegistryMonitor" />.
  /// </summary>
  [Flags]
  public enum RegChangeNotifyFilter {
    /// <summary>Notify the caller if a subkey is added or deleted.</summary>
    Key = 1,

    /// <summary>
    ///   Notify the caller of changes to the attributes of the key,
    ///   such as the security descriptor information.
    /// </summary>
    Attribute = 2,

    /// <summary>
    ///   Notify the caller of changes to a value of the key. This can
    ///   include adding or deleting a value, or changing an existing value.
    /// </summary>
    Value = 4,

    /// <summary>
    ///   Notify the caller of changes to the security descriptor
    ///   of the key.
    /// </summary>
    Security = 8
  }
}
