# CB.System

*This package is only for testing purpose*.

CB.System is a collection of common classes to solve frequently occurring challenges according to System.*. It contains extensions and utility classes for:

* Basic types like string, numerics, arrays, objects, Type and delegates,
* Runtime, IO and interop,
* Concurrency and threading,
* Reflection,
* Event and exception handling,
* Machine-wide readings (`CB.System.Diagnostics`): `SystemMemory.Read()` (total and available physical memory), `SystemCpuSampler` (CPU load,
  smoothed; `GetSystemTimes` on Windows, `/proc/stat` on Linux) and `SlidingWindowRateCounter` (events per second), and
* others.


## Contributors/Contributing

Maintained by [Christian Borck](https://github.com/Borck/)
