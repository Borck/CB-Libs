using System;
using Cloo;
using JetBrains.Annotations;



namespace CB.CV.OpenCL {
  internal struct OpenClData {
    public readonly ComputeContext Context;
    public readonly ComputeDevice Device;
    public readonly bool InitFailed;

    public OpenClData([NotNull] ComputeContext context, [NotNull] ComputeDevice device, bool failed) {
      Context = context;
      Device = device;
      InitFailed = failed;
    }

    public ComputeCommandQueue CreateCommandQueue(ComputeCommandQueueFlags flags = ComputeCommandQueueFlags.None) {
      return new ComputeCommandQueue( Context, Device, flags );
    }

    public ComputeProgram BuildProgramm(string code) {
      var programm = new ComputeProgram( Context, code );
      programm.Build( new[] { Device }, null, null, IntPtr.Zero );
      return programm;
    }
  }
}
