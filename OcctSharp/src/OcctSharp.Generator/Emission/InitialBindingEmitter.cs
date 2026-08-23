using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Emission;

public static class InitialBindingEmitter
{
    private const string GeneratedNativeHeaderPath = "src/OcctSharp.Native/generated/OcctSharp.Generated.h";
    private const string GeneratedNativeSourcePath = "src/OcctSharp.Native/generated/OcctSharp.Generated.cpp";
    private const string GeneratedPointManagedPath = "src/OcctSharp/Generated/Point3dRaw.Generated.cs";
    private const string GeneratedStaticManagedPath = "src/OcctSharp/Generated/ScalarRaw.Generated.cs";

    public static GeneratedBindingSet Emit(DiscoveryReport report)
        => Emit(report, [GenerationScopeConfiguration.Precision], [], []);

    public static GeneratedBindingSet Emit(
        DiscoveryReport report,
        IReadOnlyList<GenerationScopeConfiguration> generationScopes)
        => Emit(report, generationScopes, [], []);

    public static GeneratedBindingSet Emit(
        DiscoveryReport report,
        IReadOnlyList<GenerationScopeConfiguration> generationScopes,
        IReadOnlyList<SharedHandleScopeConfiguration> sharedHandleScopes)
        => Emit(report, generationScopes, sharedHandleScopes, []);

    public static GeneratedBindingSet Emit(
        DiscoveryReport report,
        IReadOnlyList<GenerationScopeConfiguration> generationScopes,
        IReadOnlyList<SharedHandleScopeConfiguration> sharedHandleScopes,
        IReadOnlyList<TopologyScopeConfiguration> topologyScopes)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(generationScopes);
        ArgumentNullException.ThrowIfNull(sharedHandleScopes);
        ArgumentNullException.ThrowIfNull(topologyScopes);

        BindingModel valueEligibleModel = SimpleBindingEligibilityPass.Apply(report.Model);
        BindingModel eligibleModel = SharedHandleBindingEligibilityPass.Apply(valueEligibleModel);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(eligibleModel);
        BindingDeclaration pointConstructor = eligibleModel.Declarations.SingleOrDefault(
            declaration => IsPointCoordinateConstructor(declaration, typeMap))
            ?? throw new InvalidDataException(
                "A supported gp_Pnt constructor with three TM002 coordinates was not discovered.");
        BindingDeclaration? pointDefaultConstructor = eligibleModel.Declarations.SingleOrDefault(
            declaration => IsPointDefaultConstructor(declaration));
        BindingDeclaration? pointCopyConstructor = eligibleModel.Declarations.SingleOrDefault(
            declaration => IsPointCopyConstructor(declaration, typeMap));
        GeneratedStaticMethod[] staticMethods = SelectStaticMethods(eligibleModel, typeMap, generationScopes);

        List<GeneratedFile> files =
        [
            new(GeneratedNativeHeaderPath, EmitNativeHeader(
                pointConstructor,
                pointDefaultConstructor,
                pointCopyConstructor,
                staticMethods)),
            new(GeneratedNativeSourcePath, EmitNativeSource(
                pointConstructor,
                pointDefaultConstructor,
                pointCopyConstructor,
                staticMethods)),
            new(GeneratedPointManagedPath, EmitPointManagedBinding(
                pointConstructor,
                pointDefaultConstructor,
                pointCopyConstructor)),
        ];
        if (staticMethods.Length > 0)
        {
            files.Add(new GeneratedFile(
                GeneratedStaticManagedPath,
                EmitStaticManagedBindings(staticMethods)));
        }

        List<string> sourceStableIds = [pointConstructor.StableId];
        if (pointDefaultConstructor is not null)
        {
            sourceStableIds.Add(pointDefaultConstructor.StableId);
        }

        if (pointCopyConstructor is not null)
        {
            sourceStableIds.Add(pointCopyConstructor.StableId);
        }

        sourceStableIds.AddRange(staticMethods.Select(static method => method.Declaration.StableId));

