using System.Security.Cryptography;
using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Emission;
using Xunit;

namespace OcctSharp.Generator.Tests;

public sealed class ExchangeSupportWorkflowEmitterTests
{
    private static DiscoveryConfiguration Configuration()=>new("1.13","8.0.1","test",[],new Dictionary<string,string>())
        {ExchangeSupportWorkflows=[ExchangeSupportWorkflowEmitter.ExactOffsetWorkflow]};
    [Fact] public void DisabledSupportDoesNotReadOrWriteSdkFiles()
    {
        Assert.Empty(ExchangeSupportWorkflowEmitter.Emit(Configuration() with{ExchangeSupportWorkflows=[]},"missing-sdk"));
    }
    [Theory][InlineData("8.0.0","step-exact-offset-v1")][InlineData("8.0.1","unknown")][InlineData("8.0.1","STEP-EXACT-OFFSET-V1")]
    public void UnsupportedVersionOrSelectionFailsBeforeOpeningTheSdk(string version,string workflow)
    {
        Assert.Throws<InvalidDataException>(()=>ExchangeSupportWorkflowEmitter.Emit(Configuration() with
            {OcctVersion=version,ExchangeSupportWorkflows=[workflow]},"missing-sdk"));
    }
    [Fact] public void DuplicateWorkflowCannotDuplicateGeneratedDefinitions()
    {
        Assert.Throws<InvalidDataException>(()=>ExchangeSupportWorkflowEmitter.Emit(Configuration() with
            {ExchangeSupportWorkflows=[ExchangeSupportWorkflowEmitter.ExactOffsetWorkflow,ExchangeSupportWorkflowEmitter.ExactOffsetWorkflow]},"missing-sdk"));
    }
    [Fact] public void ChangedAuditedHeaderStopsGeneration()
    {
        byte[] accepted=Encoding.UTF8.GetBytes("accepted audited header");
        var expected=new Dictionary<string,string>{{"StepGeom_OffsetCurve3d.hxx",Convert.ToHexString(SHA256.HashData(accepted))}};
        ExchangeSupportWorkflowEmitter.ValidateHeaderFingerprints(expected,_=>accepted);
        Assert.Throws<InvalidDataException>(()=>ExchangeSupportWorkflowEmitter.ValidateHeaderFingerprints(expected,_=>Encoding.UTF8.GetBytes("changed ABI")));
    }
    [Theory][InlineData("../outside.hxx")][InlineData("subdirectory/outside.hxx")]
    public void ContractCannotReadOutsideAuditedIncludeRoot(string header)
    {
        bool read=false;
        Assert.Throws<InvalidDataException>(()=>ExchangeSupportWorkflowEmitter.ValidateHeaderFingerprints(
            new Dictionary<string,string>{{header,new string('0',64)}},_=>{read=true;return [];}));
        Assert.False(read);
    }
    [Fact] public void RepeatedGenerationProducesTheSameTwoInternalSupportFiles()
    {
        var first=ExchangeSupportWorkflowEmitter.CreateExactOffsetFiles();
        var second=ExchangeSupportWorkflowEmitter.CreateExactOffsetFiles();
        Assert.Equal(2,first.Count);Assert.Equal(first,second);
    }
}
