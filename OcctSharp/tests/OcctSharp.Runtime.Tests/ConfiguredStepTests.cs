using OcctSharp.DocumentProjections;
using OcctSharp.StepCafWorkspaces;
using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;

public sealed class ConfiguredStepTests
{
    [Theory][InlineData(StepCafSchema.Ap203)][InlineData(StepCafSchema.Ap214)][InlineData(StepCafSchema.Ap242)]
    public void ActualFileSchemasGeometryAndFieldFidelity(StepCafSchema schema)=>BatchCDStepWorkflow.RunFacade(schema);
    [Fact] public void NativeDocumentDomainSupportsExplicitConfiguration()=>BatchCDStepWorkflow.RunNative();
    [Fact] public void FullSemanticAttributesSurviveNativeDomainRewrite()=>BatchCDStepWorkflow.RunFacade(StepCafSchema.Ap242,true);
    [Fact] public void ExplicitDuplicateCleaningReportsPinnedSdkFailureWithoutPublishingPartialFile()
    {
        var failure=Assert.Throws<OcctException>(()=>BatchCDStepWorkflow.RunFacade(StepCafSchema.Ap242,cleanDuplicates:true));
        Assert.Contains("IFSelect status 2",failure.Message);
        Assert.Equal("FileIoError",failure.NativeStatus);
    }
    [Fact] public void RejectionPreservesDestinationAndLeavesNoStagingFile()
    {
        string directory=Path.Combine(Path.GetTempPath(),"OcctSharp.CD.Rollback."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            string path=Path.Combine(directory,"existing.step");File.WriteAllText(path,"existing destination");using var doc=XdeDocument.Create();using var box=ShapeFactory.CreateBox(1,1,1);
            using(var transaction=doc.BeginTransaction())
            {
                doc.AddShape(box,"part");Assert.ThrowsAny<Exception>(()=>StepCafFiles.WriteConfigured(doc.Projections,path,new()));
                Assert.Equal("existing destination",File.ReadAllText(path));Assert.Single(Directory.GetFiles(directory));Assert.True(transaction.Commit());
            }
            foreach(double invalid in new[]{double.NaN,double.PositiveInfinity,0,-1})Assert.Throws<ArgumentOutOfRangeException>(()=>StepCafFiles.WriteConfigured(doc.Projections,path,new(Precision:invalid)));
            Assert.Throws<ArgumentOutOfRangeException>(()=>StepCafFiles.WriteConfigured(doc.Projections,path,new((StepCafSchema)7)));
            Assert.Throws<ArgumentOutOfRangeException>(()=>StepCafFiles.WriteConfigured(doc.Projections,path,new(Modes:(StepCafWriteModes)2048)));
            Assert.Equal("existing destination",File.ReadAllText(path));Assert.Single(Directory.GetFiles(directory));
            using var context=doc.Projections;context.Dispose();Assert.Throws<ObjectDisposedException>(()=>StepCafFiles.WriteConfigured(context,path,new()));
        }
        finally{Directory.Delete(directory,true);}
    }
}
