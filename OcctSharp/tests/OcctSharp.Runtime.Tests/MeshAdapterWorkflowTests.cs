using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;
public sealed class MeshAdapterWorkflowTests
{
 [Theory][InlineData("Remesh")][InlineData("Patch")]
 public void MeshChannelsRevisionFailurePersistenceAndActualViewerRemainIndependent(string operation)=>BatchCAMeshWorkflow.Run(operation);
}
