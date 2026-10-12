using OcctSharp.OwnedValues;

namespace OcctSharp.Tests.Shared;

internal static class BatchAEExchangeScalarWorkflow
{
    internal static void Run()
    {
        using IGESDataGlobalSection section = IGESDataGlobalSection.Create();
        section.SetScale(2.5);
        section.SetIntegerBits(64);
        section.SetMaxCoord(12);
        if (section.Scale() != 2.5 || section.IntegerBits() != 64 ||
            !section.HasMaxCoord() || section.MaxCoord() != 12)
        {
            throw new InvalidOperationException("The IGES global scalar header did not round trip.");
        }

        using StepDataField field = StepDataField.Create();
        field.SetInteger(42);
        if (field.Int() != 42 || field.Integer(1, 1) != 42 || field.Arity() != 0)
        {
            throw new InvalidOperationException("The STEP integer field did not round trip.");
        }
        field.SetReal(3.5);
        if (field.Real(1, 1) != 3.5)
        {
            throw new InvalidOperationException("The STEP real field did not round trip.");
        }
    }
}
