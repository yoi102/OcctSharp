using System.Text.Json;
using OcctSharp.Generator.Emission;

namespace OcctSharp.Generator.Tests;

public sealed class SupportWorkflowTemplateGuardTests
{
    [Fact]
    public void OnlyTheAdmittedResourceAndExactFingerprintAreAccepted()
    {
        using var contract = JsonDocument.Parse("""{"templates":{"native.txt":"FINGERPRINT"}}""");
        SupportWorkflowTemplateGuard.Validate(contract.RootElement, "native.txt", "FINGERPRINT");
        Assert.Throws<InvalidDataException>(() =>
            SupportWorkflowTemplateGuard.Validate(contract.RootElement, "native.txt", "changed"));
        Assert.Throws<InvalidDataException>(() =>
            SupportWorkflowTemplateGuard.Validate(contract.RootElement, "other.txt", "FINGERPRINT"));
    }
}
