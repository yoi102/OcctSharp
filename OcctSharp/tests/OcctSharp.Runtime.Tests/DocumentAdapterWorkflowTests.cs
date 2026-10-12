using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;
public sealed class DocumentAdapterWorkflowTests
{
    [Theory][InlineData("Binary")][InlineData("Xml")]
    public void DirectGeneratedModuleAndRetainedDocumentTransactionsPersistenceAndActualViewer(string format)=>BatchCBDocumentWorkflow.Run(format);
}
