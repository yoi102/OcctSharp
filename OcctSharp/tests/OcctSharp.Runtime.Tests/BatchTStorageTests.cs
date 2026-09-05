using System.Text.Json;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchTStorageTests
{
    [Fact]
    public void TypedValuesCopyArraysDistinguishMissingAndRoundTripSchema()
    {
        int[] source = [1, 2];
        var copied = ParametricValue.FromIntegers(source);
        source[0] = 99;
        Assert.Equal(1, copied.Integers[0]);
        Assert.False(ParametricValue.Missing().HasValue);
        Assert.True(ParametricValue.FromInteger(0).HasValue);
        Assert.True(ParametricValue.FromText("").HasValue);
        Assert.True(ParametricValue.FromIntegers([]).HasValue);
        var definition = Definition(Guid.NewGuid(), "box");
        var restored = JsonSerializer.Deserialize<ParametricFeatureDefinition>(JsonSerializer.Serialize(definition))!;
        Assert.Equal(definition.Id, restored.Id);
        Assert.Equal(1, restored.Parameters["x"].Value!.Real);
        Assert.Throws<NotSupportedException>(() => new ParametricFeatureDefinition(Guid.NewGuid(), "future", ParametricFeatureKind.Box, new Dictionary<string, ParametricParameter>(), [], schemaVersion: 2));
    }

    [Fact]
    public void ExpressionsEnforceUnitsBoundedArithmeticAndMissingInputs()
    {
        var sum = ParametricExpression.Operation(ParametricExpressionKind.Add,
            ParametricExpression.Constant(1, ParametricUnit.Meter), ParametricExpression.Constant(2, ParametricUnit.Centimeter));
        var result = sum.Evaluate(new Dictionary<ParametricParameterReference, ParametricValue>());
        Assert.Equal(1020, result.Value);
        Assert.Equal(ParametricDimension.Distance, result.Dimension);
        var invalid = ParametricExpression.Operation(ParametricExpressionKind.Add,
            ParametricExpression.Constant(1, ParametricUnit.Meter), ParametricExpression.Constant(1, ParametricUnit.Degree));
        Assert.Throws<InvalidOperationException>(() => invalid.Evaluate(new Dictionary<ParametricParameterReference, ParametricValue>()));
        var divide = ParametricExpression.Operation(ParametricExpressionKind.Divide, ParametricExpression.Constant(1), ParametricExpression.Constant(0));
        Assert.Throws<DivideByZeroException>(() => divide.Evaluate(new Dictionary<ParametricParameterReference, ParametricValue>()));
        var expression = ParametricExpression.Constant(1);
        for (int i = 1; i < 32; i++) expression = ParametricExpression.Operation(ParametricExpressionKind.Negate, expression);
        Assert.Throws<ArgumentException>(() => ParametricExpression.Operation(ParametricExpressionKind.Negate, expression));
        var missing = ParametricExpression.Parameter(Guid.NewGuid(), "absent");
        Assert.Throws<InvalidOperationException>(() => missing.Evaluate(new Dictionary<ParametricParameterReference, ParametricValue>()));
    }

    [Fact]
    public void PlansResolveCrossFeatureParametersAndRejectExpressionCycles()
    {
        Guid a = Guid.NewGuid(), b = Guid.NewGuid();
        var first = Definition(a, "a");
        var second = Definition(b, "b").WithParameter("x", ParametricParameter.FromExpression(ParametricExpression.Parameter(a, "x")));
        var plan = ParametricPlanning.Build([second, first]);
        Assert.True(plan.CanExecute);
        Assert.Equal(new[] { a, b }, plan.Order);
        Assert.Equal(1, plan.Quantities[new(b, "x")].Value);
        var cycle = first.WithParameter("x", ParametricParameter.FromExpression(ParametricExpression.Parameter(b, "x")));
        Assert.False(ParametricPlanning.Build([cycle, second]).CanExecute);
        var localCycle = first.WithParameter("x", ParametricParameter.FromExpression(ParametricExpression.Parameter(a, "x")));
        Assert.Contains(ParametricPlanning.Build([localCycle]).Issues, x => x.Code == "Parameter");
    }

    [Fact]
    public void PlansRejectMissingInputsAndIncompatibleOutputKinds()
    {
        var source = Definition(Guid.NewGuid(), "a");
        var dependent = Definition(Guid.NewGuid(), "b").WithInputs([new("source", source.Id, ParametricOutputKind.Mesh)]);
        Assert.Contains(ParametricPlanning.Build([source, dependent]).Issues, x => x.Code == "InputType");
        Assert.Contains(ParametricPlanning.Build([dependent]).Issues, x => x.Code == "MissingInput");
        Assert.Throws<ArgumentException>(() => ParametricPlanning.Build([source, source]));
    }

    [Fact]
    public void NativeParameterStoragePreservesEmptyUnicodeTypedValuesAndAbort()
    {
        using var doc = OcafDocument.Create();
        var storage = new ParametricStorage(doc.Handle);
        string entry;
        using (var command = doc.BeginTransaction())
        {
            entry = doc.RootLabel.AddChild().Entry;
            storage.SetParameter(entry, ParametricValue.FromText("颜色参数"));
            Assert.Equal("颜色参数", storage.GetParameter(entry).Text);
            storage.SetParameter(entry, ParametricValue.FromIntegers([]));
            Assert.Equal(ParametricValueKind.IntegralArray, storage.GetParameter(entry).Kind);
            Assert.Empty(storage.GetParameter(entry).Integers);
            storage.SetParameter(entry, ParametricValue.FromReals([1, 2.5], ParametricUnit.Centimeter));
            Assert.Equal(new[] { 1, 2.5 }, storage.GetParameter(entry).Reals);
            Assert.Equal(ParametricUnit.Centimeter, storage.GetParameter(entry).Unit);
            storage.SetParameter(entry, ParametricValue.FromInteger(0));
            command.Commit();
        }
        using (doc.BeginTransaction()) storage.SetParameter(entry, ParametricValue.Missing());
        Assert.Equal(ParametricValueKind.Integral, storage.GetParameter(entry).Kind);
        Assert.Throws<ArgumentException>(() => storage.SetParameter(entry, ParametricValue.FromInteger(1)));
        doc.Dispose();
        Assert.Throws<ObjectDisposedException>(() => storage.GetParameter(entry));
    }

    [Fact]
    public void NativeGraphRewireRejectsCyclesBeforeChangingEitherDirection()
    {
        using var doc = OcafDocument.Create();
        var storage = new ParametricStorage(doc.Handle);
        using var command = doc.BeginTransaction();
        string a = doc.RootLabel.AddChild().Entry, b = doc.RootLabel.AddChild().Entry;
        int first = storage.Register(a, Guid.NewGuid()), second = storage.Register(b, Guid.NewGuid());
        storage.Rewire(b, [first]);
        Assert.Equal(new[] { second }, storage.Links(a, true).Links);
        Assert.Equal(new[] { first }, storage.Links(b, false).Links);
        Assert.Throws<ArgumentException>(() => storage.Rewire(a, [second]));
        Assert.Empty(storage.Links(a, false).Links);
        Assert.Equal(new[] { first }, storage.Links(b, false).Links);
        Assert.Throws<ArgumentException>(() => storage.Remove(a));
        storage.Logbook(a, 1);
        Assert.Equal(1, storage.Logbook(a, 5) & 7);
        storage.Logbook(a, 2);
        Assert.Equal(3, storage.Logbook(a, 5) & 7);
        storage.State(a, ParametricExecutionState.Succeeded);
        storage.Logbook(a, 3);
        Assert.Equal(7, storage.Logbook(a, 5) & 7);
        storage.Remove(b);
        Assert.Empty(storage.Links(a, true).Links);
        command.Commit();
    }

    internal static ParametricFeatureDefinition Definition(Guid id, string name) => new(id, name, ParametricFeatureKind.Box,
        new Dictionary<string, ParametricParameter>
        {
            ["x"] = ParametricParameter.FromValue(ParametricValue.FromReal(1, ParametricUnit.Millimeter)),
            ["y"] = ParametricParameter.FromValue(ParametricValue.FromReal(2, ParametricUnit.Millimeter)),
            ["z"] = ParametricParameter.FromValue(ParametricValue.FromReal(3, ParametricUnit.Millimeter))
        }, []);
}
#pragma warning restore CA1861
