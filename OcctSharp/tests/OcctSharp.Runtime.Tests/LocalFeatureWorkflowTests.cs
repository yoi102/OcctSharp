using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;
public sealed class LocalFeatureWorkflowTests
{
    [Theory][InlineData("Fillet")][InlineData("Chamfer")][InlineData("Sweep")]
    public void SelectedFeatureRollbackStepAndActualViewerRemainIndependent(string operation) => BatchBZLocalFeatureWorkflow.Run(operation);
}
