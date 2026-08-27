using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Inventory;

public static class LongTailClassification
{
    public static OcctFinalClassification Create(
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlyList<string> allHeaders,
        IReadOnlySet<string> successfulHeaders,
        IReadOnlyList<OcctInventoryFailure> failures,
        IReadOnlySet<string>? emittedStableIds = null,
        IReadOnlySet<string>? manualStableIds = null,
        IReadOnlyDictionary<string, BindingSkipReason>? excludedBindings = null,
        IReadOnlyDictionary<string, BindingSkipReason>? excludedPackages = null)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(allHeaders);
        ArgumentNullException.ThrowIfNull(successfulHeaders);
        ArgumentNullException.ThrowIfNull(failures);

        ValidateKnownStableIds(declarations, emittedStableIds, manualStableIds, excludedBindings);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(new BindingModel(declarations));
        LongTailContext context = LongTailContext.Create(declarations);
        OcctDeclarationDisposition[] declarationDispositions = declarations
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .Select(declaration => ClassifyDeclaration(
                declaration,
                typeMap,
                emittedStableIds,
                manualStableIds,
                excludedBindings,
                excludedPackages,
                context))
            .ToArray();

        Dictionary<string, OcctInventoryFailure> failureByHeader = failures
            .ToDictionary(static failure => failure.Header, StringComparer.Ordinal);
        OcctHeaderDisposition[] headerDispositions = allHeaders
            .Order(StringComparer.Ordinal)
            .Select(header => successfulHeaders.Contains(header)
                ? new OcctHeaderDisposition(header, "Parsed", "HD000", "Parsed")
                : failureByHeader.TryGetValue(header, out OcctInventoryFailure? failure)
                    ? ClassifyHeaderFailure(failure)
                    : new OcctHeaderDisposition(header, "Pending", "HD999", "UnaccountedHeader"))
            .ToArray();

