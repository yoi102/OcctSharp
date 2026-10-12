using OcctSharp.Tests.Shared;
using OcctSharp.LocalStepExports;
namespace OcctSharp.Runtime.Tests;
public sealed class LocatedAssemblyWorkflowTests
{
 [Fact]public void LocatedDefinitionsRetainSelectionCommandAndNoPartialPublicationRejections()=>BatchCCLocatedAssemblyWorkflow.RunRejections();
 [Fact]public void GeneratedNativeExportActuallyPreservesSharedLocatedDefinitionsAndCumulativeTransforms()=>BatchCCLocatedAssemblyWorkflow.Run();
 [Fact]public void GeneratedManagedExportFromPersistedLocatedDefinitionActuallyRoundtrips()=>BatchCCLocatedAssemblyWorkflow.Run((source,roots,path)=>
 {
  using var doc=OcafDocument.Open(source);var context=doc.Projections;
  var written=LocalStepAssemblyExport.Export(context,roots.Select(context.GetLabel),path);
  return new(written.DirectoryPath,written.SourceLengthUnitMeters,written.SourceLengthUnitWasDeclared,written.Selected.Select(x=>x.SourceEntry).ToArray(),written.Files.Select(x=>(x.FileName,x.SourceDefinitionEntry)).ToArray());
 });
}
