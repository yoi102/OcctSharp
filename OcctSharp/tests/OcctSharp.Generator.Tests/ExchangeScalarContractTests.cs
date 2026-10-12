using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Tests;

public sealed class ExchangeScalarContractTests
{
    [Fact]
    public void ContractSelectsOnlyAuditedScalarExchangeOperations()
    {
        string[] iges = OwnedValueOperationContract.StableIds
            .Where(id => id.StartsWith("c:@S@IGESData_GlobalSection@F@", StringComparison.Ordinal)).ToArray();
        string[] step = OwnedValueOperationContract.StableIds
            .Where(id => id.StartsWith("c:@S@StepData_Field@F@", StringComparison.Ordinal)).ToArray();
        Assert.Equal(31, iges.Length);
        Assert.Equal(14, step.Length);
        Assert.Contains("c:@S@IGESData_GlobalSection@F@SetScale#d#", iges);
        Assert.Contains("c:@S@StepData_Field@F@SetInteger#I#", step);
        Assert.DoesNotContain("c:@S@StepData_Field@F@SetInteger#I#I#", step);
        Assert.DoesNotContain("c:@S@StepData_Field@F@SetBoolean#I#b#", step);
        Assert.DoesNotContain("c:@S@StepData_Field@F@SetReal#I#d#", step);
        Assert.DoesNotContain("c:@S@StepData_Field@F@Clear#I#", step);
        Assert.DoesNotContain(iges, id => id.Contains("HAsciiString", StringComparison.Ordinal));
    }
}
