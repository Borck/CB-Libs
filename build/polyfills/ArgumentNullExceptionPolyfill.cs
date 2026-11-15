namespace System;

using global::System.Diagnostics.CodeAnalysis;
using global::System.Runtime.CompilerServices;



/// <summary>
///   <c>ArgumentNullException.ThrowIfNull</c> for the targets that predate it (net48, netstandard2.0), so the code reads the same
///   on every target. Compiled into each assembly for those targets only (Directory.Build.targets); internal, so no assembly
///   exports it.
/// </summary>
internal static class ArgumentNullExceptionPolyfill {
  extension(ArgumentNullException) {
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
      if (argument is null) {
        throw new ArgumentNullException(paramName);
      }
    }
  }
}
