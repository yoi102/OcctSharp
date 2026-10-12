using OcctSharp.OwnedValues;

namespace OcctSharp.Runtime.Tests;

public sealed class ExchangeScalarOwnedDataTests
{
    [Fact]
    public void CleanConsumerExchangeWorkflow() => global::OcctSharp.Tests.Shared.BatchAEExchangeScalarWorkflow.Run();

    [Fact]
    public void IgesGlobalSectionInitializesAndRoundTripsScalarHeaderFields()
    {
        using IGESDataGlobalSection section = IGESDataGlobalSection.Create();
        // The locked OCCT 8.0.1 implementation initializes these fields in its constructor.
        Assert.Equal(32, section.IntegerBits());
        Assert.Equal(1.0, section.Scale());
        Assert.False(section.HasMaxCoord());

        section.SetIntegerBits(64);
        section.SetMaxPower10Single(37);
        section.SetMaxDigitsSingle(7);
        section.SetMaxPower10Double(307);
        section.SetMaxDigitsDouble(16);
        section.SetScale(2.5);
        section.SetCascadeUnit(0.001);
        section.SetUnitFlag(2);
        section.SetLineWeightGrad(4);
        section.SetMaxLineWeight(0.75);
        section.SetResolution(0.01);
        section.SetIGESVersion(11);
        section.SetDraftingStandard(3);

        Assert.Equal(64, section.IntegerBits());
        Assert.Equal(37, section.MaxPower10Single());
        Assert.Equal(7, section.MaxDigitsSingle());
        Assert.Equal(307, section.MaxPower10Double());
        Assert.Equal(16, section.MaxDigitsDouble());
        Assert.Equal(2.5, section.Scale());
        Assert.Equal(0.001, section.CascadeUnit());
        Assert.Equal(2, section.UnitFlag());
        Assert.Equal(4, section.LineWeightGrad());
        Assert.Equal(0.75, section.MaxLineWeight());
        Assert.Equal(0.01, section.Resolution());
        Assert.Equal(11, section.IGESVersion());
        Assert.Equal(3, section.DraftingStandard());

        section.SetMaxCoord(12);
        section.MaxMaxCoord(-20);
        Assert.True(section.HasMaxCoord());
        Assert.Equal(20, section.MaxCoord());
        section.SetMaxCoord(0);
        Assert.False(section.HasMaxCoord());
        Assert.Equal(0, section.MaxCoord());
    }

    [Fact]
    public void StepFieldTransitionsBetweenScalarKindsAndEmptyState()
    {
        using StepDataField field = StepDataField.Create();
        Assert.Equal(0, field.Arity());
        Assert.Equal(0, field.Length(1));
        Assert.Equal(0, field.Lower(1));

        field.SetInteger(42);
        int integerKind = field.Kind(true);
        Assert.Equal(42, field.Int());
        Assert.Equal(42, field.Integer(1, 1));
        field.SetInt(43);
        Assert.Equal(43, field.Int());

        field.SetBoolean(true);
        Assert.NotEqual(integerKind, field.Kind(true));
        Assert.True(field.Boolean(1, 1));
        field.SetReal(3.5);
        Assert.Equal(3.5, field.Real(1, 1));
        field.SetDerived();
        Assert.Equal(0, field.Arity());
    }

    [Fact]
    public void ExchangeScalarOwnersRejectCallsAfterDispose()
    {
        IGESDataGlobalSection section = IGESDataGlobalSection.Create();
        section.Dispose();
        Assert.Throws<ObjectDisposedException>(() => section.Scale());
        StepDataField field = StepDataField.Create();
        field.Dispose();
        Assert.Throws<ObjectDisposedException>(() => field.SetInteger(1));
    }
}
