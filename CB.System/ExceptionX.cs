using System;



namespace CB.System {
  public static class ExceptionX {
    public static Exception GetInnerMostException(this Exception e) {
      var result = e;
      while (result.InnerException != null) {
        result = result.InnerException;
      }

      return result;
    }
  }
}
