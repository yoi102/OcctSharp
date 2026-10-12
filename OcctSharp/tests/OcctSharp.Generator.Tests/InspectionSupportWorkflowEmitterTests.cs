using System.Security.Cryptography;
using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using Xunit;

namespace OcctSharp.Generator.Tests;

public sealed class InspectionSupportWorkflowEmitterTests
{
    private static DiscoveryConfiguration Configuration()=>new("1.13","8.0.1","test",[],new Dictionary<string,string>())
        {InspectionSupportWorkflows=[InspectionSupportWorkflowEmitter.PeriodicDistanceWorkflow]};
    [Fact] public void DisabledSupportDoesNotReadOrWriteSdkFiles()
    {
        Assert.Empty(InspectionSupportWorkflowEmitter.Emit(Configuration() with{InspectionSupportWorkflows=[]},"missing-sdk"));
    }
    [Theory][InlineData("8.0.0","periodic-exact-distance-v1")][InlineData("8.0.1","unknown")][InlineData("8.0.1","PERIODIC-EXACT-DISTANCE-V1")]
    public void UnsupportedVersionOrSelectionFailsBeforeOpeningTheSdk(string version,string workflow)
    {
        Assert.Throws<InvalidDataException>(()=>InspectionSupportWorkflowEmitter.Emit(Configuration() with
            {OcctVersion=version,InspectionSupportWorkflows=[workflow]},"missing-sdk"));
    }
    [Fact] public void DuplicateWorkflowCannotDuplicateGeneratedDefinitions()
    {
        Assert.Throws<InvalidDataException>(()=>InspectionSupportWorkflowEmitter.Emit(Configuration() with
            {InspectionSupportWorkflows=[InspectionSupportWorkflowEmitter.PeriodicDistanceWorkflow,InspectionSupportWorkflowEmitter.PeriodicDistanceWorkflow]},"missing-sdk"));
    }
    [Fact] public void ChangedAuditedHeaderStopsGeneration()
    {
        byte[] accepted=Encoding.UTF8.GetBytes("accepted audited header");
        var expected=new Dictionary<string,string>{{"Geom_Curve.hxx",Convert.ToHexString(SHA256.HashData(accepted))}};
        InspectionSupportWorkflowEmitter.ValidateHeaderFingerprints(expected,_=>accepted);
        Assert.Throws<InvalidDataException>(()=>InspectionSupportWorkflowEmitter.ValidateHeaderFingerprints(expected,_=>Encoding.UTF8.GetBytes("changed ABI")));
    }
    [Theory][InlineData("../outside.hxx")][InlineData("subdirectory/outside.hxx")]
    public void ContractCannotReadOutsideAuditedIncludeRoot(string header)
    {
        bool read=false;
        Assert.Throws<InvalidDataException>(()=>InspectionSupportWorkflowEmitter.ValidateHeaderFingerprints(
            new Dictionary<string,string>{{header,new string('0',64)}},_=>{read=true;return [];}));
        Assert.False(read);
    }
    [Fact] public void RepeatedGenerationProducesTheSameTwoInternalSupportFiles()
    {
        var first=InspectionSupportWorkflowEmitter.CreatePeriodicDistanceFiles();
        var second=InspectionSupportWorkflowEmitter.CreatePeriodicDistanceFiles();
        Assert.Equal(2,first.Count);Assert.Equal(first,second);
    }
}
