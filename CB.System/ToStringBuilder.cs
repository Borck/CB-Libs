namespace CB.System;

using global::System;
using global::System.Linq.Expressions;
using global::System.Text;

// forgive the mangled code; I hate horizontal scrolling
public sealed class ToStringBuilder<T> {
  private readonly T _obj;
  private readonly Type _objType;
  private readonly StringBuilder _innerSb;



  public ToStringBuilder(T obj) {
    _obj = obj;
    _objType = obj.GetType();
    _innerSb = new StringBuilder();
  }



  public ToStringBuilder<T> Append<TProperty>(Expression<Func<T, TProperty>> expression) {
    if (!TryGetPropertyName(expression, out var propertyName)) {
      throw new ArgumentException(
        "Expression must be a simple property expression."
      );
    }

    var func = expression.Compile();

    var commaNeeded = _innerSb.Length >= 1;
    _innerSb.Append(
      (commaNeeded ? ", " : "") + propertyName + ": " + func(_obj)
    );
    return this;
  }



  private static bool TryGetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression,
                                                    out string propertyName) {
    propertyName = default;

    if (expression.Body is not MemberExpression propertyExpression) {
      return false;
    }

    propertyName = propertyExpression.Member.Name;
    return true;
  }



  public override string ToString() => _objType.Name + "{" + _innerSb + "}";
}
