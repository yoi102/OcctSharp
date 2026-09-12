namespace OcctSharp.Runtime.Tests;

public sealed class BatchACConsumerWorkflowTests
{
    [Fact]
    public void OwnedUtilitiesComposeAcrossModuleBoundaries() =>
        OcctSharp.Tests.Shared.BatchACOwnedUtilityWorkflow.Run();
}
