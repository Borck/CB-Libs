namespace CB.System.Linq.Expressions;

using global::System;
using global::System.Linq.Expressions;



/// <summary>Extensions for expression trees: naming what a lambda like <c>() =&gt; obj.Property</c> refers to.</summary>
public static class ExpressionX {
  /// <summary>
  ///   Name of the member an expression refers to, so a property can be named without a string:
  ///   <c>(() =&gt; person.Name).GetMemberName()</c> is <c>"Name"</c>.
  /// </summary>
  /// <remarks>
  ///   A member access (also under a boxing conversion) gives the member's name. Any other supported body gives the kind of
  ///   expression: "Constant", "Binary", "MethodCall", "New", "Parameter" or "Conditional".
  /// </remarks>
  /// <exception cref="ArgumentNullException"><paramref name="expression" /> is null.</exception>
  /// <exception cref="ArgumentException">The body is none of the above.</exception>
  public static string GetMemberName<T>(this Expression<Func<T>> expression) =>
    NameOf(expression, nameof(expression));



  /// <inheritdoc cref="GetMemberName{T}(Expression{Func{T}})" />
  public static string GetMemberName<TObj, TRet>(this Expression<Func<TObj, TRet>> expression) =>
    NameOf(expression, nameof(expression));



  /// <summary>The name from <see cref="GetMemberName{T}(Expression{Func{T}})"> and the value the expression evaluates to.</summary>
  /// <remarks>The expression is compiled and run on every call; do not use it on a hot path.</remarks>
  public static (string Name, T Value) GetMemberNameValue<T>(this Expression<Func<T>> expression) =>
    (expression.GetMemberName(), expression.Compile()());



  private static string NameOf(LambdaExpression? expression, string parameterName) {
    ArgumentNullException.ThrowIfNull(expression, parameterName);

    return expression.Body switch {
      MemberExpression member => member.Member.Name,
      // A boxing or other conversion around the member, e.g. () => (object)person.Age.
      UnaryExpression { Operand: MemberExpression member } => member.Member.Name,
      ConstantExpression => "Constant",
      BinaryExpression => "Binary",
      MethodCallExpression => "MethodCall",
      NewExpression => "New",
      ParameterExpression => "Parameter",
      ConditionalExpression => "Conditional",
      var other => throw new ArgumentException($"Unsupported expression type: {other.NodeType}", parameterName)
    };
  }
}
