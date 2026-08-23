using System.Text;
using System.Globalization;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Emission;

public static class TopologyBindingEmitter
{
    private const string NativeHeaderPath = "src/OcctSharp.Native/generated/Topology/OcctSharp.Topology.Generated.h";
    private const string NativeSourcePath = "src/OcctSharp.Native/generated/Topology/OcctSharp.Topology.Generated.cpp";
    private const string ManagedRawPath = "src/OcctSharp/Generated/Topology/TopologyRaw.Generated.cs";
    private const string ManagedFriendlyPath = "src/OcctSharp/Generated/Topology/Topology.Generated.cs";

    public static GeneratedBindingSet Emit(
        string occtVersion,
        BindingModel model,
        IReadOnlyList<TopologyScopeConfiguration> scopes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occtVersion);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(scopes);

        if (scopes.Count == 0)
        {
            return new GeneratedBindingSet(occtVersion, [], []);
        }

        ValidateScopes(scopes);
        TopologyScopeConfiguration scope = scopes.Single();
        BindingDeclaration[] baseDeclarations = SelectDeclarations(model, scope);
        TypedTopologyBinding[] typedBindings = SelectTypedBindings(model, scope);
        BindingDeclaration[] declarations = baseDeclarations
            .Concat(typedBindings.Select(static binding => binding.Declaration))
            .ToArray();
        return new GeneratedBindingSet(
            occtVersion,
            declarations.Select(static declaration => declaration.StableId)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            [
                new GeneratedFile(NativeHeaderPath, EmitNativeHeader(declarations, scope, typedBindings)),
                new GeneratedFile(NativeSourcePath, EmitNativeSource(declarations, scope, typedBindings)),
                new GeneratedFile(ManagedRawPath, EmitManagedRaw(declarations, typedBindings)),
                new GeneratedFile(ManagedFriendlyPath, EmitManagedFriendly(declarations, typedBindings)),
            ]);
    }

    private sealed record TypedTopologyBinding(
        TopologyTypeConfiguration Configuration,
        BindingDeclaration Declaration);

    private static BindingDeclaration[] SelectDeclarations(
        BindingModel model,
        TopologyScopeConfiguration scope)
    {
        BindingDeclaration[] candidates = model.Declarations
            .Where(declaration => declaration.SourcePackage == scope.SourcePackage
                && declaration.Header == scope.Header
                && (declaration.NativeName == scope.NativeType
                    || declaration.NativeName.StartsWith(scope.NativeType + "::", StringComparison.Ordinal)))
            .ToArray();

        return
        [
            Find(candidates, "copy constructor", declaration =>
                declaration.Kind == BindingDeclarationKind.Constructor
                && declaration.NativeName == "TopoDS_Shape::TopoDS_Shape"
                && declaration.Parameters is [var parameter]
                && IsConstShapeReference(parameter.Type)),
            FindMethod(candidates, "IsNull", 0, "bool"),
            FindMethod(candidates, "ShapeType", 0, "TopAbs_ShapeEnum"),
            FindMethod(candidates, "Orientation", 0, "TopAbs_Orientation"),
            FindMethod(candidates, "Reversed", 0, "TopoDS_Shape"),
            FindShapeComparison(candidates, "IsPartner"),
            FindShapeComparison(candidates, "IsSame"),
            FindShapeComparison(candidates, "IsEqual"),
        ];
    }

    private static TypedTopologyBinding[] SelectTypedBindings(
        BindingModel model,
        TopologyScopeConfiguration scope)
    {
        return scope.TypedTypes
            .Select(configuration => new TypedTopologyBinding(
                configuration,
                Find(model.Declarations, configuration, declaration =>
                    declaration.SourcePackage == scope.SourcePackage
                    && declaration.Header == configuration.Header
                    && declaration.NativeName == configuration.NativeType
                    && declaration.Kind == BindingDeclarationKind.Record)))
            .ToArray();
    }

    private static BindingDeclaration Find(
        IReadOnlyList<BindingDeclaration> declarations,
        TopologyTypeConfiguration configuration,
        Func<BindingDeclaration, bool> predicate)
    {
        BindingDeclaration[] matches = declarations.Where(predicate).ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidDataException(
                $"The configured topology type '{configuration.NativeType}' was not discovered."),
            _ => throw new InvalidDataException(
                $"The configured topology type '{configuration.NativeType}' is ambiguous."),
        };
    }

    private static BindingDeclaration FindMethod(
        BindingDeclaration[] candidates,
        string memberName,
        int parameterCount,
        string returnType) =>
        Find(candidates, memberName, declaration =>
            declaration.Kind == BindingDeclarationKind.Method
            && declaration.NativeName == $"TopoDS_Shape::{memberName}"
            && declaration.Parameters.Count == parameterCount
            && declaration.IsConst
            && declaration.ReturnType is not null
            && IsBaseType(declaration.ReturnType, returnType));

    private static BindingDeclaration FindShapeComparison(
        BindingDeclaration[] candidates,
        string memberName) =>
        Find(candidates, memberName, declaration =>
            declaration.Kind == BindingDeclarationKind.Method
            && declaration.NativeName == $"TopoDS_Shape::{memberName}"
            && declaration.IsConst
            && declaration.ReturnType is not null
            && IsBaseType(declaration.ReturnType, "bool")
            && declaration.Parameters is [var parameter]
            && IsConstShapeReference(parameter.Type));

    private static BindingDeclaration Find(
        BindingDeclaration[] candidates,
        string description,
        Func<BindingDeclaration, bool> predicate)
    {
        BindingDeclaration[] matches = candidates.Where(predicate).ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidDataException($"The configured TopoDS_Shape {description} declaration was not discovered."),
            _ => throw new InvalidDataException($"The configured TopoDS_Shape {description} declaration is ambiguous."),
        };
    }

    private static bool IsConstShapeReference(BindingType type) =>
        IsBaseType(type, "TopoDS_Shape")
        && type.Layers is
        [
            { Kind: BindingTypeLayerKind.LValueReference },
            { Kind: BindingTypeLayerKind.Value, IsConstQualified: true },
        ];

    private static bool IsBaseType(BindingType type, string expected) =>
        string.Equals(Normalize(type.BaseNativeSpelling), expected, StringComparison.Ordinal)
        || string.Equals(Normalize(type.BaseCanonicalSpelling), expected, StringComparison.Ordinal);

    private static string Normalize(string value)
    {
        string result = value.Trim();
        if (result.StartsWith("const ", StringComparison.Ordinal))
        {
            result = result[6..].TrimStart();
        }

        if (result.EndsWith(" const", StringComparison.Ordinal))
        {
            result = result[..^6].TrimEnd();
        }

        return result;
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Identifier cannot be empty.", nameof(value));
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string ToSnakeCase(string value)
    {
        StringBuilder builder = new();
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (char.IsUpper(character) && index > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    private static void ValidateScopes(IReadOnlyList<TopologyScopeConfiguration> scopes)
    {
        if (scopes.Count != 1)
        {
            throw new InvalidDataException("The initial topology emitter requires exactly one TopoDS_Shape scope.");
        }

        TopologyScopeConfiguration scope = scopes[0];
        if (string.IsNullOrWhiteSpace(scope.SourcePackage)
            || string.IsNullOrWhiteSpace(scope.NativeType)
            || string.IsNullOrWhiteSpace(scope.Header)
            || string.IsNullOrWhiteSpace(scope.ExportNamePrefix)
            || string.IsNullOrWhiteSpace(scope.ManagedTypeName))
        {
            throw new InvalidDataException("Topology scope fields cannot be empty.");
        }

        if (scope.SourcePackage != "TopoDS"
            || scope.NativeType != "TopoDS_Shape"
            || scope.Header != "TopoDS_Shape.hxx"
            || scope.ManagedTypeName != "Shape")
        {
            throw new InvalidDataException("The initial topology emitter supports only the TopoDS_Shape/Shape scope.");
        }

        HashSet<string> managedNames = new(StringComparer.Ordinal);
        HashSet<string> nativeNames = new(StringComparer.Ordinal);
        Dictionary<string, string> expectedKinds = new(StringComparer.Ordinal)
        {
            ["TopoDS_Compound"] = "Compound",
            ["TopoDS_CompSolid"] = "CompSolid",
            ["TopoDS_Solid"] = "Solid",
            ["TopoDS_Shell"] = "Shell",
            ["TopoDS_Face"] = "Face",
            ["TopoDS_Wire"] = "Wire",
            ["TopoDS_Edge"] = "Edge",
            ["TopoDS_Vertex"] = "Vertex",
        };
        foreach (TopologyTypeConfiguration type in scope.TypedTypes)
        {
            if (string.IsNullOrWhiteSpace(type.NativeType)
                || string.IsNullOrWhiteSpace(type.Header)
                || string.IsNullOrWhiteSpace(type.ManagedTypeName)
                || string.IsNullOrWhiteSpace(type.ShapeKind))
            {
                throw new InvalidDataException("Typed topology type fields cannot be empty.");
            }

            if (!type.NativeType.StartsWith("TopoDS_", StringComparison.Ordinal)
                || type.ManagedTypeName == scope.ManagedTypeName
                || !managedNames.Add(type.ManagedTypeName)
                || !nativeNames.Add(type.NativeType)
                || !expectedKinds.TryGetValue(type.NativeType, out string? expectedKind)
                || !string.Equals(type.ShapeKind, expectedKind, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"The configured typed topology type '{type.NativeType}' has an invalid or duplicate identity.");
            }
        }
    }

    private static string EmitNativeHeader(
        IReadOnlyList<BindingDeclaration> declarations,
        TopologyScopeConfiguration scope,
        IReadOnlyList<TypedTopologyBinding> typedBindings)
    {
        StringBuilder builder = CreatePreamble(declarations);
        builder.AppendLine("#pragma once");
        builder.AppendLine();
        builder.AppendLine("#include \"../../include/OcctSharp.Native.h\"");
        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("extern \"C\" {");
        builder.AppendLine("#endif");
        builder.AppendLine();
        string prefix = $"occtsharp_generated_{scope.ExportNamePrefix}";
        AppendStatusExport(builder, $"{prefix}_clone", "const OcctSharp_ShapeHandle* source,", "OcctSharp_ShapeHandle** out_shape");
        AppendStatusExport(builder, $"{prefix}_is_null", "const OcctSharp_ShapeHandle* shape,", "int32_t* out_value");
        AppendStatusExport(builder, $"{prefix}_shape_type", "const OcctSharp_ShapeHandle* shape,", "int32_t* out_value");
        AppendStatusExport(builder, $"{prefix}_orientation", "const OcctSharp_ShapeHandle* shape,", "int32_t* out_value");
        AppendStatusExport(builder, $"{prefix}_reversed", "const OcctSharp_ShapeHandle* shape,", "OcctSharp_ShapeHandle** out_shape");
        AppendStatusExport(builder, $"{prefix}_is_partner", "const OcctSharp_ShapeHandle* left,", "const OcctSharp_ShapeHandle* right,", "int32_t* out_value");
        AppendStatusExport(builder, $"{prefix}_is_same", "const OcctSharp_ShapeHandle* left,", "const OcctSharp_ShapeHandle* right,", "int32_t* out_value");
        AppendStatusExport(builder, $"{prefix}_is_equal", "const OcctSharp_ShapeHandle* left,", "const OcctSharp_ShapeHandle* right,", "int32_t* out_value");
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            AppendStatusExport(
                builder,
                $"{prefix}_cast_{ToSnakeCase(typed.Configuration.ManagedTypeName)}",
                "const OcctSharp_ShapeHandle* source,",
                "OcctSharp_ShapeHandle** out_shape");
        }
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("}");
        builder.AppendLine("#endif");
        return builder.ToString();
    }

    private static void AppendStatusExport(StringBuilder builder, string name, params string[] parameters)
    {
        builder.AppendLine(CultureInfo.InvariantCulture, $"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL {name}(");
        for (int index = 0; index < parameters.Length; index++)
        {
            string suffix = index == parameters.Length - 1 ? ");" : string.Empty;
            builder.AppendLine(CultureInfo.InvariantCulture, $"  {parameters[index]}{suffix}");
        }

        builder.AppendLine();
    }

    private static string EmitNativeSource(
        IReadOnlyList<BindingDeclaration> declarations,
        TopologyScopeConfiguration scope,
        IReadOnlyList<TypedTopologyBinding> typedBindings)
    {
        StringBuilder builder = CreatePreamble(declarations);
        builder.AppendLine("#include \"OcctSharp.Topology.Generated.h\"");
        builder.AppendLine("#include \"../../include/OcctSharp.Native.Internal.hxx\"");
        builder.AppendLine();
        builder.AppendLine("#include <Standard_Failure.hxx>");
        builder.AppendLine("#include <Standard_TypeMismatch.hxx>");
        builder.AppendLine("#include <TopAbs_Orientation.hxx>");
        builder.AppendLine("#include <TopAbs_ShapeEnum.hxx>");
        builder.AppendLine("#include <TopoDS.hxx>");
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"#include <{typed.Configuration.Header}>");
        }
        builder.AppendLine("#include <TopoDS_Shape.hxx>");
        builder.AppendLine();
        builder.AppendLine("#include <exception>");
        builder.AppendLine("#include <stdexcept>");
        builder.AppendLine();
        builder.AppendLine("static_assert(TopAbs_COMPOUND == 0 && TopAbs_SHAPE == 8);");
        builder.AppendLine("static_assert(TopAbs_FORWARD == 0 && TopAbs_EXTERNAL == 3);");
        builder.AppendLine();
        builder.AppendLine("namespace");
        builder.AppendLine("{");
        builder.AppendLine("template <typename TAction>");
        builder.AppendLine("OcctSharp_Status GuardTopology(TAction&& action)");
        builder.AppendLine("{");
        builder.AppendLine("  try");
        builder.AppendLine("  {");
        builder.AppendLine("    action();");
        builder.AppendLine("    return OCCTSHARP_STATUS_SUCCESS;");
        builder.AppendLine("  }");
        builder.AppendLine("  catch (const Standard_TypeMismatch& error)");
        builder.AppendLine("  {");
        builder.AppendLine("    OcctSharp_Internal_SetLastError(error.GetMessageString());");
        builder.AppendLine("    return OCCTSHARP_STATUS_TYPE_MISMATCH;");
        builder.AppendLine("  }");
        builder.AppendLine("  catch (const Standard_Failure& error)");
        builder.AppendLine("  {");
        builder.AppendLine("    OcctSharp_Internal_SetLastError(error.GetMessageString());");
        builder.AppendLine("    return OCCTSHARP_STATUS_OCCT_FAILURE;");
        builder.AppendLine("  }");
        builder.AppendLine("  catch (const std::exception& error)");
        builder.AppendLine("  {");
        builder.AppendLine("    OcctSharp_Internal_SetLastError(error.what());");
        builder.AppendLine("    return OCCTSHARP_STATUS_STANDARD_EXCEPTION;");
        builder.AppendLine("  }");
        builder.AppendLine("  catch (...)");
        builder.AppendLine("  {");
        builder.AppendLine("    OcctSharp_Internal_SetLastError(\"Unknown C++ exception.\");");
        builder.AppendLine("    return OCCTSHARP_STATUS_UNKNOWN_EXCEPTION;");
        builder.AppendLine("  }");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("OcctSharp_Status ValidateOutput(const void* output)");
        builder.AppendLine("{");
        builder.AppendLine("  if (output != nullptr)");
        builder.AppendLine("  {");
        builder.AppendLine("    return OCCTSHARP_STATUS_SUCCESS;");
        builder.AppendLine("  }");
        builder.AppendLine("  OcctSharp_Internal_SetLastError(\"The topology output pointer is null.\");");
        builder.AppendLine("  return OCCTSHARP_STATUS_INVALID_ARGUMENT;");
        builder.AppendLine("}");
        builder.AppendLine("}");
        builder.AppendLine();

        string prefix = $"occtsharp_generated_{scope.ExportNamePrefix}";
        AppendUnaryShapeOutput(builder, $"{prefix}_clone", "source", "TopoDS_Shape(*value)");
        AppendScalarOutput(builder, $"{prefix}_is_null", "shape", "value->IsNull() ? 1 : 0");
        AppendScalarOutput(builder, $"{prefix}_shape_type", "shape", "static_cast<int32_t>(value->ShapeType())");
        AppendScalarOutput(builder, $"{prefix}_orientation", "shape", "static_cast<int32_t>(value->Orientation())");
        AppendUnaryShapeOutput(builder, $"{prefix}_reversed", "shape", "value->Reversed()");
        AppendComparison(builder, $"{prefix}_is_partner", "IsPartner");
        AppendComparison(builder, $"{prefix}_is_same", "IsSame");
        AppendComparison(builder, $"{prefix}_is_equal", "IsEqual");
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            AppendTypedCast(
                builder,
                $"{prefix}_cast_{ToSnakeCase(typed.Configuration.ManagedTypeName)}",
                typed.Configuration.NativeType);
        }
        return builder.ToString().TrimEnd() + "\n";
    }

    private static void AppendTypedCast(StringBuilder builder, string name, string nativeType)
    {
        string functionName = nativeType["TopoDS_".Length..];
        builder.AppendLine(CultureInfo.InvariantCulture, $"OcctSharp_Status OCCTSHARP_CALL {name}(");
        builder.AppendLine("  const OcctSharp_ShapeHandle* source,");
        builder.AppendLine("  OcctSharp_ShapeHandle** out_shape)");
        builder.AppendLine("{");
        builder.AppendLine("  if (OcctSharp_Status status = ValidateOutput(out_shape); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  *out_shape = nullptr;");
        builder.AppendLine("  const TopoDS_Shape* value = nullptr;");
        builder.AppendLine("  if (OcctSharp_Status status = OcctSharp_Internal_TryGetShape(source, &value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  return GuardTopology([&]()");
        builder.AppendLine("  {");
        builder.AppendLine(CultureInfo.InvariantCulture, $"    *out_shape = OcctSharp_Internal_AllocateShape(TopoDS::{functionName}(*value));");
        builder.AppendLine("  });");
        builder.AppendLine("}");
        builder.AppendLine();
    }

    private static void AppendUnaryShapeOutput(
        StringBuilder builder,
        string name,
        string parameterName,
        string expression)
    {
        builder.AppendLine(CultureInfo.InvariantCulture, $"OcctSharp_Status OCCTSHARP_CALL {name}(");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  const OcctSharp_ShapeHandle* {parameterName},");
        builder.AppendLine("  OcctSharp_ShapeHandle** out_shape)");
        builder.AppendLine("{");
        builder.AppendLine("  if (OcctSharp_Status status = ValidateOutput(out_shape); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  *out_shape = nullptr;");
        builder.AppendLine("  const TopoDS_Shape* value = nullptr;");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  if (OcctSharp_Status status = OcctSharp_Internal_TryGetShape({parameterName}, &value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  return GuardTopology([&]()");
        builder.AppendLine("  {");
        builder.AppendLine(CultureInfo.InvariantCulture, $"    *out_shape = OcctSharp_Internal_AllocateShape({expression});");
        builder.AppendLine("  });");
        builder.AppendLine("}");
        builder.AppendLine();
    }

    private static void AppendScalarOutput(
        StringBuilder builder,
        string name,
        string parameterName,
        string expression)
    {
        builder.AppendLine(CultureInfo.InvariantCulture, $"OcctSharp_Status OCCTSHARP_CALL {name}(");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  const OcctSharp_ShapeHandle* {parameterName},");
        builder.AppendLine("  int32_t* out_value)");
        builder.AppendLine("{");
        builder.AppendLine("  if (OcctSharp_Status status = ValidateOutput(out_value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  const TopoDS_Shape* value = nullptr;");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  if (OcctSharp_Status status = OcctSharp_Internal_TryGetShape({parameterName}, &value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  *out_value = {expression};");
        builder.AppendLine("  return OCCTSHARP_STATUS_SUCCESS;");
        builder.AppendLine("}");
        builder.AppendLine();
    }

    private static void AppendComparison(StringBuilder builder, string name, string method)
    {
        builder.AppendLine(CultureInfo.InvariantCulture, $"OcctSharp_Status OCCTSHARP_CALL {name}(");
        builder.AppendLine("  const OcctSharp_ShapeHandle* left,");
        builder.AppendLine("  const OcctSharp_ShapeHandle* right,");
        builder.AppendLine("  int32_t* out_value)");
        builder.AppendLine("{");
        builder.AppendLine("  if (OcctSharp_Status status = ValidateOutput(out_value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  const TopoDS_Shape* left_value = nullptr;");
        builder.AppendLine("  const TopoDS_Shape* right_value = nullptr;");
        builder.AppendLine("  if (OcctSharp_Status status = OcctSharp_Internal_TryGetShape(left, &left_value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine("  if (OcctSharp_Status status = OcctSharp_Internal_TryGetShape(right, &right_value); status != OCCTSHARP_STATUS_SUCCESS)");
        builder.AppendLine("  {");
        builder.AppendLine("    return status;");
        builder.AppendLine("  }");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  *out_value = left_value->{method}(*right_value) ? 1 : 0;");
        builder.AppendLine("  return OCCTSHARP_STATUS_SUCCESS;");
        builder.AppendLine("}");
        builder.AppendLine();
    }

    private static string EmitManagedRaw(
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlyList<TypedTopologyBinding> typedBindings)
    {
        StringBuilder builder = CreatePreamble(declarations);
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.Append(
            """
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using OcctSharp.Interop;

            namespace OcctSharp.Generated.Topology;

            internal static partial class TopologyNativeMethods
            {
                private const string LibraryName = "OcctSharp.Native";

                static TopologyNativeMethods() => NativeLibraryResolver.EnsureRegistered();

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_clone")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus Clone(ShapeHandle source, out nint shape);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_is_null")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus IsNull(ShapeHandle shape, out int value);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_shape_type")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus ShapeType(ShapeHandle shape, out int value);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_orientation")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus Orientation(ShapeHandle shape, out int value);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_reversed")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus Reversed(ShapeHandle shape, out nint result);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_is_partner")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus IsPartner(ShapeHandle left, ShapeHandle right, out int value);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_is_same")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
                internal static partial NativeStatus IsSame(ShapeHandle left, ShapeHandle right, out int value);

                [LibraryImport(LibraryName, EntryPoint = "occtsharp_generated_topods_shape_is_equal")]
                [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            internal static partial NativeStatus IsEqual(ShapeHandle left, ShapeHandle right, out int value);
            """);
        builder.AppendLine();
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            string methodName = ToPascalCase(typed.Configuration.ManagedTypeName);
            string exportName = $"occtsharp_generated_topods_shape_cast_{ToSnakeCase(typed.Configuration.ManagedTypeName)}";
            builder.AppendLine(CultureInfo.InvariantCulture, $"    [LibraryImport(LibraryName, EntryPoint = \"{exportName}\")]\n    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]\n    internal static partial NativeStatus Cast{methodName}(ShapeHandle source, out nint shape);\n");
        }
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string EmitManagedFriendly(
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlyList<TypedTopologyBinding> typedBindings)
    {
        StringBuilder builder = CreatePreamble(declarations);
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.Append(
            """
            using OcctSharp.Generated.Topology;
            using OcctSharp.Interop;

            namespace OcctSharp;

            /// <summary>Identifies the OCCT topological kind from complex to simple.</summary>
            public enum ShapeKind
            {
                /// <summary>A group containing any topological shapes.</summary>
                Compound = 0,
                /// <summary>A connected set of solids.</summary>
                CompSolid = 1,
                /// <summary>A bounded three-dimensional region.</summary>
                Solid = 2,
                /// <summary>A connected set of faces.</summary>
                Shell = 3,
                /// <summary>A bounded surface region.</summary>
                Face = 4,
                /// <summary>A connected sequence of edges.</summary>
                Wire = 5,
                /// <summary>A bounded curve.</summary>
                Edge = 6,
                /// <summary>A zero-dimensional point topology.</summary>
                Vertex = 7,
                /// <summary>The generic topological shape category.</summary>
                Shape = 8,
            }

            /// <summary>Identifies the orientation of a topological shape.</summary>
            public enum ShapeOrientation
            {
                /// <summary>The default direction.</summary>
                Forward = 0,
                /// <summary>The opposite direction.</summary>
                Reversed = 1,
                /// <summary>An internal boundary relation.</summary>
                Internal = 2,
                /// <summary>An external boundary relation.</summary>
                External = 3,
            }

            public partial class Shape
            {
                /// <summary>Gets whether the wrapped OCCT topology value is null.</summary>
                public bool IsNull
                {
                    get
                    {
                        ThrowIfDisposed();
                        NativeError.ThrowIfFailed(TopologyNativeMethods.IsNull(handle, out int value), "topods_shape_is_null");
                        return value != 0;
                    }
                }

                /// <summary>Gets the OCCT topological kind.</summary>
                public ShapeKind Kind
                {
                    get
                    {
                        ThrowIfDisposed();
                        NativeError.ThrowIfFailed(TopologyNativeMethods.ShapeType(handle, out int value), "topods_shape_shape_type");
                        return (ShapeKind)value;
                    }
                }

                /// <summary>Gets this topology value's orientation.</summary>
                public ShapeOrientation Orientation
                {
                    get
                    {
                        ThrowIfDisposed();
                        NativeError.ThrowIfFailed(TopologyNativeMethods.Orientation(handle, out int value), "topods_shape_orientation");
                        return (ShapeOrientation)value;
                    }
                }

                /// <summary>Copies the TopoDS value while retaining OCCT's shared TShape.</summary>
                public Shape Clone()
                {
                    ThrowIfDisposed();
                    NativeError.ThrowIfFailed(TopologyNativeMethods.Clone(handle, out nint result), "topods_shape_clone");
                    return ShapeFactory.FromNativeHandle(result, "topods_shape_clone");
                }

                /// <summary>Returns an independently owned TopoDS value with reversed orientation.</summary>
                public Shape Reversed()
                {
                    ThrowIfDisposed();
                    NativeError.ThrowIfFailed(TopologyNativeMethods.Reversed(handle, out nint result), "topods_shape_reversed");
                    return ShapeFactory.FromNativeHandle(result, "topods_shape_reversed");
                }

                /// <summary>Tests whether two values share the same underlying TShape.</summary>
                public bool IsPartner(Shape other) => Compare(other, TopologyNativeMethods.IsPartner, "topods_shape_is_partner");

                /// <summary>Tests whether two values share TShape and location, ignoring orientation.</summary>
                public bool IsSame(Shape other) => Compare(other, TopologyNativeMethods.IsSame, "topods_shape_is_same");

                /// <summary>Tests whether two values share TShape, location, and orientation.</summary>
                public bool IsEqual(Shape other) => Compare(other, TopologyNativeMethods.IsEqual, "topods_shape_is_equal");

                private delegate NativeStatus ShapeComparison(ShapeHandle left, ShapeHandle right, out int value);

                private bool Compare(Shape other, ShapeComparison comparison, string operation)
                {
                    ArgumentNullException.ThrowIfNull(other);
                    ThrowIfDisposed();
                    other.ThrowIfDisposed();
                    NativeError.ThrowIfFailed(comparison(handle, other.handle, out int value), operation);
                    return value != 0;
                }

                internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            }
            """);
        builder.AppendLine();
        builder.AppendLine("public partial class Shape");
        builder.AppendLine("{");
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            string managedName = ToPascalCase(typed.Configuration.ManagedTypeName);
            builder.AppendLine(CultureInfo.InvariantCulture, $"    /// <summary>Casts this value to {managedName} after validating its OCCT shape kind.</summary>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    public {managedName} Cast{managedName}() => TopologyTypedCasting.Cast{managedName}(this);");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    /// <summary>Attempts a checked cast to {managedName}; a mismatched non-null kind returns false.</summary>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    public bool TryCast{managedName}(out {managedName}? result) => TopologyTypedCasting.TryCast{managedName}(this, out result);");
            builder.AppendLine();
        }
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("internal static class TopologyTypedCasting");
        builder.AppendLine("{");
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            string managedName = ToPascalCase(typed.Configuration.ManagedTypeName);
            string methodName = ToSnakeCase(typed.Configuration.ManagedTypeName);
            builder.AppendLine(CultureInfo.InvariantCulture, $"    internal static {managedName} Cast{managedName}(Shape source)");
            builder.AppendLine("    {");
            builder.AppendLine("        source.ThrowIfDisposed();");
            builder.AppendLine(CultureInfo.InvariantCulture, $"        NativeError.ThrowIfFailed(TopologyNativeMethods.Cast{managedName}(source.Handle, out nint result), \"topods_shape_cast_{methodName}\");");
            builder.AppendLine(CultureInfo.InvariantCulture, $"        return {managedName}.FromNativeHandle(result, \"topods_shape_cast_{methodName}\");");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine(CultureInfo.InvariantCulture, $"    internal static bool TryCast{managedName}(Shape source, out {managedName}? result)");
            builder.AppendLine("    {");
            builder.AppendLine("        source.ThrowIfDisposed();");
            builder.AppendLine(CultureInfo.InvariantCulture, $"        NativeStatus status = TopologyNativeMethods.Cast{managedName}(source.Handle, out nint nativeResult);");
            builder.AppendLine("        if (status == NativeStatus.TypeMismatch)");
            builder.AppendLine("        {");
            builder.AppendLine("            result = null;");
            builder.AppendLine("            return false;");
            builder.AppendLine("        }");
            builder.AppendLine(CultureInfo.InvariantCulture, $"        NativeError.ThrowIfFailed(status, \"topods_shape_cast_{methodName}\");");
            builder.AppendLine(CultureInfo.InvariantCulture, $"        result = {managedName}.FromNativeHandle(nativeResult, \"topods_shape_cast_{methodName}\");");
            builder.AppendLine("        return true;");
            builder.AppendLine("    }");
            builder.AppendLine();
        }
        builder.AppendLine("}");
        builder.AppendLine();
        foreach (TypedTopologyBinding typed in typedBindings)
        {
            string managedName = ToPascalCase(typed.Configuration.ManagedTypeName);
            builder.AppendLine(CultureInfo.InvariantCulture, $"/// <summary>Represents an OCCT {managedName} topology value.</summary>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"public sealed class {managedName} : Shape");
            builder.AppendLine("{");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    internal {managedName}(ShapeHandle handle) : base(handle) {{ }}");
            builder.AppendLine();
            builder.AppendLine(CultureInfo.InvariantCulture, $"    internal static {managedName} FromNativeHandle(nint nativeHandle, string operation)");
            builder.AppendLine("    {");
            builder.AppendLine("        if (nativeHandle == 0)");
            builder.AppendLine("        {");
            builder.AppendLine(CultureInfo.InvariantCulture, $"            throw new OcctException(NativeStatus.UnknownException.ToString(), $\"The native bridge returned a null {managedName} handle for '{{operation}}'.\");");
            builder.AppendLine("        }");
            builder.AppendLine(CultureInfo.InvariantCulture, $"        return new {managedName}(new ShapeHandle(nativeHandle));");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine();
        }
        return new StringBuilder(builder.ToString().TrimEnd()).AppendLine().ToString();
    }

    private static StringBuilder CreatePreamble(IEnumerable<BindingDeclaration> declarations)
    {
        StringBuilder builder = new();
        builder.AppendLine("// <auto-generated />");
        foreach (BindingDeclaration declaration in declarations.OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"// Source: {declaration.StableId}");
        }

        return builder;
    }
}