        int declarationPending = declarationDispositions.Count(static item => item.State == "Pending");
        int headerPending = headerDispositions.Count(static item => item.State == "Pending");
        return new OcctFinalClassification(
            declarationPending == 0 && headerPending == 0,
            declarationDispositions.Length,
            declarationDispositions.Length - declarationPending,
            declarationPending,
            CountStates(declarationDispositions.Select(static item => item.State)),
            CountReasons(declarationDispositions.Select(static item => (item.Code, item.Category))),
            headerDispositions.Length,
            headerDispositions.Length - headerPending,
            headerPending,
            CountStates(headerDispositions.Select(static item => item.State)),
            CountReasons(headerDispositions.Select(static item => (item.Code, item.Category))),
            declarationDispositions,
            headerDispositions);
    }

    private static OcctDeclarationDisposition ClassifyDeclaration(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        IReadOnlySet<string>? emittedStableIds,
        IReadOnlySet<string>? manualStableIds,
        IReadOnlyDictionary<string, BindingSkipReason>? excludedBindings,
        IReadOnlyDictionary<string, BindingSkipReason>? excludedPackages,
        LongTailContext context)
    {
        if (emittedStableIds?.Contains(declaration.StableId) == true
            && manualStableIds?.Contains(declaration.StableId) == true)
        {
            throw new InvalidDataException(
                $"Declaration '{declaration.StableId}' cannot be both emitted and manual.");
        }

        (string State, string Code, string Category) disposition = emittedStableIds?.Contains(declaration.StableId) == true
            ? ("Emitted", "EM001", "GeneratedBinding")
            : manualStableIds?.Contains(declaration.StableId) == true
                ? ("Manual", "MN001", "ManualBinding")
            : excludedBindings?.TryGetValue(declaration.StableId, out BindingSkipReason? reason) == true
                ? ("Skipped", reason.Code, reason.Category)
            : excludedPackages?.TryGetValue(declaration.SourcePackage, out reason) == true
                ? ("Skipped", reason.Code, reason.Category)
            : declaration.SupportState switch
        {
            BindingSupportState.Supported => ("SupportedUnselected", "LT000", "EligibleUnselected"),
            BindingSupportState.Manual => ("Manual", "MN001", "ManualBinding"),
            BindingSupportState.Skipped when declaration.SkipReason is not null =>
                ("Skipped", declaration.SkipReason.Code, declaration.SkipReason.Category),
            BindingSupportState.Pending => ClassifyPending(declaration, typeMap, context),
            _ => ("Pending", "LT999", "UnrecognizedSupportState"),
        };

        return new OcctDeclarationDisposition(
            declaration.StableId,
            declaration.NativeName,
            declaration.Kind.ToString(),
            declaration.Header,
            declaration.SourcePackage,
            declaration.SourceToolkit,
            disposition.State,
            disposition.Code,
            disposition.Category);
    }

    private static void ValidateKnownStableIds(
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlySet<string>? emittedStableIds,
        IReadOnlySet<string>? manualStableIds,
        IReadOnlyDictionary<string, BindingSkipReason>? excludedBindings)
    {
        HashSet<string> discovered = declarations
            .Select(static declaration => declaration.StableId)
            .ToHashSet(StringComparer.Ordinal);
        string[] unknown = (emittedStableIds ?? new HashSet<string>(StringComparer.Ordinal))
            .Concat(manualStableIds ?? new HashSet<string>(StringComparer.Ordinal))
            .Concat(excludedBindings?.Keys ?? [])
            .Where(stableId => !discovered.Contains(stableId))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (unknown.Length != 0)
        {
            throw new InvalidDataException(
                $"Manifest stable IDs were not found in the inventory: {string.Join(", ", unknown)}.");
        }
    }

    private static (string State, string Code, string Category) ClassifyPending(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        LongTailContext context)
    {
        SimpleBindingEligibilityAssessment assessment = SimpleBindingEligibilityPass.Assess(declaration, typeMap);
        return assessment.Code switch
        {
            "EL001" when declaration.Kind == BindingDeclarationKind.Record =>
                ("Skipped", "SK012", "TypeMetadata"),
            "EL001" when declaration.Kind == BindingDeclarationKind.Enum =>
                EnumBindingEligibility.HasStableManagedTypeIdentity(declaration)
                    ? ("Blocked", "BL001", "EnumEmissionInvariant")
                    : ("Skipped", "SK017", "AnonymousOrUnnameableEnum"),
            "EL002" => ClassifyInstanceReceiver(declaration, typeMap, context),
            "EL003" when declaration.Kind == BindingDeclarationKind.Constructor =>
                ClassifyConstructor(declaration, typeMap, context),
            "EL003" => ClassifyType(declaration.ReturnType, BindingTypeUsage.ReturnValue),
            "EL004" => ClassifyFirstUnsupportedParameter(declaration, typeMap),
            "EL005" => ("Blocked", "BL002", "MissingToolkitProvenance"),
            "EL006" => ("Skipped", "SK013", "InternalHeaderFunction"),
            "EL007" => ("Blocked", "BL003", "UnverifiedFreeFunctionExport"),
            _ => ("Blocked", "BL099", "UnimplementedSpecificRule"),
        };
    }

    private static (string State, string Code, string Category) ClassifyInstanceReceiver(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        LongTailContext context)
    {
        string? declaringType = GetDeclaringType(declaration.NativeName);
        if (declaringType is null)
        {
            return ("Blocked", "BL101", "MissingDeclaringTypeIdentity");
        }
        if (IsDestructor(declaration, declaringType))
        {
            return ("Skipped", "SK014", "DestructorLifecycleBoundary");
        }
        if (declaration.IsPureVirtual)
        {
            return ("Skipped", "SK015", "PureVirtualDispatch");
        }
        if (!context.TransientTypes.Contains(declaringType))
        {
            return ("Blocked", "BL102", "NonTransientReceiverOwnership");
        }
        if (!IsSupportedSharedReturn(declaration.ReturnType, typeMap, context.TransientTypes))
        {
            return ClassifyType(declaration.ReturnType, BindingTypeUsage.ReturnValue);
        }
        return ClassifyFirstUnsupportedParameter(declaration, typeMap);
    }

    private static (string State, string Code, string Category) ClassifyConstructor(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        LongTailContext context)
    {
        string? declaringType = GetDeclaringType(declaration.NativeName);
        if (declaringType is not null && context.AbstractTypes.Contains(declaringType))
        {
            return ("Skipped", "SK016", "AbstractTypeConstruction");
        }
        if (declaringType is not null && context.TransientTypes.Contains(declaringType))
        {
            return ClassifyFirstUnsupportedParameter(declaration, typeMap);
        }
        return ("Blocked", "BL103", "NonTransientValueConstruction");
    }

    private static (string State, string Code, string Category) ClassifyFirstUnsupportedParameter(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        foreach (BindingParameter parameter in declaration.Parameters)
        {
            if (!IsValueCopy(parameter.Type, typeMap, BindingTypeUsage.Parameter))
            {
                return ClassifyType(parameter.Type, BindingTypeUsage.Parameter);
            }
        }
        return ("Blocked", "BL104", "CallableEligibilityInvariant");
    }

    private static (string State, string Code, string Category) ClassifyType(
        BindingType? type,
        BindingTypeUsage usage)
    {
        if (type is null)
        {
            return ("Blocked", "BL201", "MissingTypeFacts");
        }
        if (type.Layers.Any(static layer => layer.Kind == BindingTypeLayerKind.PointerIndirection))
        {
            return ("Blocked", "BL202", "RawPointerLifetime");
        }
        if (type.Layers.Any(static layer => layer.Kind == BindingTypeLayerKind.RValueReference))
        {
            return ("Blocked", "BL203", "RValueReferenceTransfer");
        }
        if (type.Layers.Any(static layer => layer.Kind == BindingTypeLayerKind.LValueReference))
        {
            bool verifiedConstInput = usage == BindingTypeUsage.Parameter
                && type.Layers is
                [
                    { Kind: BindingTypeLayerKind.LValueReference },
                    { Kind: BindingTypeLayerKind.Value, IsConstQualified: true },
                ];
            if (!verifiedConstInput)
            {
                return ("Blocked", "BL204", "BorrowedOrOutputReference");
            }
        }
        if (type.IsOcctHandle)
        {
            return ("Blocked", "BL205", "UnselectedHandleTarget");
        }
        if (!string.IsNullOrWhiteSpace(type.TemplateName) || type.TemplateArguments.Count != 0)
        {
            return ("Blocked", "BL206", "TemplateInstantiationProjection");
        }
        if (string.Equals(NormalizeTypeName(type.BaseCanonicalSpelling), "void", StringComparison.Ordinal))
        {
            return ("Blocked", "BL207", "VoidParameterOrMalformedReturn");
        }
        return ("Blocked", "BL208", "UnmappedValueType");
    }

    private static bool IsSupportedSharedReturn(
        BindingType? type,
        InitialTypeMap typeMap,
        IReadOnlySet<string> transientTypes)
    {
        if (type is null)
        {
            return false;
        }
        if (string.Equals(NormalizeTypeName(type.BaseCanonicalSpelling), "void", StringComparison.Ordinal)
            && type.Layers is [{ Kind: BindingTypeLayerKind.Value }])
        {
            return true;
        }
        if (IsValueCopy(type, typeMap, BindingTypeUsage.ReturnValue))
        {
            return true;
        }
        return type.IsOcctHandle
            && type.HandleTargetType is not null
            && transientTypes.Contains(NormalizeTypeName(type.HandleTargetType))
            && typeMap.TryMap(type, BindingTypeUsage.ReturnValue, out BindingTypeProjection? projection)
            && string.Equals(projection?.Ownership, "Shared", StringComparison.Ordinal);
    }

    private static bool IsValueCopy(BindingType type, InitialTypeMap typeMap, BindingTypeUsage usage) =>
        typeMap.TryMap(type, usage, out BindingTypeProjection? projection)
        && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal);

    private static string? GetDeclaringType(string nativeName)
    {
        int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal);
        return separator <= 0 ? null : NormalizeTypeName(nativeName[..separator]);
    }

    private static bool IsDestructor(BindingDeclaration declaration, string declaringType)
    {
        string memberName = declaration.NativeName[(declaration.NativeName.LastIndexOf("::", StringComparison.Ordinal) + 2)..];
        int separator = declaringType.LastIndexOf("::", StringComparison.Ordinal);
        string unqualifiedType = separator < 0 ? declaringType : declaringType[(separator + 2)..];
        return string.Equals(memberName, "~" + unqualifiedType, StringComparison.Ordinal);
    }

    private static string NormalizeTypeName(string value)
    {
        string normalized = value.Trim();
        foreach (string prefix in new[] { "class ", "struct ", "const " })
        {
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
            {
                normalized = normalized[prefix.Length..].TrimStart();
            }
        }
        return normalized.EndsWith(" const", StringComparison.Ordinal)
            ? normalized[..^6].TrimEnd()
            : normalized;
    }

    private sealed record LongTailContext(
        IReadOnlySet<string> TransientTypes,
        IReadOnlySet<string> AbstractTypes)
    {
        public static LongTailContext Create(IReadOnlyList<BindingDeclaration> declarations)
        {
            Dictionary<string, string[]> bases = declarations
                .Where(static declaration => declaration.Kind == BindingDeclarationKind.Record)
                .GroupBy(static declaration => NormalizeTypeName(declaration.NativeName), StringComparer.Ordinal)
                .ToDictionary(
                    static group => group.Key,
                    static group => group.SelectMany(static declaration => declaration.BaseTypes)
                        .Select(static baseType => NormalizeTypeName(baseType.Type.BaseCanonicalSpelling))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    StringComparer.Ordinal);
            HashSet<string> transientTypes = new(StringComparer.Ordinal) { "Standard_Transient" };
            bool changed;
            do
            {
                changed = false;
                foreach ((string type, string[] baseTypes) in bases)
                {
                    if (!transientTypes.Contains(type) && baseTypes.Any(transientTypes.Contains))
                    {
                        changed |= transientTypes.Add(type);
                    }
                }
            }
            while (changed);

            HashSet<string> abstractTypes = declarations
                .Where(static declaration => declaration.Kind == BindingDeclarationKind.Record && declaration.IsAbstract)
                .Select(static declaration => NormalizeTypeName(declaration.NativeName))
                .ToHashSet(StringComparer.Ordinal);
            foreach (BindingDeclaration method in declarations.Where(static declaration =>
                         declaration.Kind == BindingDeclarationKind.Method && declaration.IsPureVirtual))
            {
                if (GetDeclaringType(method.NativeName) is string declaringType)
                {
                    abstractTypes.Add(declaringType);
                }
            }
            return new LongTailContext(transientTypes, abstractTypes);
        }
    }

    private static OcctHeaderDisposition ClassifyHeaderFailure(OcctInventoryFailure failure)
    {
        string header = failure.Header;
        if (header.StartsWith("IVtk", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "BlockedExternalDependency", "HD001", "MissingVtk");
        }
        if (header.Equals("OpenGl_GLESExtensions.hxx", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "BlockedExternalDependency", "HD002", "MissingEglGlesContext");
        }
        if (header.Equals("RWGltf_GltfOStreamWriter.hxx", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "BlockedExternalDependency", "HD003", "MissingRapidJson");
        }
        if (header.Equals("NCollection_Haft.h", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "ExcludedLanguage", "HD004", "CppCliOnly");
        }
        if (failure.Error.Contains("file not found", StringComparison.Ordinal))
        {
            return new OcctHeaderDisposition(header, "UnavailableInArtifact", "HD005", "MissingOcctGeneratedHeader");
        }
        return new OcctHeaderDisposition(header, "Blocked", "HD099", "IsolatedParseFailure");
    }

    private static OcctDispositionCount[] CountStates(IEnumerable<string> states) => states
        .GroupBy(static state => state, StringComparer.Ordinal)
        .OrderBy(static group => group.Key, StringComparer.Ordinal)
        .Select(static group => new OcctDispositionCount(group.Key, group.Count()))
        .ToArray();

    private static OcctReasonCount[] CountReasons(IEnumerable<(string Code, string Category)> reasons) => reasons
        .GroupBy(static reason => reason, EqualityComparer<(string Code, string Category)>.Default)
        .OrderBy(static group => group.Key.Code, StringComparer.Ordinal)
        .ThenBy(static group => group.Key.Category, StringComparer.Ordinal)
        .Select(static group => new OcctReasonCount(group.Key.Code, group.Key.Category, group.Count()))
        .ToArray();
}
