using System.Text.Json;

namespace OcctSharp.Generator.Emission;

/// <summary>Requires exact template admission for bounded support profiles.</summary>
internal static class SupportWorkflowTemplateGuard
{
    internal static void Require(Type owner, string contractName, string resource, string fingerprint)
    {
        if (Path.GetFileName(contractName) != contractName || Path.GetFileName(resource) != resource
            || contractName.Contains("..", StringComparison.Ordinal) || resource.Contains("..", StringComparison.Ordinal))
            throw new InvalidDataException("Support template contract escaped the embedded input boundary.");
        using Stream stream = owner.Assembly.GetManifestResourceStream("OcctSharp.Generator.Emission." + contractName)
            ?? throw new InvalidDataException("Support template contract is missing: " + contractName);
        using JsonDocument contract = JsonDocument.Parse(stream);
        Validate(contract.RootElement, resource, fingerprint);
    }

    internal static void Validate(JsonElement contract, string resource, string fingerprint)
    {
        if (!contract.GetProperty("templates").TryGetProperty(resource, out var expected)
            || !string.Equals(expected.GetString(), fingerprint, StringComparison.Ordinal))
            throw new InvalidDataException("Support template changed without exact admission: " + resource);
    }
}
