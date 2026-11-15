namespace CB.System.Linq.Expressions;

using global::System;
using global::System.Linq.Expressions;
using Xunit;



public class ExpressionXTests {
  private sealed class Subject {
    public int Number { get; set; } = 7;

    public int Other { get; set; } = 3;

    public int Method() => 42;
  }



  private readonly Subject _subject = new();



  [Fact]
  public void GetMemberName_WithMemberAccess_ExpectMemberName() =>
    Assert.Equal(nameof(Subject.Number), ((Expression<Func<int>>)(() => _subject.Number)).GetMemberName());



  [Fact]
  public void GetMemberName_WithBoxedMemberAccess_ExpectMemberName() =>
    Assert.Equal(nameof(Subject.Number), ((Expression<Func<object>>)(() => _subject.Number)).GetMemberName());



  [Fact]
  public void GetMemberName_WithMemberOfLambdaParameter_ExpectMemberName() =>
    Assert.Equal(nameof(Subject.Number), ((Expression<Func<Subject, int>>)(s => s.Number)).GetMemberName());



  [Fact]
  public void GetMemberName_WithBoxedMemberOfLambdaParameter_ExpectMemberName() =>
    Assert.Equal(nameof(Subject.Number), ((Expression<Func<Subject, object>>)(s => s.Number)).GetMemberName());



  [Fact]
  public void GetMemberName_WithOtherSupportedBodies_ExpectKindOfExpression() {
    Assert.Equal("Constant", ((Expression<Func<int>>)(() => 42)).GetMemberName());
    Assert.Equal("Binary", ((Expression<Func<int>>)(() => _subject.Number + _subject.Other)).GetMemberName());
    Assert.Equal("MethodCall", ((Expression<Func<int>>)(() => _subject.Method())).GetMemberName());
    Assert.Equal("New", ((Expression<Func<object>>)(() => new Subject())).GetMemberName());
    Assert.Equal("Parameter", ((Expression<Func<int, int>>)(x => x)).GetMemberName());
    Assert.Equal("Conditional", ((Expression<Func<int>>)(() => _subject.Number > 0 ? 1 : 2)).GetMemberName());
  }



  [Fact]
  public void GetMemberName_WithUnsupportedBody_ExpectArgumentExceptionNamingTheNodeType() {
    Expression<Func<int[]>> expression = () => new[] { 1, 2 };

    var exception = Assert.Throws<ArgumentException>(() => expression.GetMemberName());

    Assert.Contains(nameof(ExpressionType.NewArrayInit), exception.Message, StringComparison.Ordinal);
  }



  [Fact]
  public void GetMemberName_WithNull_ExpectArgumentNullException() {
    Expression<Func<int>>? expression = null;

    Assert.Throws<ArgumentNullException>(() => expression!.GetMemberName());
  }



  [Fact]
  public void GetMemberNameValue_WithMemberAccess_ExpectNameAndCurrentValue() {
    var (name, value) = ((Expression<Func<int>>)(() => _subject.Number)).GetMemberNameValue();

    Assert.Equal(nameof(Subject.Number), name);
    Assert.Equal(7, value);
  }



  [Fact]
  public void GetMemberNameValue_WithBinaryBody_ExpectKindAndEvaluatedValue() {
    var (name, value) = ((Expression<Func<int>>)(() => _subject.Number + _subject.Other)).GetMemberNameValue();

    Assert.Equal("Binary", name);
    Assert.Equal(10, value);
  }
}
