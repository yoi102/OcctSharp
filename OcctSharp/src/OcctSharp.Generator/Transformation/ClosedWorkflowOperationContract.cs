using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

/// <summary>Reusable exact-signature admission for private, copied workflow adapters.</summary>
internal sealed class ClosedWorkflowOperationContract
{
    internal static readonly ClosedWorkflowOperationContract ModelingBuilders =
        new("modeling-builders", "ModelingBuilders", "TM021", OcctProductModule.Modeling);
    internal static readonly ClosedWorkflowOperationContract HatchingWorkspaces =
        new("hatching-workspaces", "HatchingWorkspaces", "TM022", OcctProductModule.Geometry);
    internal static readonly ClosedWorkflowOperationContract PolyDataWorkspaces =
        new("poly-data-workspaces", "PolyDataWorkspaces", "TM023", OcctProductModule.MeshData);
    internal static readonly ClosedWorkflowOperationContract MeshingWorkspaces =
        new("meshing-workspaces", "MeshingWorkspaces", "TM024", OcctProductModule.Mesh);
    internal static readonly ClosedWorkflowOperationContract DocumentProjections =
        new("document-projections", "DocumentProjections", "TM025", OcctProductModule.Documents);
    internal static readonly ClosedWorkflowOperationContract XdeAttributeProjections =
        new("xde-attribute-projections", "XdeAttributeProjections", "TM026", OcctProductModule.Xde);
    internal static readonly ClosedWorkflowOperationContract ExchangeSessions =
        new("exchange-sessions", "ExchangeSessions", "TM027", OcctProductModule.DataExchange);
    internal static readonly ClosedWorkflowOperationContract SelectionWorkspaces =
        new("selection-workspaces", "SelectionWorkspaces", "TM028", OcctProductModule.Visualization);
    internal static readonly ClosedWorkflowOperationContract FoundationValues =
        new("foundation-value-workspaces", "FoundationValues", "TM029", OcctProductModule.Foundation);
    internal static readonly ClosedWorkflowOperationContract FillingWorkspaces =
        new("filling-workspaces", "FillingWorkspaces", "TM030", OcctProductModule.Geometry);
    internal static readonly ClosedWorkflowOperationContract TopologyAssembly =
        new("topology-assembly", "TopologyAssembly", "TM031", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract XdeMetadata =
        new("xde-metadata-workspaces", "XdeMetadata", "TM032", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract StepCaf =
        new("stepcaf-workspaces", "StepCaf", "TM033", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract PolygonBoundary =
        new("polygon-boundary-workspaces", "PolygonBoundary", "TM034", OcctProductModule.MeshData);

    internal static readonly ClosedWorkflowOperationContract FoundationDiagnostics =
        new("foundation-diagnostics", "FoundationDiagnostics", "TM035", OcctProductModule.Foundation);

    internal static readonly ClosedWorkflowOperationContract AssemblyDiagnostics =
        new("assembly-diagnostics", "AssemblyDiagnostics", "TM036", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract CurveSampling =
        new("curve-sampling", "CurveSampling", "TM037", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract DocumentAssociations =
        new("document-associations", "DocumentAssociations", "TM038", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract ExchangeRoots =
        new("exchange-roots", "ExchangeRoots", "TM039", OcctProductModule.DataExchange);

    internal static readonly ClosedWorkflowOperationContract MeshBoundaryDiagnostics =
        new("mesh-boundary-diagnostics", "MeshBoundaryDiagnostics", "TM040", OcctProductModule.Mesh);

    internal static readonly ClosedWorkflowOperationContract NumericResults =
        new("numeric-results", "NumericResults", "TM041", OcctProductModule.Foundation);

    internal static readonly ClosedWorkflowOperationContract ModelingResults =
        new("modeling-results", "ModelingResults", "TM042", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract RayResults =
        new("ray-results", "RayResults", "TM043", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract SurfaceIntersections =
        new("surface-intersections", "SurfaceIntersections", "TM044", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract DirectCollections =
        new("direct-collections", "DirectCollections", "TM045", OcctProductModule.Documents);

    internal static readonly ClosedWorkflowOperationContract LocalStepAssembly =
        new("local-step-assembly", "LocalStepAssembly", "TM046", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract MeshExchangeConfig =
        new("mesh-exchange-config", "MeshExchangeConfig", "TM047", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract ParameterizedConstruction =
        new("parameterized-construction", "ParameterizedConstruction", "TM048", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract FiniteExtrema =
        new("finite-extrema", "FiniteExtrema", "TM049", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract BoundedTextTransforms =
        new("bounded-text-transforms", "BoundedTextTransforms", "TM050", OcctProductModule.Foundation);

    internal static readonly ClosedWorkflowOperationContract MeshProximity =
        new("mesh-proximity", "MeshProximity", "TM051", OcctProductModule.Mesh);

    internal static readonly ClosedWorkflowOperationContract LocalStepExport =
        new("local-step-export", "LocalStepExport", "TM052", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract LegacyKnotBuffers =
        new("legacy-knot-buffers", "LegacyKnotBuffers", "TM053", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract CoupledConstraints =
        new("coupled-constraints", "CoupledConstraints", "TM054", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract ProjectionAdapters =
        new("projection-adapters", "ProjectionAdapters", "TM055", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract FittingAdapters =
        new("fitting-adapters", "FittingAdapters", "TM056", OcctProductModule.Geometry);

    internal static readonly ClosedWorkflowOperationContract TopologyFaceAdapters =
        new("topology-face-adapters", "TopologyFaceAdapters", "TM057", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract BooleanRepairAdapters =
        new("boolean-repair-adapters", "BooleanRepairAdapters", "TM058", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract LocalFeatureAdapters =
        new("local-feature-adapters", "LocalFeatureAdapters", "TM059", OcctProductModule.Modeling);

    internal static readonly ClosedWorkflowOperationContract MeshAdapters =
        new("mesh-adapters", "MeshAdapters", "TM060", OcctProductModule.MeshData);

    internal static readonly ClosedWorkflowOperationContract DocumentAdapters =
        new("document-adapters", "DocumentAdapters", "TM061", OcctProductModule.Documents);

    internal static readonly ClosedWorkflowOperationContract XdeAdapters =
        new("xde-adapters", "XdeAdapters", "TM062", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract StepAdapters =
        new("step-adapters", "StepAdapters", "TM063", OcctProductModule.DataExchange);

    internal static readonly ClosedWorkflowOperationContract MeshExchangeAdapters =
        new("mesh-exchange-adapters", "MeshExchangeAdapters", "TM064", OcctProductModule.DataExchange);

    internal static readonly ClosedWorkflowOperationContract MeshResourceAdapters =
        new("mesh-resource-adapters", "MeshResourceAdapters", "TM065", OcctProductModule.Xde);

    internal static readonly ClosedWorkflowOperationContract ViewerAdapters =
        new("viewer-adapters", "ViewerAdapters", "TM066", OcctProductModule.Visualization);

    internal static readonly ClosedWorkflowOperationContract DocumentIdentityAdapters =
        new("document-identity-adapters", "DocumentIdentityAdapters", "TM067", OcctProductModule.Foundation);

    internal static readonly ClosedWorkflowOperationContract RenderResourceAdapters =
        new("render-resource-adapters", "RenderResourceAdapters", "TM068", OcctProductModule.Visualization);

    internal string Resource { get; }
    internal string Name { get; }
    internal string Rule { get; }
    internal OcctProductModule Module { get; }
    internal IReadOnlyDictionary<string, CopiedCollectionOperationContract.Operation> Operations { get; }

    private ClosedWorkflowOperationContract(string resource, string name, string rule, OcctProductModule module)
    {
        Resource = resource; Name = name; Rule = rule; Module = module;
        Operations = CopiedCollectionOperationContract.Load(resource + "-operation-contracts.json").Operations;
    }

    internal bool Contains(BindingDeclaration d) => Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && (op.Kind == "Function" ? d.Access == BindingAccess.None : d.Access == BindingAccess.Public)
        && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && (op.Kind is null && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
            || op.Kind == "Function" && d.Kind == BindingDeclarationKind.Function
                && d.Header == op.Header && d.SourcePackage == op.SourcePackage
                && (d.SourceToolkit is null || d.SourceToolkit == op.SourceToolkit))
        && d.SupportState != BindingSupportState.Skipped
        && (op.Supporting || d.SupportState != BindingSupportState.Manual);

    internal bool TryMap(BindingDeclaration d, BindingType t, BindingTypeUsage usage,
        InitialTypeMap map, out BindingTypeProjection? projection)
    {
        if (!Contains(d)) return map.TryMap(t, usage, out projection);
        projection = new(Rule, "void*", Name + "Handle", Name + "Snapshot", "PrivateWorkflowCopy",
            "Exact audited private operation; initialized call-owned inputs and success-only copied outputs.");
        return true;
    }
}
