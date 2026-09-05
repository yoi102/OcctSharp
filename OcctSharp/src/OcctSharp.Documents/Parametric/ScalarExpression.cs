using System.Text.Json.Serialization;

namespace OcctSharp;

#pragma warning disable CS1591
public enum ParametricExpressionKind { Literal, Reference, Add, Subtract, Multiply, Divide, Negate, Minimum, Maximum }

/// <summary>A bounded arithmetic tree. No source code, scripts, reflection or callbacks are evaluated.</summary>
public sealed class ParametricExpression
{
    [JsonConstructor]
    public ParametricExpression(ParametricExpressionKind kind, ParametricValue? literal,
        ParametricParameterReference? reference, IReadOnlyList<ParametricExpression>? arguments)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        ParametricExpression[] copied = arguments?.ToArray() ?? [];
        int arity = kind switch { ParametricExpressionKind.Literal or ParametricExpressionKind.Reference => 0,
            ParametricExpressionKind.Negate => 1, _ => 2 };
        if (copied.Length != arity || copied.Any(x => x is null)) throw new ArgumentException("Expression arity is invalid.");
        if (kind == ParametricExpressionKind.Literal) _ = (literal ?? throw new ArgumentNullException(nameof(literal))).Quantity();
        else if (literal is not null) throw new ArgumentException("Only literal expressions contain a value.");
        if (kind == ParametricExpressionKind.Reference)
        {
            if (reference is not { } key || key.FeatureId == Guid.Empty || string.IsNullOrWhiteSpace(key.Name) || key.Name.Length > 128)
                throw new ArgumentException("A reference requires a feature identity and parameter name.");
        }
        else if (reference is not null) throw new ArgumentException("Only reference expressions contain a reference.");
        Kind = kind;
        Literal = literal;
        Reference = reference;
        Arguments = Array.AsReadOnly(copied);
        ValidateBounds();
    }

    public ParametricExpressionKind Kind { get; }
    public ParametricValue? Literal { get; }
    public ParametricParameterReference? Reference { get; }
    public IReadOnlyList<ParametricExpression> Arguments { get; }
    public static ParametricExpression Constant(double value, ParametricUnit unit = ParametricUnit.None) =>
        new(ParametricExpressionKind.Literal, ParametricValue.FromReal(value, unit), null, null);
    public static ParametricExpression Parameter(Guid feature, string name) =>
        new(ParametricExpressionKind.Reference, null, new(feature, name), null);
    public static ParametricExpression Operation(ParametricExpressionKind kind, params ParametricExpression[] arguments) =>
        new(kind, null, null, arguments);

    public IReadOnlyList<ParametricParameterReference> References()
    {
        List<ParametricParameterReference> result = [];
        Stack<ParametricExpression> pending = new([this]);
        while (pending.TryPop(out var node))
        {
            if (node.Reference is { } value && !result.Contains(value)) result.Add(value);
            foreach (var child in node.Arguments) pending.Push(child);
        }
        return result.AsReadOnly();
    }

    /// <summary>Evaluates copied, explicitly supplied parameter values in base units.</summary>
    public ParametricQuantity Evaluate(IReadOnlyDictionary<ParametricParameterReference, ParametricValue> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return EvaluateCore(key => values.TryGetValue(key, out var value) ? value.Quantity()
            : throw new InvalidOperationException($"Missing parameter {key.FeatureId}/{key.Name}."));
    }

    internal ParametricQuantity EvaluateCore(Func<ParametricParameterReference, ParametricQuantity> resolve)
    {
        if (Kind == ParametricExpressionKind.Literal) return Literal!.Quantity();
        if (Kind == ParametricExpressionKind.Reference) return resolve(Reference!.Value);
        ParametricQuantity left = Arguments[0].EvaluateCore(resolve);
        if (Kind == ParametricExpressionKind.Negate) return new(-left.Value, left.Dimension);
        ParametricQuantity right = Arguments[1].EvaluateCore(resolve);
        if (Kind is ParametricExpressionKind.Multiply or ParametricExpressionKind.Divide)
        {
            int sign = Kind == ParametricExpressionKind.Multiply ? 1 : -1;
            if (sign == -1 && right.Value == 0) throw new DivideByZeroException("A parameter expression divides by zero.");
            return new(sign == 1 ? left.Value * right.Value : left.Value / right.Value,
                new(left.Dimension.Length + sign * right.Dimension.Length, left.Dimension.Angle + sign * right.Dimension.Angle));
        }
        if (left.Dimension != right.Dimension) throw new InvalidOperationException("Expression operands have incompatible dimensions.");
        return new(Kind switch
        {
            ParametricExpressionKind.Add => left.Value + right.Value,
            ParametricExpressionKind.Subtract => left.Value - right.Value,
            ParametricExpressionKind.Minimum => Math.Min(left.Value, right.Value),
            ParametricExpressionKind.Maximum => Math.Max(left.Value, right.Value),
            _ => throw new InvalidOperationException("Unsupported expression operator.")
        }, left.Dimension);
    }

    private void ValidateBounds()
    {
        int count = 0;
        Stack<(ParametricExpression Node, int Depth)> pending = new([(this, 1)]);
        while (pending.TryPop(out var item))
        {
            if (++count > 1024 || item.Depth > 32) throw new ArgumentException("Expression exceeds 1024 nodes or 32 levels.");
            foreach (var child in item.Node.Arguments) pending.Push((child, item.Depth + 1));
        }
    }
}
#pragma warning restore CS1591
