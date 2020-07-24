using System;



namespace CB.Win32.Native.Structures {
  [Flags]
  public enum ErrorModes : uint {
    SYSTEM_DEFAULT = 0x0,
    SEM_FAILCRITICALERRORS = 0x0001,
    SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
    SEM_NOGPFAULTERRORBOX = 0x0002,
    SEM_NOOPENFILEERRORBOX = 0x8000
  }
}