        GeneratedBindingSet sharedHandles = SharedHandleBindingEmitter.Emit(
            report.OcctVersion,
            eligibleModel,
            sharedHandleScopes);
        files.AddRange(sharedHandles.Files);
        sourceStableIds.AddRange(sharedHandles.SourceStableIds);
        GeneratedBindingSet topology = TopologyBindingEmitter.Emit(
            report.OcctVersion,
            eligibleModel,
            topologyScopes);
        files.AddRange(topology.Files);
        sourceStableIds.AddRange(topology.SourceStableIds);
        GeneratedBindingSet enums = EnumBindingEmitter.Emit(
            report.OcctVersion,
            eligibleModel,
            sourceStableIds);
        files.AddRange(enums.Files);
        sourceStableIds.AddRange(enums.SourceStableIds);
        return new GeneratedBindingSet(
            report.OcctVersion,
            sourceStableIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            files.OrderBy(static file => file.RelativePath, StringComparer.Ordinal).ToArray());
    }

    private static GeneratedStaticMethod[] SelectStaticMethods(
        BindingModel model,
        InitialTypeMap typeMap,
        IReadOnlyList<GenerationScopeConfiguration> generationScopes)
    {
        ValidateGenerationScopes(generationScopes);
        return generationScopes
            .OrderBy(static scope => scope.SourcePackage, StringComparer.Ordinal)
            .SelectMany(scope => model.Declarations
                .Where(declaration => IsStaticValueCopyMethod(declaration, typeMap, scope))
                .GroupBy(static declaration => declaration.NativeName, StringComparer.Ordinal)
                .OrderBy(static group => group.Key, StringComparer.Ordinal)
                .SelectMany(group => group
                    .OrderBy(static declaration => declaration.NativeSignature, StringComparer.Ordinal)
                    .ThenBy(static declaration => declaration.StableId, StringComparer.Ordinal)
                    .Select((declaration, overloadIndex) => CreateStaticMethod(
                        declaration,
                        overloadIndex,
                        typeMap,
                        scope))))
            .ToArray();
    }

    private static GeneratedStaticMethod CreateStaticMethod(
        BindingDeclaration declaration,
        int overloadIndex,
        InitialTypeMap typeMap,
        GenerationScopeConfiguration scope)
    {
        BindingTypeProjection returnProjection = GetProjection(
            declaration.ReturnType!,
            BindingTypeUsage.ReturnValue,
            typeMap);
        GeneratedParameter[] parameters = declaration.Parameters
            .Select(parameter => new GeneratedParameter(
                parameter.Name,
                parameter.Type,
                GetProjection(parameter.Type, BindingTypeUsage.Parameter, typeMap)))
            .ToArray();
        string memberName = declaration.NativeName[(declaration.NativeName.LastIndexOf("::", StringComparison.Ordinal) + 2)..];
        string normalizedMemberName = ToSnakeCase(memberName);
        return new GeneratedStaticMethod(
            declaration,
            returnProjection,
            parameters,
            $"occtsharp_generated_{scope.ExportNamePrefix}_{normalizedMemberName}_{overloadIndex}",
            $"{scope.ManagedNamePrefix}{memberName}{overloadIndex}",
            scope);
    }

    private static string EmitNativeHeader(
        BindingDeclaration pointConstructor,
        BindingDeclaration? pointDefaultConstructor,
        BindingDeclaration? pointCopyConstructor,
        IReadOnlyList<GeneratedStaticMethod> methods)
    {
        StringBuilder builder = new();
        builder.AppendLine("// <auto-generated />");
        AppendSourceComments(builder, GetPointSourceDeclarations(
            pointConstructor,
            pointDefaultConstructor,
            pointCopyConstructor,
            methods));
        builder.AppendLine("#pragma once");
        builder.AppendLine();
        builder.AppendLine("#include \"../include/OcctSharp.Native.h\"");
        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("extern \"C\" {");
        builder.AppendLine("#endif");
        builder.AppendLine();
        builder.AppendLine("typedef struct OcctSharp_Point3d");
        builder.AppendLine("{");
        builder.AppendLine("  double x;");
        builder.AppendLine("  double y;");
        builder.AppendLine("  double z;");
        builder.AppendLine("} OcctSharp_Point3d;");
        builder.AppendLine();
        builder.AppendLine("OCCTSHARP_API OcctSharp_Point3d OCCTSHARP_CALL occtsharp_generated_gp_pnt_create(");
        builder.AppendLine("  double x,");
        builder.AppendLine("  double y,");
        builder.AppendLine("  double z);");

        if (pointDefaultConstructor is not null)
        {
            builder.AppendLine();
            builder.AppendLine("OCCTSHARP_API OcctSharp_Point3d OCCTSHARP_CALL occtsharp_generated_gp_pnt_default(void);");
        }

        if (pointCopyConstructor is not null)
        {
            builder.AppendLine();
            builder.AppendLine("OCCTSHARP_API OcctSharp_Point3d OCCTSHARP_CALL occtsharp_generated_gp_pnt_copy(");
            builder.AppendLine("  OcctSharp_Point3d point);");
        }

        foreach (GeneratedStaticMethod method in methods)
        {
            builder.AppendLine();
            AppendNativeDeclaration(builder, method);
        }

        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("}");
        builder.AppendLine("#endif");
        return Normalize(builder.ToString());
    }

    private static string EmitNativeSource(
        BindingDeclaration pointConstructor,
        BindingDeclaration? pointDefaultConstructor,
        BindingDeclaration? pointCopyConstructor,
        IReadOnlyList<GeneratedStaticMethod> methods)
    {
        StringBuilder builder = new();
        builder.AppendLine("// <auto-generated />");
        AppendSourceComments(builder, GetPointSourceDeclarations(
            pointConstructor,
            pointDefaultConstructor,
            pointCopyConstructor,
            methods));
        builder.AppendLine("#include \"OcctSharp.Generated.h\"");
        builder.AppendLine();
        builder.AppendLine("#include <gp_Pnt.hxx>");
        foreach (string header in methods
            .Select(static method => method.Scope.Header)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal))
        {
            builder.AppendLine("#include <" + header + ">");
        }

        builder.AppendLine();
        builder.AppendLine("static_assert(sizeof(OcctSharp_Point3d) == 24);");
        builder.AppendLine("static_assert(alignof(OcctSharp_Point3d) == 8);");
        builder.AppendLine();
        builder.AppendLine("OcctSharp_Point3d OCCTSHARP_CALL occtsharp_generated_gp_pnt_create(");
        builder.AppendLine("  const double x,");
        builder.AppendLine("  const double y,");
        builder.AppendLine("  const double z)");
        builder.AppendLine("{");
        builder.AppendLine("  const gp_Pnt value(x, y, z);");
        builder.AppendLine("  return {value.X(), value.Y(), value.Z()};");
        builder.AppendLine("}");

        if (pointDefaultConstructor is not null)
        {
            builder.AppendLine();
            builder.AppendLine("OcctSharp_Point3d OCCTSHARP_CALL occtsharp_generated_gp_pnt_default(void)");
            builder.AppendLine("{");
            builder.AppendLine("  const gp_Pnt value;");
            builder.AppendLine("  return {value.X(), value.Y(), value.Z()};");
            builder.AppendLine("}");
        }

        if (pointCopyConstructor is not null)
        {
            builder.AppendLine();
            builder.AppendLine("OcctSharp_Point3d OCCTSHARP_CALL occtsharp_generated_gp_pnt_copy(");
            builder.AppendLine("  const OcctSharp_Point3d point)");
            builder.AppendLine("{");
            builder.AppendLine("  const gp_Pnt source(point.x, point.y, point.z);");
            builder.AppendLine("  const gp_Pnt value(source);");
            builder.AppendLine("  return {value.X(), value.Y(), value.Z()};");
            builder.AppendLine("}");
        }

        foreach (GeneratedStaticMethod method in methods)
        {
            builder.AppendLine();
            AppendNativeDefinition(builder, method);
        }

        return Normalize(builder.ToString());
    }

    private static string EmitPointManagedBinding(
        BindingDeclaration pointConstructor,
        BindingDeclaration? pointDefaultConstructor,
        BindingDeclaration? pointCopyConstructor)
    {
        StringBuilder builder = new();
        builder.AppendLine("// <auto-generated />");
        AppendSourceComments(builder, GetPointSourceDeclarations(
            pointConstructor,
            pointDefaultConstructor,
            pointCopyConstructor,
            []));
        builder.AppendLine("using System.Runtime.CompilerServices;");
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharp.Generated;");
        builder.AppendLine();
        builder.AppendLine("[StructLayout(LayoutKind.Sequential)]");
        builder.AppendLine("internal readonly struct Point3dRaw");
        builder.AppendLine("{");
        builder.AppendLine("    internal Point3dRaw(double x, double y, double z)");
        builder.AppendLine("    {");
        builder.AppendLine("        X = x;");
        builder.AppendLine("        Y = y;");
        builder.AppendLine("        Z = z;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    internal readonly double X;");
        builder.AppendLine("    internal readonly double Y;");
        builder.AppendLine("    internal readonly double Z;");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("internal static partial class GeneratedNativeMethods");
        builder.AppendLine("{");
        builder.AppendLine("    private const string LibraryName = \"OcctSharp.Native\";");
        builder.AppendLine();
        builder.AppendLine("    static GeneratedNativeMethods()");
        builder.AppendLine("    {");
        builder.AppendLine("        global::OcctSharp.Interop.NativeLibraryResolver.EnsureRegistered();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    [LibraryImport(LibraryName, EntryPoint = \"occtsharp_generated_gp_pnt_create\")]");
        builder.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
        builder.AppendLine("    internal static partial Point3dRaw CreatePoint3d(double x, double y, double z);");

        if (pointDefaultConstructor is not null)
        {
            builder.AppendLine();
            builder.AppendLine("    [LibraryImport(LibraryName, EntryPoint = \"occtsharp_generated_gp_pnt_default\")]");
            builder.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
            builder.AppendLine("    internal static partial Point3dRaw CreatePoint3dDefault();");
        }

        if (pointCopyConstructor is not null)
        {
            builder.AppendLine();
            builder.AppendLine("    [LibraryImport(LibraryName, EntryPoint = \"occtsharp_generated_gp_pnt_copy\")]");
            builder.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
            builder.AppendLine("    internal static partial Point3dRaw CreatePoint3dCopy(Point3dRaw point);");
        }

        builder.AppendLine("}");
        return Normalize(builder.ToString());
    }

    private static string EmitStaticManagedBindings(IReadOnlyList<GeneratedStaticMethod> methods)
    {
        StringBuilder builder = new();
        builder.AppendLine("// <auto-generated />");
        AppendSourceComments(builder, methods.Select(static method => method.Declaration));
        builder.AppendLine("using System.Runtime.CompilerServices;");
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharp.Generated;");
        builder.AppendLine();
        builder.AppendLine("internal static partial class GeneratedNativeMethods");
        builder.AppendLine("{");
        foreach (GeneratedStaticMethod method in methods)
        {
            builder.AppendLine();
            builder.AppendLine("    [LibraryImport(LibraryName, EntryPoint = \"" + method.ExportName + "\")]");
            builder.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
            builder.Append("    internal static partial ");
            builder.Append(method.ReturnProjection.ManagedRawType);
            builder.Append(' ');
            builder.Append(method.ManagedName);
            builder.Append('(');
            builder.Append(string.Join(", ", method.Parameters.Select(static parameter =>
                $"{parameter.Projection.ManagedRawType} {parameter.Name}")));
            builder.AppendLine(");");
        }

        builder.AppendLine("}");
        return Normalize(builder.ToString());
    }

    private static void AppendNativeDeclaration(StringBuilder builder, GeneratedStaticMethod method)
    {
        builder.Append("OCCTSHARP_API ");
        builder.Append(method.ReturnProjection.AbiType);
        builder.Append(" OCCTSHARP_CALL ");
        builder.Append(method.ExportName);
        if (method.Parameters.Length == 0)
        {
            builder.AppendLine("(void);");
            return;
        }

        builder.AppendLine("(");
        for (int index = 0; index < method.Parameters.Length; index++)
        {
            GeneratedParameter parameter = method.Parameters[index];
            builder.Append("  ");
            builder.Append(parameter.Projection.AbiType);
            builder.Append(' ');
            builder.Append(parameter.Name);
            builder.AppendLine(index == method.Parameters.Length - 1 ? ");" : ",");
        }
    }

    private static void AppendNativeDefinition(StringBuilder builder, GeneratedStaticMethod method)
    {
        builder.Append(method.ReturnProjection.AbiType);
        builder.Append(" OCCTSHARP_CALL ");
        builder.Append(method.ExportName);
        if (method.Parameters.Length == 0)
        {
            builder.AppendLine("(void)");
        }
        else
        {
            builder.AppendLine("(");
            for (int index = 0; index < method.Parameters.Length; index++)
            {
                GeneratedParameter parameter = method.Parameters[index];
                builder.Append("  const ");
                builder.Append(parameter.Projection.AbiType);
                builder.Append(' ');
                builder.Append(parameter.Name);
                builder.AppendLine(index == method.Parameters.Length - 1 ? ")" : ",");
            }
        }

        string arguments = string.Join(", ", method.Parameters.Select(RenderNativeArgument));
        string invocation = $"{method.Declaration.NativeName}({arguments})";
        builder.AppendLine("{");
        builder.Append("  return ");
        builder.Append(RenderNativeReturn(invocation, method.ReturnProjection));
        builder.AppendLine(";");
        builder.AppendLine("}");
    }

    private static string RenderNativeArgument(GeneratedParameter parameter) => parameter.Projection.RuleId switch
    {
        "TM003" => $"({parameter.Name} != 0)",
        "TM004" => $"static_cast<{parameter.Type.BaseCanonicalSpelling}>({parameter.Name})",
        _ => parameter.Name,
    };

    private static string RenderNativeReturn(string invocation, BindingTypeProjection projection) => projection.RuleId switch
    {
        "TM003" => $"{invocation} ? 1 : 0",
        "TM004" => $"static_cast<int32_t>({invocation})",
        _ => invocation,
    };

    private static void AppendSourceComments(
        StringBuilder builder,
        IEnumerable<BindingDeclaration> declarations)
    {
        foreach (BindingDeclaration declaration in declarations.OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal))
        {
            builder.AppendLine("// Source: " + declaration.StableId);
        }
    }

    private static bool IsStaticValueCopyMethod(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        GenerationScopeConfiguration scope)
    {
        if (declaration.SupportState != BindingSupportState.Supported
            || declaration.Kind != BindingDeclarationKind.Method
            || !declaration.IsStatic
            || !string.Equals(declaration.SourcePackage, scope.SourcePackage, StringComparison.Ordinal)
            || !declaration.NativeName.StartsWith(scope.NativeNamePrefix, StringComparison.Ordinal)
            || declaration.ReturnType is null
            || !TryGetScalarProjection(declaration.ReturnType, BindingTypeUsage.ReturnValue, typeMap, out _))
        {
            return false;
        }

        return declaration.Parameters.All(parameter =>
            TryGetScalarProjection(parameter.Type, BindingTypeUsage.Parameter, typeMap, out _));
    }

    private static void ValidateGenerationScopes(IReadOnlyList<GenerationScopeConfiguration> scopes)
    {
        if (scopes.Count == 0)
        {
            throw new InvalidDataException("At least one generated static-method scope is required.");
        }

        HashSet<string> identities = new(StringComparer.Ordinal);
        HashSet<string> exports = new(StringComparer.Ordinal);
        foreach (GenerationScopeConfiguration scope in scopes)
        {
            if (string.IsNullOrWhiteSpace(scope.SourcePackage)
                || string.IsNullOrWhiteSpace(scope.NativeNamePrefix)
                || string.IsNullOrWhiteSpace(scope.Header)
                || string.IsNullOrWhiteSpace(scope.ExportNamePrefix)
                || string.IsNullOrWhiteSpace(scope.ManagedNamePrefix))
            {
                throw new InvalidDataException("Every generated static-method scope must define package, prefix, header, and naming fields.");
            }

            string identity = scope.SourcePackage + "\u001f" + scope.NativeNamePrefix;
            if (!identities.Add(identity))
            {
                throw new InvalidDataException(
                    $"Generated static-method scope '{scope.SourcePackage}:{scope.NativeNamePrefix}' is configured more than once.");
            }

            if (!exports.Add(scope.ExportNamePrefix))
            {
                throw new InvalidDataException($"Generated static-method export prefix '{scope.ExportNamePrefix}' is configured more than once.");
            }
        }
    }

    private static bool TryGetScalarProjection(
        BindingType type,
        BindingTypeUsage usage,
        InitialTypeMap typeMap,
        out BindingTypeProjection? projection)
    {
        return typeMap.TryMap(type, usage, out projection)
            && projection is not null
            && projection.Ownership == "ValueCopy"
            && projection.RuleId is "TM001" or "TM002" or "TM003" or "TM004";
    }

    private static BindingTypeProjection GetProjection(
        BindingType type,
        BindingTypeUsage usage,
        InitialTypeMap typeMap)
    {
        if (!TryGetScalarProjection(type, usage, typeMap, out BindingTypeProjection? projection))
        {
            throw new InvalidDataException(
                $"Declaration type '{type.NativeSpelling}' lacks a supported scalar value-copy projection.");
        }

        return projection!;
    }

    private static bool IsPointCoordinateConstructor(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        return declaration.SupportState == BindingSupportState.Supported
            && declaration.Kind == BindingDeclarationKind.Constructor
            && string.Equals(declaration.NativeName, "gp_Pnt::gp_Pnt", StringComparison.Ordinal)
            && declaration.Parameters.Count == 3
            && declaration.Parameters.All(parameter =>
                typeMap.TryMap(
                    parameter.Type,
                    BindingTypeUsage.Parameter,
                    out BindingTypeProjection? projection)
                && string.Equals(projection?.RuleId, "TM002", StringComparison.Ordinal));
    }

    private static bool IsPointDefaultConstructor(BindingDeclaration declaration)
    {
        return declaration.SupportState == BindingSupportState.Supported
            && declaration.Kind == BindingDeclarationKind.Constructor
            && string.Equals(declaration.NativeName, "gp_Pnt::gp_Pnt", StringComparison.Ordinal)
            && declaration.Parameters.Count == 0;
    }

    private static bool IsPointCopyConstructor(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        return declaration.SupportState == BindingSupportState.Supported
            && declaration.Kind == BindingDeclarationKind.Constructor
            && string.Equals(declaration.NativeName, "gp_Pnt::gp_Pnt", StringComparison.Ordinal)
            && declaration.Parameters.Count == 1
            && typeMap.TryMap(
                declaration.Parameters[0].Type,
                BindingTypeUsage.Parameter,
                out BindingTypeProjection? projection)
            && string.Equals(projection?.RuleId, "TM005", StringComparison.Ordinal);
    }

    private static BindingDeclaration[] GetPointSourceDeclarations(
        BindingDeclaration pointConstructor,
        BindingDeclaration? pointDefaultConstructor,
        BindingDeclaration? pointCopyConstructor,
        IReadOnlyList<GeneratedStaticMethod> methods)
    {
        List<BindingDeclaration> declarations = [pointConstructor];
        if (pointDefaultConstructor is not null)
        {
            declarations.Add(pointDefaultConstructor);
        }

        if (pointCopyConstructor is not null)
        {
            declarations.Add(pointCopyConstructor);
        }

        declarations.AddRange(methods.Select(static method => method.Declaration));
        return declarations.ToArray();
    }

    private static string ToSnakeCase(string value)
    {
        StringBuilder builder = new(value.Length + 8);
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            bool startsWord = char.IsUpper(current)
                && index > 0
                && (!char.IsUpper(value[index - 1])
                    || (index + 1 < value.Length && char.IsLower(value[index + 1])));
            if (startsWord)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString();
    }

    private static string Normalize(string value) => value
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .TrimEnd('\n') + "\n";

    private sealed record GeneratedStaticMethod(
        BindingDeclaration Declaration,
        BindingTypeProjection ReturnProjection,
        GeneratedParameter[] Parameters,
        string ExportName,
        string ManagedName,
        GenerationScopeConfiguration Scope);

    private sealed record GeneratedParameter(
        string Name,
        BindingType Type,
        BindingTypeProjection Projection);
}
