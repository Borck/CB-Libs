using System;
using Cloo;



namespace CB.CV.OpenCL {
  static class OpenCl {
    private static readonly Lazy<ComputeContext> LzOclc = new Lazy<ComputeContext>(
      () =>
        new ComputeContext(
          ComputeDeviceTypes.Gpu,
          new ComputeContextPropertyList(ComputePlatform.Platforms[0]),
          null,
          IntPtr.Zero
        )
    );

    internal static ComputeContext OclContext => LzOclc.Value;
    private static ComputeDevice _oclDefaultDevice;
    private static bool _oclDefaultInited;
    public const float CL_WEIGHT_MAX_MEM_ALLOC = 1 / 1.3f;



    public static long GetWeightedMaxMemoryAllocationSize(this ComputeDevice thisDevice) {
      return (long)(thisDevice.MaxMemoryAllocationSize * CL_WEIGHT_MAX_MEM_ALLOC);
    }



    internal static ComputeDevice GetDefaultOclDevice() {
      if (_oclDefaultInited) {
        return _oclDefaultDevice;
      }

      var ds = OclContext.Devices;
      if (ds.Count > 0) {
        _oclDefaultDevice = ds[0];
      } else {
        SetOpenClFailed();
      }

      _oclDefaultInited = true;
      return _oclDefaultDevice;
    }



    internal static OpenClData GetOpenCl() {
      return new OpenClData(OclContext, GetDefaultOclDevice(), OpenClFailed);
    }



    public static bool OpenClFailed { get; private set; }



    internal static bool SetOpenClFailed() {
      return OpenClFailed = true;
    }
  }
}
