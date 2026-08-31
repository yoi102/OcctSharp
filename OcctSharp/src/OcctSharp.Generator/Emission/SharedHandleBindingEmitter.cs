using System.Globalization;
using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

#pragma warning disable CA1305 // Interpolated values are normalized identifiers and signatures, not locale-sensitive data.

namespace OcctSharp.Generator.Emission;

public static class SharedHandleBindingEmitter
{
    public static GeneratedBindingSet Emit(
        string occtVersion,
        BindingModel model,
        IReadOnlyList<SharedHandleScopeConfiguration> scopes,
        IReadOnlyList<string>? preambleHeaders = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occtVersion);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(scopes);
        preambleHeaders ??= [];
        ValidatePreambleHeaders(preambleHeaders);

        if (scopes.Count == 0)
        {
            return new GeneratedBindingSet(occtVersion, [], []);
        }

        ValidateScopes(scopes);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType = scopes
            .ToDictionary(static scope => scope.NativeType, StringComparer.Ordinal);
        SharedTypeBinding[] bindings = scopes
            .OrderBy(static scope => scope.NativeType, StringComparer.Ordinal)
            .Select(scope => CreateBinding(model, typeMap, scope, scopeByNativeType))
            .ToArray();
        BindingDeclaration[] declarations = bindings
            .SelectMany(static binding => binding.Constructors
                .Select(static constructor => constructor.Declaration)
                .Concat(binding.Methods.Select(static method => method.Declaration)))
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .ToArray();

        SharedTypeBinding[] assignedBindings = bindings
            .Select(binding => binding with { ProductModule = GetProductModule(model, binding.Scope) })
            .ToArray();
        List<GeneratedFile> files =
        [
            new GeneratedFile(
                "src/OcctSharp.Native/generated/Runtime/OcctSharp.Runtime.SharedSupport.Generated.hxx",
                EmitNativeSupportHeader(),
                OcctProductModule.Runtime,
                GeneratedApiLayer.Runtime,
                "SharedHandles.NativeSupport"),
        ];
        foreach (IGrouping<OcctProductModule, SharedTypeBinding> moduleGroup in assignedBindings
            .GroupBy(static binding => binding.ProductModule)
            .OrderBy(static group => group.Key))
        {
            OcctProductModule module = moduleGroup.Key;
            SharedTypeBinding[] moduleBindings = moduleGroup
                .OrderBy(static binding => binding.Scope.NativeType, StringComparer.Ordinal)
                .ToArray();
            BindingDeclaration[] moduleDeclarations = GetDeclarations(moduleBindings);
            SharedHandleScopeConfiguration[] referencedScopes = GetReferencedScopes(moduleBindings);
            OcctProductModule[] referencedModules = referencedScopes
                .Select(scope => assignedBindings.Single(binding =>
                    string.Equals(binding.Scope.NativeType, scope.NativeType, StringComparison.Ordinal)).ProductModule)
                .Distinct()
                .Order()
                .ToArray();
            string moduleName = module.ToString();
            string nativeBase = $"src/OcctSharp.Native/generated/{moduleName}/OcctSharp.{moduleName}.SharedHandles.Generated";
            string managedBase = $"src/OcctSharp/Generated/{moduleName}/{moduleName}.SharedHandles";
            files.Add(new GeneratedFile(
                nativeBase + ".h",
                EmitNativeHeader(moduleBindings, moduleDeclarations, referencedScopes),
                module,
                GeneratedApiLayer.Raw,
                "SharedHandles.NativeHeader"));
            files.Add(new GeneratedFile(
                nativeBase + ".cpp",
                EmitNativeSource(moduleBindings, moduleDeclarations, referencedScopes, referencedModules, preambleHeaders, moduleName),
                module,
                GeneratedApiLayer.Raw,
                "SharedHandles.NativeSource"));
            files.Add(new GeneratedFile(
                managedBase + "Raw.Generated.cs",
                UseModuleNativeMethods(EmitManagedRaw(moduleBindings, moduleDeclarations), module),
                module,
                GeneratedApiLayer.Raw,
                "SharedHandles.ManagedRaw"));
            files.Add(new GeneratedFile(
                managedBase + ".Generated.cs",
                UseModuleNativeMethods(
                    EmitManagedFriendly(
                        moduleBindings,
                        moduleDeclarations,
                        includePointValue: module == OcctProductModule.Geometry),
                    module),
                module,
                GeneratedApiLayer.SafeManaged,
                "SharedHandles.ManagedFriendly"));
        }

        return new GeneratedBindingSet(
            occtVersion,
            declarations.Select(static declaration => declaration.StableId).ToArray(),
            files.OrderBy(static file => file.RelativePath, StringComparer.Ordinal).ToArray());
    }

    private static string UseModuleNativeMethods(string source, OcctProductModule module) =>
        source.Replace(
            "GeneratedNativeMethods",
            module + "GeneratedNativeMethods",
            StringComparison.Ordinal);

    private static BindingDeclaration[] GetDeclarations(IEnumerable<SharedTypeBinding> bindings) => bindings
        .SelectMany(static binding => binding.Constructors
            .Select(static constructor => constructor.Declaration)
            .Concat(binding.Methods.Select(static method => method.Declaration)))
        .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
        .ToArray();

    private static SharedHandleScopeConfiguration[] GetReferencedScopes(IEnumerable<SharedTypeBinding> bindings) => bindings
        .SelectMany(static binding =>
            binding.Constructors.SelectMany(static constructor => constructor.Parameters)
                .Concat(binding.Methods.SelectMany(static method => method.Parameters))
                .Select(static parameter => parameter.SharedScope)
                .Concat(binding.Methods.Select(static method => method.ReturnSharedScope))
                .Append(binding.Scope))
        .Where(static scope => scope is not null)
        .Cast<SharedHandleScopeConfiguration>()
        .DistinctBy(static scope => scope.NativeType, StringComparer.Ordinal)
        .OrderBy(static scope => scope.NativeType, StringComparer.Ordinal)
        .ToArray();

    private static OcctProductModule GetProductModule(
        BindingModel model,
        SharedHandleScopeConfiguration scope)
    {
        BindingDeclaration? declaration = model.Declarations.FirstOrDefault(item =>
            string.Equals(item.SourcePackage, scope.SourcePackage, StringComparison.Ordinal)
            && (string.Equals(item.NativeName, scope.NativeType, StringComparison.Ordinal)
                || item.NativeName.StartsWith(scope.NativeType + "::", StringComparison.Ordinal)));
        return declaration?.ProductModule is not null and not OcctProductModule.Unassigned
            ? declaration.ProductModule
            : OcctProductModuleClassifier.ClassifyOrThrow(scope.SourcePackage, declaration?.SourceToolkit);
    }

    private static string EmitNativeSupportHeader() => Normalize(
        """
        // <auto-generated />
        #pragma once

        #include "../../include/OcctSharp.Native.h"
        #include <stdexcept>

        namespace OcctSharpGenerated
        {
        class GeneratedOperationFailure final : public std::runtime_error
        {
        public:
          GeneratedOperationFailure(const OcctSharp_Status status, const char* message)
            : std::runtime_error(message), Status(status) {}
          OcctSharp_Status Status;
        };
        }
        """);

    private static SharedTypeBinding CreateBinding(
        BindingModel model,
        InitialTypeMap typeMap,
        SharedHandleScopeConfiguration scope,
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType)
    {
        string constructorName = scope.NativeType + "::" + GetUnqualifiedName(scope.NativeType);
        SharedConstructor[] constructors = scope.SuppressConstructors
            ? []
            : model.Declarations
            .Where(declaration => declaration.SupportState == BindingSupportState.Supported
                && declaration.Kind == BindingDeclarationKind.Constructor
                && string.Equals(declaration.SourcePackage, scope.SourcePackage, StringComparison.Ordinal)
                && string.Equals(declaration.NativeName, constructorName, StringComparison.Ordinal)
                && !scope.ExcludedStableIds.Contains(declaration.StableId, StringComparer.Ordinal)
                && declaration.Parameters.All(parameter => IsSupportedParameter(
                    parameter.Type, typeMap, scopeByNativeType)))
            .OrderBy(static declaration => declaration.NativeSignature, StringComparer.Ordinal)
            .ThenBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .Select((declaration, index) => CreateConstructor(
                declaration,
                index,
                scope,
                typeMap,
                scopeByNativeType))
            .ToArray();

        if (constructors.Length == 0 && !scope.SuppressConstructors)
        {
            throw new InvalidDataException(
                $"Shared-handle scope '{scope.NativeType}' has no supported public constructor.");
        }

        SharedMethod[] methods = model.Declarations
            .Where(declaration => IsSupportedMethod(
                declaration, scope, typeMap, scopeByNativeType))
            .GroupBy(
                static declaration => ToSnakeCase(GetMemberName(declaration.NativeName)),
                StringComparer.Ordinal)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .SelectMany(group => group
                .OrderBy(static declaration => declaration.NativeName, StringComparer.Ordinal)
                .ThenBy(static declaration => declaration.NativeSignature, StringComparer.Ordinal)
                .ThenBy(static declaration => declaration.StableId, StringComparer.Ordinal)
                .Select((declaration, index) =>
                {
                    bool returnsVoid = IsVoid(declaration.ReturnType!);
                    BindingTypeProjection? returnProjection = returnsVoid
                        ? null
                        : TryValueProjection(declaration.ReturnType!, BindingTypeUsage.ReturnValue, typeMap, out BindingTypeProjection? projection)
                            ? projection
                            : null;
                    SharedHandleScopeConfiguration? returnSharedScope = returnsVoid || returnProjection is not null
                        ? null
                        : GetSharedScope(declaration.ReturnType!, scopeByNativeType);
                    string memberName = GetMemberName(declaration.NativeName);
                    return new SharedMethod(
                        declaration,
                        CreateParameters(declaration.Parameters, typeMap, scopeByNativeType),
                        returnProjection,
                        returnSharedScope,
                        returnsVoid,
                        index,
                        $"occtsharp_generated_{scope.ExportNamePrefix}_method_{ToSnakeCase(memberName)}_{index}",
                        $"{scope.ManagedTypeName}Method{ToManagedMemberName(memberName)}{index}",
                        memberName,
                        ToManagedMemberName(memberName));
                }))
            .ToArray();

        methods = AssignUniqueManagedMemberNames(methods);

        return new SharedTypeBinding(scope, constructors, methods, OcctProductModule.Unassigned);
    }

    private static SharedConstructor CreateConstructor(
        BindingDeclaration declaration,
        int index,
        SharedHandleScopeConfiguration scope,
        InitialTypeMap typeMap,
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType)
    {
        GeneratedParameter[] parameters = CreateParameters(
            declaration.Parameters, typeMap, scopeByNativeType);
        GeneratedParameter? placementAllocator = null;
        if (scope.UsesPlacementAllocator)
        {
            GeneratedParameter[] candidates = parameters
                .Where(static parameter => string.Equals(
                    parameter.SharedScope?.NativeType,
                    "NCollection_IncAllocator",
                    StringComparison.Ordinal))
                .ToArray();
            if (candidates.Length != 1)
            {
                throw new InvalidDataException(
                    $"Placement-allocated shared type '{scope.NativeType}' constructor '{declaration.StableId}' must have exactly one generated NCollection_IncAllocator handle parameter.");
            }
            placementAllocator = candidates[0];
        }

        return new SharedConstructor(
            declaration,
            parameters,
            placementAllocator,
            index,
            $"occtsharp_generated_{scope.ExportNamePrefix}_create_{index}",
            $"{scope.ManagedTypeName}Create{index}");
    }

    private static bool IsSupportedMethod(
        BindingDeclaration declaration,
        SharedHandleScopeConfiguration scope,
        InitialTypeMap typeMap,
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType)
    {
        if (declaration.SupportState != BindingSupportState.Supported
            || declaration.Kind != BindingDeclarationKind.Method
            || declaration.IsStatic
            || declaration.IsPureVirtual
            || declaration.IsOverloadedOperator
            || !declaration.NativeName.StartsWith(scope.NativeType + "::", StringComparison.Ordinal)
            || declaration.NativeName.Contains("::~", StringComparison.Ordinal)
            || declaration.ReturnType is null
            || scope.ExcludedStableIds.Contains(declaration.StableId, StringComparer.Ordinal)
            || (!IsVoid(declaration.ReturnType)
                && !TryValueProjection(declaration.ReturnType, BindingTypeUsage.ReturnValue, typeMap, out _)
                && GetSharedScope(declaration.ReturnType, scopeByNativeType) is null))
        {
            return false;
        }

        return declaration.Parameters.All(parameter => IsSupportedParameter(
            parameter.Type, typeMap, scopeByNativeType));
    }

    private static GeneratedParameter[] CreateParameters(
        IReadOnlyList<BindingParameter> parameters,
        InitialTypeMap typeMap,
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType) => parameters
        .Select(parameter =>
        {
            BindingTypeProjection? projection = TryValueProjection(
                parameter.Type, BindingTypeUsage.Parameter, typeMap, out BindingTypeProjection? valueProjection)
                ? valueProjection
                : null;
            return new GeneratedParameter(
                parameter,
                ToParameterName(parameter.Name, parameter.Position),
                projection,
                projection is null ? GetSharedScope(parameter.Type, scopeByNativeType) : null);
        })
        .ToArray();

    private static bool IsSupportedParameter(
        BindingType type,
        InitialTypeMap typeMap,
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType) =>
        TryValueProjection(type, BindingTypeUsage.Parameter, typeMap, out _)
        || GetSharedScope(type, scopeByNativeType) is not null;

    private static SharedHandleScopeConfiguration? GetSharedScope(
        BindingType type,
        IReadOnlyDictionary<string, SharedHandleScopeConfiguration> scopeByNativeType)
    {
        if (!type.IsOcctHandle || string.IsNullOrWhiteSpace(type.HandleTargetType))
        {
            return null;
        }

        scopeByNativeType.TryGetValue(type.HandleTargetType.Trim(), out SharedHandleScopeConfiguration? scope);
        return scope;
    }

    private static string EmitNativeHeader(
        IReadOnlyList<SharedTypeBinding> bindings,
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlyList<SharedHandleScopeConfiguration> referencedScopes)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine("#pragma once");
        builder.AppendLine();
        builder.AppendLine("#include \"../Geometry/OcctSharp.Geometry.Values.Generated.h\"");
        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("extern \"C\" {");
        builder.AppendLine("#endif");

        foreach (SharedHandleScopeConfiguration scope in referencedScopes)
        {
            string nativeHandle = NativeHandleName(scope);
            builder.AppendLine();
            builder.AppendLine($"typedef struct {nativeHandle} {nativeHandle};");
        }

        foreach (SharedTypeBinding binding in bindings)
        {
            string nativeHandle = NativeHandleName(binding.Scope);
            foreach (SharedConstructor constructor in binding.Constructors)
            {
                builder.AppendLine();
                AppendNativeFunctionDeclaration(builder, constructor.ExportName, constructor.Parameters, null, null, nativeHandle);
            }

            foreach (SharedMethod method in binding.Methods)
            {
                builder.AppendLine();
                AppendNativeFunctionDeclaration(
                    builder,
                    method.ExportName,
                    method.Parameters,
                    method.ReturnProjection,
                    method.ReturnSharedScope,
                    nativeHandle,
                    hasReceiver: true,
                    returnsVoid: method.ReturnsVoid);
            }

            string prefix = binding.Scope.ExportNamePrefix;
            builder.AppendLine();
            builder.AppendLine($"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_clone(");
            builder.AppendLine($"  const {nativeHandle}* source,");
            builder.AppendLine($"  {nativeHandle}** out_handle);");
            builder.AppendLine();
            builder.AppendLine($"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_get_ref_count(");
            builder.AppendLine($"  const {nativeHandle}* handle,");
            builder.AppendLine("  int32_t* out_ref_count);");
            builder.AppendLine();
            builder.AppendLine($"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_get_type_name(");
            builder.AppendLine($"  const {nativeHandle}* handle,");
            builder.AppendLine("  const char** out_type_name);");
            builder.AppendLine();
            builder.AppendLine($"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_is_kind(");
            builder.AppendLine($"  const {nativeHandle}* handle,");
            builder.AppendLine("  const char* type_name,");
            builder.AppendLine("  int32_t* out_is_kind);");
            builder.AppendLine();
            builder.AppendLine($"OCCTSHARP_API void OCCTSHARP_CALL occtsharp_generated_{prefix}_release({nativeHandle}* handle);");
        }

        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("}");
        builder.AppendLine("#endif");
        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        foreach (string header in bindings.Select(static binding => binding.Scope.Header)
            .Concat(bindings.Where(static binding => binding.Scope.UsesPlacementAllocator)
                .Select(static _ => "NCollection_IncAllocator.hxx"))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal))
        {
            builder.AppendLine($"#include <{header}>");
        }
        builder.AppendLine("#include <utility>");
        foreach (SharedTypeBinding binding in bindings)
        {
            SharedHandleScopeConfiguration scope = binding.Scope;
            string handleType = NativeHandleName(scope);
            builder.AppendLine();
            builder.AppendLine($"struct {handleType}");
            builder.AppendLine("{");
            if (scope.UsesPlacementAllocator)
            {
                builder.AppendLine($"  {handleType}(opencascade::handle<{scope.NativeType}> value, opencascade::handle<NCollection_IncAllocator> allocator) : ConstructionAllocator(std::move(allocator)), Value(std::move(value)) {{}}");
                builder.AppendLine("  opencascade::handle<NCollection_IncAllocator> ConstructionAllocator;");
            }
            else
            {
                builder.AppendLine($"  explicit {handleType}(opencascade::handle<{scope.NativeType}> value) : Value(std::move(value)) {{}}");
            }
            builder.AppendLine($"  opencascade::handle<{scope.NativeType}> Value;");
            builder.AppendLine("};");
        }
        builder.AppendLine("#endif");
        return Normalize(builder.ToString());
    }

    private static string EmitNativeSource(
        IReadOnlyList<SharedTypeBinding> bindings,
        IReadOnlyList<BindingDeclaration> declarations,
        IReadOnlyList<SharedHandleScopeConfiguration> referencedScopes,
        IReadOnlyList<OcctProductModule> referencedModules,
        IReadOnlyList<string> preambleHeaders,
        string moduleName)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine($"#include \"OcctSharp.{moduleName}.SharedHandles.Generated.h\"");
        foreach (OcctProductModule referencedModule in referencedModules.Where(item => item != bindings[0].ProductModule))
        {
            builder.AppendLine($"#include \"../{referencedModule}/OcctSharp.{referencedModule}.SharedHandles.Generated.h\"");
        }
        builder.AppendLine("#include \"../Runtime/OcctSharp.Runtime.SharedSupport.Generated.hxx\"");
        builder.AppendLine("#include \"../../include/OcctSharp.Native.Internal.hxx\"");
        foreach (string header in preambleHeaders.Order(StringComparer.Ordinal))
        {
            builder.AppendLine($"#include <{header}>");
        }
        foreach (string header in referencedScopes.Select(static scope => scope.Header)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            builder.AppendLine($"#include <{header}>");
        }

        builder.AppendLine("#include <Standard_Failure.hxx>");
        builder.AppendLine("#include <Standard_Handle.hxx>");
        builder.AppendLine("#include <Standard_Type.hxx>");
        builder.AppendLine("#include <exception>");
        builder.AppendLine("#include <mutex>");
        builder.AppendLine("#include <stdexcept>");
        builder.AppendLine("#include <unordered_set>");
        builder.AppendLine("#include <utility>");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharpGenerated");
        builder.AppendLine("{");
        foreach (SharedHandleScopeConfiguration scope in referencedScopes)
        {
            string allocatorParameter = scope.UsesPlacementAllocator
                ? ", opencascade::handle<NCollection_IncAllocator> allocator"
                : string.Empty;
            builder.AppendLine($"{NativeHandleName(scope)}* Allocate{scope.ManagedTypeName}(opencascade::handle<{scope.NativeType}> value{allocatorParameter});");
            builder.AppendLine($"const {NativeHandleName(scope)}* Validate{scope.ManagedTypeName}(const {NativeHandleName(scope)}* handle);");
        }
        builder.AppendLine("}");
        builder.AppendLine("using namespace OcctSharpGenerated;");
        builder.AppendLine();
        builder.AppendLine("namespace");
        builder.AppendLine("{");
        builder.AppendLine("template <typename TAction>");
        builder.AppendLine("OcctSharp_Status GeneratedGuard(TAction&& action)");
        builder.AppendLine("{");
        builder.AppendLine("  OcctSharp_Internal_SetLastError(\"\");");
        builder.AppendLine("  try { action(); return OCCTSHARP_STATUS_SUCCESS; }");
        builder.AppendLine("  catch (const Standard_Failure& error) { OcctSharp_Internal_SetLastError(error.GetMessageString()); return OCCTSHARP_STATUS_OCCT_FAILURE; }");
        builder.AppendLine("  catch (const OcctSharpGenerated::GeneratedOperationFailure& error) { OcctSharp_Internal_SetLastError(error.what()); return error.Status; }");
        builder.AppendLine("  catch (const std::exception& error) { OcctSharp_Internal_SetLastError(error.what()); return OCCTSHARP_STATUS_STANDARD_EXCEPTION; }");
        builder.AppendLine("  catch (...) { OcctSharp_Internal_SetLastError(\"Unknown C++ exception in generated shared binding.\"); return OCCTSHARP_STATUS_UNKNOWN_EXCEPTION; }");
        builder.AppendLine("}");
        builder.AppendLine("}");

        foreach (SharedTypeBinding binding in bindings)
        {
            AppendNativeTypeInfrastructure(builder, binding);
        }
        foreach (SharedTypeBinding binding in bindings)
        {
            AppendNativeTypeOperations(builder, binding);
        }

        return Normalize(builder.ToString());
    }

    private static void AppendNativeTypeInfrastructure(StringBuilder builder, SharedTypeBinding binding)
    {
        SharedHandleScopeConfiguration scope = binding.Scope;
        string handleType = NativeHandleName(scope);
        string helperPrefix = scope.ManagedTypeName;
        builder.AppendLine();
        builder.AppendLine("namespace");
        builder.AppendLine("{");
        builder.AppendLine($"std::mutex {helperPrefix}Mutex;");
        builder.AppendLine($"std::unordered_set<const {handleType}*> Live{helperPrefix}Handles;");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharpGenerated");
        builder.AppendLine("{");
        string allocatorParameter = scope.UsesPlacementAllocator
            ? ", opencascade::handle<NCollection_IncAllocator> allocator"
            : string.Empty;
        builder.AppendLine($"{handleType}* Allocate{helperPrefix}(opencascade::handle<{scope.NativeType}> value{allocatorParameter})");
        builder.AppendLine("{");
        string allocatorArgument = scope.UsesPlacementAllocator ? ", std::move(allocator)" : string.Empty;
        builder.AppendLine($"  {handleType}* handle = new {handleType}(std::move(value){allocatorArgument});");
        builder.AppendLine("  try");
        builder.AppendLine("  {");
        builder.AppendLine($"    std::lock_guard<std::mutex> lock({helperPrefix}Mutex);");
        builder.AppendLine($"    Live{helperPrefix}Handles.insert(handle);");
        builder.AppendLine("    return handle;");
        builder.AppendLine("  }");
        builder.AppendLine("  catch (...) { delete handle; throw; }");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"const {handleType}* Validate{helperPrefix}(const {handleType}* handle)");
        builder.AppendLine("{");
        builder.AppendLine("  if (handle == nullptr) throw GeneratedOperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, \"The generated shared handle is null.\");");
        builder.AppendLine($"  std::lock_guard<std::mutex> lock({helperPrefix}Mutex);");
        builder.AppendLine($"  if (!Live{helperPrefix}Handles.contains(handle)) throw GeneratedOperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, \"The generated shared handle is invalid or already released.\");");
        builder.AppendLine("  if (handle->Value.IsNull()) throw GeneratedOperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, \"The generated OCCT shared value is null.\");");
        builder.AppendLine("  return handle;");
        builder.AppendLine("}");
        builder.AppendLine("}");
    }

    private static void AppendNativeTypeOperations(StringBuilder builder, SharedTypeBinding binding)
    {
        SharedHandleScopeConfiguration scope = binding.Scope;
        string handleType = NativeHandleName(scope);
        string helperPrefix = scope.ManagedTypeName;
        foreach (SharedConstructor constructor in binding.Constructors)
        {
            builder.AppendLine();
            AppendNativeConstructorDefinition(builder, binding, constructor);
        }

        foreach (SharedMethod method in binding.Methods)
        {
            builder.AppendLine();
            AppendNativeMethodDefinition(builder, binding, method);
        }

        string prefix = scope.ExportNamePrefix;
        builder.AppendLine();
        builder.AppendLine($"OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_clone(");
        builder.AppendLine($"  const {handleType}* source,");
        builder.AppendLine($"  {handleType}** out_handle)");
        builder.AppendLine("{");
        AppendOutputCheck(builder, "out_handle", "The output generated shared handle pointer is null.", "nullptr");
        string cloneAllocatorArgument = scope.UsesPlacementAllocator
            ? ", value->ConstructionAllocator"
            : string.Empty;
        builder.AppendLine($"  return GeneratedGuard([&] {{ const {handleType}* value = Validate{helperPrefix}(source); *out_handle = Allocate{helperPrefix}(value->Value{cloneAllocatorArgument}); }});");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_get_ref_count(");
        builder.AppendLine($"  const {handleType}* handle,");
        builder.AppendLine("  int32_t* out_ref_count)");
        builder.AppendLine("{");
        AppendOutputCheck(builder, "out_ref_count", "The output reference-count pointer is null.", "0");
        builder.AppendLine($"  return GeneratedGuard([&] {{ *out_ref_count = Validate{helperPrefix}(handle)->Value->GetRefCount(); }});");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_get_type_name(");
        builder.AppendLine($"  const {handleType}* handle,");
        builder.AppendLine("  const char** out_type_name)");
        builder.AppendLine("{");
        AppendOutputCheck(builder, "out_type_name", "The output type-name pointer is null.", "nullptr");
        builder.AppendLine($"  return GeneratedGuard([&] {{ *out_type_name = Validate{helperPrefix}(handle)->Value->DynamicType()->Name(); }});");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"OcctSharp_Status OCCTSHARP_CALL occtsharp_generated_{prefix}_is_kind(");
        builder.AppendLine($"  const {handleType}* handle,");
        builder.AppendLine("  const char* type_name,");
        builder.AppendLine("  int32_t* out_is_kind)");
        builder.AppendLine("{");
        builder.AppendLine("  if (type_name == nullptr || type_name[0] == '\\0') { OcctSharp_Internal_SetLastError(\"The generated shared type name is null or empty.\"); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }");
        AppendOutputCheck(builder, "out_is_kind", "The output kind-state pointer is null.", "0");
        builder.AppendLine($"  return GeneratedGuard([&] {{ *out_is_kind = Validate{helperPrefix}(handle)->Value->IsKind(type_name) ? 1 : 0; }});");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"void OCCTSHARP_CALL occtsharp_generated_{prefix}_release({handleType}* handle)");
        builder.AppendLine("{");
        builder.AppendLine("  if (handle == nullptr) return;");
        builder.AppendLine("  bool removed = false;");
        builder.AppendLine($"  {{ std::lock_guard<std::mutex> lock({helperPrefix}Mutex); removed = Live{helperPrefix}Handles.erase(handle) != 0; }}");
        builder.AppendLine("  if (removed) delete handle;");
        builder.AppendLine("}");
    }

    private static void AppendNativeConstructorDefinition(
        StringBuilder builder,
        SharedTypeBinding binding,
        SharedConstructor constructor)
    {
        string handleType = NativeHandleName(binding.Scope);
        builder.AppendLine($"OcctSharp_Status OCCTSHARP_CALL {constructor.ExportName}(");
        AppendNativeParameters(builder, constructor.Parameters, $"{handleType}** out_handle");
        builder.AppendLine("{");
        AppendOutputCheck(builder, "out_handle", "The output generated shared handle pointer is null.", "nullptr");
        string arguments = string.Join(", ", constructor.Parameters.Select(parameter =>
            ReferenceEquals(parameter, constructor.PlacementAllocatorParameter)
                ? "constructionAllocator"
                : RenderNativeArgument(parameter)));
        builder.AppendLine("  return GeneratedGuard([&]");
        builder.AppendLine("  {");
        if (constructor.PlacementAllocatorParameter is not null)
        {
            builder.AppendLine($"    opencascade::handle<NCollection_IncAllocator> constructionAllocator = ValidateNCollectionIncAllocator({constructor.PlacementAllocatorParameter.Name})->Value;");
            builder.AppendLine($"    opencascade::handle<{binding.Scope.NativeType}> createdHandle = new (constructionAllocator) {binding.Scope.NativeType}({arguments});");
            builder.AppendLine($"    *out_handle = Allocate{binding.Scope.ManagedTypeName}(std::move(createdHandle), std::move(constructionAllocator));");
        }
        else
        {
            builder.AppendLine($"    opencascade::handle<{binding.Scope.NativeType}> createdHandle = new {binding.Scope.NativeType}({arguments});");
            builder.AppendLine($"    *out_handle = Allocate{binding.Scope.ManagedTypeName}(std::move(createdHandle));");
        }
        builder.AppendLine("  });");
        builder.AppendLine("}");
    }

    private static void AppendNativeMethodDefinition(
        StringBuilder builder,
        SharedTypeBinding binding,
        SharedMethod method)
    {
        string handleType = NativeHandleName(binding.Scope);
        builder.AppendLine($"OcctSharp_Status OCCTSHARP_CALL {method.ExportName}(");
        List<string> trailing = method.Parameters.Select(RenderNativeParameterDeclaration).ToList();
        if (method.ReturnSharedScope is not null)
        {
            trailing.Add($"{NativeHandleName(method.ReturnSharedScope)}** out_handle");
        }
        else if (!method.ReturnsVoid)
        {
            trailing.Add($"{method.ReturnProjection!.AbiType}* out_value");
        }
        builder.AppendLine($"  const {handleType}* handle{(trailing.Count == 0 ? ")" : ",")}");
        for (int index = 0; index < trailing.Count; index++)
        {
            builder.AppendLine($"  {trailing[index]}{(index == trailing.Count - 1 ? ")" : ",")}");
        }
        builder.AppendLine("{");
        if (method.ReturnSharedScope is not null)
        {
            AppendOutputCheck(builder, "out_handle", "The generated shared return pointer is null.", "nullptr");
        }
        else if (!method.ReturnsVoid)
        {
            AppendOutputCheck(builder, "out_value", "The generated method output pointer is null.", DefaultAbiValue(method.ReturnProjection!));
        }
        string arguments = string.Join(", ", method.Parameters.Select(RenderNativeArgument));
        string invocation = $"Validate{binding.Scope.ManagedTypeName}(handle)->Value->{method.MemberName}({arguments})";
        builder.AppendLine("  return GeneratedGuard([&]");
        builder.AppendLine("  {");
        if (method.ReturnsVoid)
        {
            builder.AppendLine($"    {invocation};");
        }
        else if (method.ReturnSharedScope is not null)
        {
            builder.AppendLine($"    opencascade::handle<{method.ReturnSharedScope.NativeType}> returnedHandle = {invocation};");
            builder.AppendLine($"    if (!returnedHandle.IsNull()) *out_handle = Allocate{method.ReturnSharedScope.ManagedTypeName}(std::move(returnedHandle));");
        }
        else if (method.ReturnProjection!.RuleId == "TM005")
        {
            builder.AppendLine($"    const gp_Pnt nativeValue = {invocation};");
            builder.AppendLine("    *out_value = {nativeValue.X(), nativeValue.Y(), nativeValue.Z()};");
        }
        else if (method.ReturnProjection.RuleId == "TM003")
        {
            builder.AppendLine($"    *out_value = {invocation} ? 1 : 0;");
        }
        else if (method.ReturnProjection.RuleId == "TM004")
        {
            builder.AppendLine($"    *out_value = static_cast<int32_t>({invocation});");
        }
        else
        {
            builder.AppendLine($"    *out_value = {invocation};");
        }
        builder.AppendLine("  });");
        builder.AppendLine("}");
    }

    private static string EmitManagedRaw(
        IReadOnlyList<SharedTypeBinding> bindings,
        IReadOnlyList<BindingDeclaration> declarations)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using Microsoft.Win32.SafeHandles;");
        builder.AppendLine("using System.Runtime.CompilerServices;");
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharp.Generated;");

        foreach (SharedTypeBinding binding in bindings)
        {
            string handleName = binding.Scope.ManagedTypeName + "Handle";
            builder.AppendLine();
            builder.AppendLine($"internal sealed class {handleName} : SafeHandleZeroOrMinusOneIsInvalid");
            builder.AppendLine("{");
            builder.AppendLine($"    internal {handleName}() : base(true) {{ }}");
            builder.AppendLine($"    internal {handleName}(nint handle) : base(true) => SetHandle(handle);");
            builder.AppendLine($"    protected override bool ReleaseHandle() {{ GeneratedNativeMethods.{binding.Scope.ManagedTypeName}Release(handle); return true; }}");
            builder.AppendLine("}");
        }

        builder.AppendLine();
        builder.AppendLine("internal static partial class GeneratedNativeMethods");
        builder.AppendLine("{");
        foreach (SharedTypeBinding binding in bindings)
        {
            string handleName = binding.Scope.ManagedTypeName + "Handle";
            foreach (SharedConstructor constructor in binding.Constructors)
            {
                AppendLibraryImport(builder, constructor.ExportName);
                builder.Append($"    internal static partial global::OcctSharp.Interop.NativeStatus {constructor.ManagedName}(");
                List<string> parameters = constructor.Parameters
                    .Select(RenderManagedRawParameterDeclaration)
                    .ToList();
                parameters.Add("out nint handle");
                builder.Append(string.Join(", ", parameters));
                builder.AppendLine(");");
            }

            foreach (SharedMethod method in binding.Methods)
            {
                string resultName = GetUniqueGeneratedName(method.Parameters, "resultValue");
                AppendLibraryImport(builder, method.ExportName);
                builder.Append($"    internal static partial global::OcctSharp.Interop.NativeStatus {method.ManagedName}({handleName} handle");
                foreach (GeneratedParameter parameter in method.Parameters)
                {
                    builder.Append($", {RenderManagedRawParameterDeclaration(parameter)}");
                }
                if (method.ReturnSharedScope is not null)
                {
                    builder.Append(", out nint handleValue");
                }
                else if (!method.ReturnsVoid)
                {
                    builder.Append($", out {method.ReturnProjection!.ManagedRawType} {resultName}");
                }
                builder.AppendLine(");");
            }

            string prefix = binding.Scope.ExportNamePrefix;
            AppendLibraryImport(builder, $"occtsharp_generated_{prefix}_clone");
            builder.AppendLine($"    internal static partial global::OcctSharp.Interop.NativeStatus {binding.Scope.ManagedTypeName}Clone({handleName} source, out nint handle);");
            AppendLibraryImport(builder, $"occtsharp_generated_{prefix}_get_ref_count");
            builder.AppendLine($"    internal static partial global::OcctSharp.Interop.NativeStatus {binding.Scope.ManagedTypeName}GetReferenceCount({handleName} handle, out int value);");
            AppendLibraryImport(builder, $"occtsharp_generated_{prefix}_get_type_name");
            builder.AppendLine($"    internal static partial global::OcctSharp.Interop.NativeStatus {binding.Scope.ManagedTypeName}GetTypeName({handleName} handle, out nint value);");
            builder.AppendLine($"    [LibraryImport(LibraryName, EntryPoint = \"occtsharp_generated_{prefix}_is_kind\", StringMarshalling = StringMarshalling.Utf8)]");
            builder.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
            builder.AppendLine($"    internal static partial global::OcctSharp.Interop.NativeStatus {binding.Scope.ManagedTypeName}IsKind({handleName} handle, string typeName, out int value);");
            AppendLibraryImport(builder, $"occtsharp_generated_{prefix}_release");
            builder.AppendLine($"    internal static partial void {binding.Scope.ManagedTypeName}Release(nint handle);");
        }
        builder.AppendLine("}");
        return Normalize(builder.ToString());
    }

    private static string EmitManagedFriendly(
        IReadOnlyList<SharedTypeBinding> bindings,
        IReadOnlyList<BindingDeclaration> declarations,
        bool includePointValue)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine("using OcctSharp.Generated;");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharp;");
        if (includePointValue)
        {
            builder.AppendLine();
            builder.AppendLine("/// <summary>A copied OCCT three-dimensional point value.</summary>");
            builder.AppendLine("public readonly record struct Point3d(double X, double Y, double Z);");
        }

        foreach (SharedTypeBinding binding in bindings)
        {
            string managedType = binding.Scope.ManagedTypeName;
            string handleType = managedType + "Handle";
            builder.AppendLine();
            builder.AppendLine($"/// <summary>Generated shared-handle wrapper for OCCT {binding.Scope.NativeType}.</summary>");
            builder.AppendLine($"public sealed class {managedType} : IDisposable");
            builder.AppendLine("{");
            builder.AppendLine($"    private readonly {handleType} handle;");
            builder.AppendLine();
            builder.AppendLine($"    private {managedType}({handleType} handle) => this.handle = handle;");

            foreach (SharedConstructor constructor in binding.Constructors)
            {
                builder.AppendLine();
                builder.AppendLine($"    /// <summary>Creates a retained OCCT {binding.Scope.NativeType} shared object.</summary>");
                builder.Append($"    public {managedType}(");
                builder.Append(string.Join(", ", constructor.Parameters.Select(parameter =>
                    RenderFriendlyConstructorParameter(constructor, parameter))));
                builder.AppendLine(")");
                builder.AppendLine("    {");
                builder.AppendLine("        OcctRuntime.EnsureCompatible();");
                if (constructor.PlacementAllocatorParameter is not null)
                {
                    builder.AppendLine($"        ArgumentNullException.ThrowIfNull({constructor.PlacementAllocatorParameter.Name});");
                }
                builder.Append($"        Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{constructor.ManagedName}(");
                builder.Append(string.Join(", ", constructor.Parameters.Select(RenderManagedRawArgument)));
                if (constructor.Parameters.Length > 0) builder.Append(", ");
                builder.AppendLine("out nint nativeHandle), \"generated_shared_create\");");
                builder.AppendLine($"        handle = CreateHandle(nativeHandle, \"{constructor.ManagedName}\");");
                builder.AppendLine("    }");
            }

            foreach (SharedMethod method in binding.Methods)
            {
                string resultName = GetUniqueGeneratedName(method.Parameters, "resultValue");
                builder.AppendLine();
                builder.AppendLine($"    /// <summary>Invokes OCCT {binding.Scope.NativeType}::{method.MemberName}.</summary>");
                string returnType = method.ReturnsVoid
                    ? "void"
                    : method.ReturnSharedScope is not null
                        ? method.ReturnSharedScope.ManagedTypeName + "?"
                        : method.ReturnProjection!.ManagedFriendlyType;
                builder.Append($"    public {returnType} {method.ManagedMemberName}(");
                builder.Append(string.Join(", ", method.Parameters.Select(RenderFriendlyParameter)));
                builder.AppendLine(")");
                builder.AppendLine("    {");
                builder.AppendLine("        ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
                builder.Append($"        Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{method.ManagedName}(handle");
                foreach (GeneratedParameter parameter in method.Parameters)
                {
                    builder.Append(", " + RenderManagedRawArgument(parameter));
                }
                if (method.ReturnSharedScope is not null)
                {
                    builder.Append(", out nint handleValue");
                }
                else if (!method.ReturnsVoid)
                {
                    builder.Append($", out {method.ReturnProjection!.ManagedRawType} {resultName}");
                }
                builder.AppendLine($"), \"{method.ExportName}\");");
                if (method.ReturnSharedScope is not null)
                {
                    builder.AppendLine($"        return global::OcctSharp.{method.ReturnSharedScope.ManagedTypeName}.FromNative(handleValue, \"{method.ExportName}\");");
                }
                else if (!method.ReturnsVoid)
                {
                    builder.AppendLine("        return " + RenderFriendlyReturn(resultName, method.ReturnProjection!) + ";");
                }
                builder.AppendLine("    }");
            }

            builder.AppendLine();
            builder.AppendLine("    /// <summary>Gets the OCCT intrusive reference count.</summary>");
            builder.AppendLine("    public int ReferenceCount");
            builder.AppendLine("    {");
            builder.AppendLine("        get");
            builder.AppendLine("        {");
            builder.AppendLine("            ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
            builder.AppendLine($"            Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{managedType}GetReferenceCount(handle, out int value), \"generated_shared_get_ref_count\");");
            builder.AppendLine("            return value;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    /// <summary>Gets the registered OCCT runtime type name.</summary>");
            builder.AppendLine("    public string TypeName");
            builder.AppendLine("    {");
            builder.AppendLine("        get");
            builder.AppendLine("        {");
            builder.AppendLine("            ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
            builder.AppendLine($"            Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{managedType}GetTypeName(handle, out nint value), \"generated_shared_get_type_name\");");
            builder.AppendLine("            return Marshal.PtrToStringUTF8(value) ?? string.Empty;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    /// <summary>Checks the OCCT runtime type and base-type relationship.</summary>");
            builder.AppendLine("    public bool IsKind(string typeName)");
            builder.AppendLine("    {");
            builder.AppendLine("        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);");
            builder.AppendLine("        ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
            builder.AppendLine($"        Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{managedType}IsKind(handle, typeName, out int value), \"generated_shared_is_kind\");");
            builder.AppendLine("        return value != 0;");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    /// <summary>Creates another wrapper retaining the same OCCT object.</summary>");
            builder.AppendLine($"    public {managedType} Clone()");
            builder.AppendLine("    {");
            builder.AppendLine("        ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
            builder.AppendLine($"        Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{managedType}Clone(handle, out nint nativeHandle), \"generated_shared_clone\");");
            builder.AppendLine($"        return new {managedType}(CreateHandle(nativeHandle, \"generated_shared_clone\"));");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    /// <summary>Releases this wrapper's retained OCCT reference.</summary>");
            builder.AppendLine("    public void Dispose() => handle.Dispose();");
            builder.AppendLine();
            builder.AppendLine($"    internal {handleType} NativeHandle");
            builder.AppendLine("    {");
            builder.AppendLine("        get");
            builder.AppendLine("        {");
            builder.AppendLine("            ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
            builder.AppendLine("            return handle;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine($"    internal static {managedType}? FromNative(nint nativeHandle, string operation) =>");
            builder.AppendLine($"        nativeHandle == 0 ? null : new {managedType}(CreateHandle(nativeHandle, operation));");
            builder.AppendLine();
            builder.AppendLine($"    private static {handleType} CreateHandle(nint nativeHandle, string operation)");
            builder.AppendLine("    {");
            builder.AppendLine("        if (nativeHandle == 0) throw new OcctException(\"UnknownException\", $\"Native operation '{operation}' returned a null generated shared handle.\");");
            builder.AppendLine($"        return new {handleType}(nativeHandle);");
            builder.AppendLine("    }");
            builder.AppendLine("}");
        }

        return Normalize(builder.ToString());
    }

    private static void AppendNativeFunctionDeclaration(
        StringBuilder builder,
        string exportName,
        IReadOnlyList<GeneratedParameter> parameters,
        BindingTypeProjection? returnProjection,
        SharedHandleScopeConfiguration? returnSharedScope,
        string handleType,
        bool hasReceiver = false,
        bool returnsVoid = false)
    {
        builder.AppendLine($"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL {exportName}(");
        List<string> items = [];
        if (hasReceiver) items.Add($"const {handleType}* handle");
        items.AddRange(parameters.Select(RenderNativeParameterDeclaration));
        if (returnProjection is not null)
        {
            items.Add($"{returnProjection.AbiType}* out_value");
        }
        else if (returnSharedScope is not null)
        {
            items.Add($"{NativeHandleName(returnSharedScope)}** out_handle");
        }
        else if (!returnsVoid)
        {
            items.Add($"{handleType}** out_handle");
        }
        for (int index = 0; index < items.Count; index++)
        {
            builder.AppendLine($"  {items[index]}{(index == items.Count - 1 ? ");" : ",")}");
        }
    }

    private static void AppendNativeParameters(
        StringBuilder builder,
        IReadOnlyList<GeneratedParameter> parameters,
        string finalParameter)
    {
        List<string> items = parameters.Select(RenderNativeParameterDeclaration).ToList();
        items.Add(finalParameter);
        for (int index = 0; index < items.Count; index++)
        {
            builder.AppendLine($"  {items[index]}{(index == items.Count - 1 ? ")" : ",")}");
        }
    }

    private static void AppendOutputCheck(
        StringBuilder builder,
        string pointer,
        string message,
        string defaultValue)
    {
        builder.AppendLine($"  if ({pointer} == nullptr) {{ OcctSharp_Internal_SetLastError(\"{message}\"); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }}");
        builder.AppendLine($"  *{pointer} = {defaultValue};");
    }

    private static void AppendLibraryImport(StringBuilder builder, string entryPoint)
    {
        builder.AppendLine();
        builder.AppendLine($"    [LibraryImport(LibraryName, EntryPoint = \"{entryPoint}\")]");
        builder.AppendLine("    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
    }

    private static string RenderNativeParameterDeclaration(GeneratedParameter parameter) =>
        parameter.SharedScope is not null
            ? $"const {NativeHandleName(parameter.SharedScope)}* {parameter.Name}"
            : $"{parameter.Projection.AbiType} {parameter.Name}";

    private static string RenderManagedRawParameterDeclaration(GeneratedParameter parameter) =>
        parameter.SharedScope is not null
            ? $"nint {parameter.Name}"
            : $"{parameter.Projection.ManagedRawType} {parameter.Name}";

    private static string RenderNativeArgument(GeneratedParameter parameter)
    {
        if (parameter.SharedScope is not null)
        {
            return $"({parameter.Name} == nullptr ? opencascade::handle<{parameter.SharedScope.NativeType}>() : Validate{parameter.SharedScope.ManagedTypeName}({parameter.Name})->Value)";
        }

        return parameter.Projection.RuleId switch
        {
            "TM003" => $"({parameter.Name} != 0)",
            "TM004" => $"static_cast<{parameter.Parameter.Type.BaseCanonicalSpelling}>({parameter.Name})",
            "TM005" => $"gp_Pnt({parameter.Name}.x, {parameter.Name}.y, {parameter.Name}.z)",
            _ => parameter.Name,
        };
    }

    private static string RenderFriendlyParameter(GeneratedParameter parameter) =>
        parameter.SharedScope is not null
            ? $"{parameter.SharedScope.ManagedTypeName}? {parameter.Name}"
            : $"{parameter.Projection.ManagedFriendlyType} {parameter.Name}";

    private static string RenderFriendlyConstructorParameter(
        SharedConstructor constructor,
        GeneratedParameter parameter) =>
        ReferenceEquals(parameter, constructor.PlacementAllocatorParameter)
            ? $"{parameter.SharedScope!.ManagedTypeName} {parameter.Name}"
            : RenderFriendlyParameter(parameter);

    private static string RenderManagedRawArgument(GeneratedParameter parameter)
    {
        if (parameter.SharedScope is not null)
        {
            return $"{parameter.Name} is null ? nint.Zero : {parameter.Name}.NativeHandle.DangerousGetHandle()";
        }

        return parameter.Projection.RuleId switch
        {
            "TM003" => $"{parameter.Name} ? 1 : 0",
            "TM004" => $"(int){parameter.Name}",
            "TM005" => $"new Point3dRaw({parameter.Name}.X, {parameter.Name}.Y, {parameter.Name}.Z)",
            _ => parameter.Name,
        };
    }

    private static string RenderFriendlyReturn(string value, BindingTypeProjection projection) => projection.RuleId switch
    {
        "TM003" => value + " != 0",
        "TM004" => $"({projection.ManagedFriendlyType}){value}",
        "TM005" => $"new Point3d({value}.X, {value}.Y, {value}.Z)",
        _ => value,
    };

    private static string DefaultAbiValue(BindingTypeProjection projection) => projection.RuleId switch
    {
        "TM005" => "{}",
        _ => "{}",
    };

    private static bool TryValueProjection(
        BindingType type,
        BindingTypeUsage usage,
        InitialTypeMap typeMap,
        out BindingTypeProjection? projection) =>
        typeMap.TryMap(type, usage, out projection)
        && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal);

    private static BindingTypeProjection GetValueProjection(
        BindingType type,
        BindingTypeUsage usage,
        InitialTypeMap typeMap)
    {
        if (!TryValueProjection(type, usage, typeMap, out BindingTypeProjection? projection))
        {
            throw new InvalidDataException($"Type '{type.NativeSpelling}' lacks a value-copy projection.");
        }
        return projection!;
    }

    private static bool IsVoid(BindingType type) =>
        string.Equals(type.BaseCanonicalSpelling.Trim(), "void", StringComparison.Ordinal)
        && type.Layers is [{ Kind: BindingTypeLayerKind.Value }];

    private static void ValidateScopes(IReadOnlyList<SharedHandleScopeConfiguration> scopes)
    {
        HashSet<string> nativeTypes = new(StringComparer.Ordinal);
        HashSet<string> exports = new(StringComparer.Ordinal);
        HashSet<string> managedTypes = new(StringComparer.Ordinal);
        foreach (SharedHandleScopeConfiguration scope in scopes)
        {
            if (string.IsNullOrWhiteSpace(scope.SourcePackage)
                || string.IsNullOrWhiteSpace(scope.NativeType)
                || string.IsNullOrWhiteSpace(scope.Header)
                || string.IsNullOrWhiteSpace(scope.ExportNamePrefix)
                || string.IsNullOrWhiteSpace(scope.ManagedTypeName))
            {
                throw new InvalidDataException("Every generated shared-handle scope must define package, native type, header, export prefix, and managed type.");
            }
            if (!nativeTypes.Add(scope.NativeType)) throw new InvalidDataException($"Shared native type '{scope.NativeType}' is configured more than once.");
            if (!exports.Add(scope.ExportNamePrefix)) throw new InvalidDataException($"Shared export prefix '{scope.ExportNamePrefix}' is configured more than once.");
            if (!managedTypes.Add(scope.ManagedTypeName)) throw new InvalidDataException($"Shared managed type '{scope.ManagedTypeName}' is configured more than once.");
        }
    }

    private static void ValidatePreambleHeaders(IReadOnlyList<string> headers)
    {
        HashSet<string> unique = new(StringComparer.Ordinal);
        foreach (string header in headers)
        {
            if (string.IsNullOrWhiteSpace(header)
                || header.IndexOfAny(['\r', '\n', '<', '>', '"']) >= 0
                || !unique.Add(header))
            {
                throw new InvalidDataException(
                    $"Generated preamble header '{header}' is empty, unsafe, or configured more than once.");
            }
        }
    }

    private static StringBuilder StartFile(IEnumerable<BindingDeclaration> declarations)
    {
        StringBuilder builder = new();
        builder.AppendLine("// <auto-generated />");
        foreach (BindingDeclaration declaration in declarations.OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal))
        {
            builder.AppendLine("// Source: " + declaration.StableId);
        }
        return builder;
    }

    private static string NativeHandleName(SharedHandleScopeConfiguration scope) =>
        "OcctSharp_" + scope.ManagedTypeName + "Handle";

    private static string GetUnqualifiedName(string nativeType)
    {
        int separator = nativeType.LastIndexOf("::", StringComparison.Ordinal);
        return separator < 0 ? nativeType : nativeType[(separator + 2)..];
    }

    private static string GetMemberName(string nativeName)
    {
        int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal);
        return separator < 0 ? nativeName : nativeName[(separator + 2)..];
    }

    private static SharedMethod[] AssignUniqueManagedMemberNames(IReadOnlyList<SharedMethod> methods)
    {
        Dictionary<string, int> signatureCounts = new(StringComparer.Ordinal);
        List<SharedMethod> result = new(methods.Count);
        foreach (SharedMethod method in methods)
        {
            string parameterSignature = string.Join(",", method.Parameters.Select(GetManagedParameterType));
            string baseSignature = $"{method.ManagedMemberName}({parameterSignature})";
            signatureCounts.TryGetValue(baseSignature, out int duplicateIndex);
            signatureCounts[baseSignature] = duplicateIndex + 1;
            result.Add(duplicateIndex == 0
                ? method
                : method with { ManagedMemberName = method.ManagedMemberName + "Generated" + duplicateIndex });
        }

        return result.ToArray();
    }

    private static string GetManagedParameterType(GeneratedParameter parameter) =>
        parameter.SharedScope is not null
            ? parameter.SharedScope.ManagedTypeName
            : parameter.Projection.ManagedFriendlyType;

    private static string GetUniqueGeneratedName(
        IReadOnlyList<GeneratedParameter> parameters,
        string preferredName)
    {
        HashSet<string> names = parameters
            .Select(static parameter => parameter.Name)
            .ToHashSet(StringComparer.Ordinal);
        if (!names.Contains(preferredName))
        {
            return preferredName;
        }

        for (int suffix = 2; ; suffix++)
        {
            string candidate = preferredName + suffix.ToString(CultureInfo.InvariantCulture);
            if (!names.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    private static string ToParameterName(string name, int position)
    {
        if (string.IsNullOrWhiteSpace(name)) return "value" + position;
        string value = char.ToLowerInvariant(name[0]) + name[1..];
        return IsReservedParameterName(value) ? value + "_value" : value;
    }

    private static bool IsReservedParameterName(string value) => value is
        "alignas" or "alignof" or "and" or "and_eq" or "asm" or "auto" or "bitand" or
        "bitor" or "bool" or "break" or "case" or "catch" or "char" or "class" or
        "compl" or "concept" or "const" or "consteval" or "constexpr" or "constinit" or
        "const_cast" or "continue" or "co_await" or "co_return" or "co_yield" or
        "decltype" or "default" or "delete" or "do" or "double" or "dynamic_cast" or
        "else" or "enum" or "explicit" or "export" or "extern" or "false" or "float" or
        "for" or "friend" or "goto" or "if" or "inline" or "int" or "long" or
        "mutable" or "namespace" or "new" or "noexcept" or "not" or "not_eq" or
        "nullptr" or "operator" or "or" or "or_eq" or "private" or "protected" or
        "public" or "register" or "reinterpret_cast" or "requires" or "return" or
        "short" or "signed" or "sizeof" or "static" or "static_assert" or "static_cast" or
        "struct" or "switch" or "template" or "this" or "thread_local" or "throw" or
        "true" or "try" or "typedef" or "typeid" or "typename" or "union" or "unsigned" or
        "using" or "virtual" or "void" or "volatile" or "wchar_t" or "while" or "xor" or
        "xor_eq" or "abstract" or "as" or "base" or "byte" or "checked" or "decimal" or
        "delegate" or "event" or "fixed" or "foreach" or "implicit" or "in" or
        "interface" or "internal" or "is" or "lock" or "object" or "out" or "override" or
        "params" or "readonly" or "ref" or "sbyte" or "sealed" or "stackalloc" or "string" or
        "uint" or "ulong" or "unchecked" or "unsafe" or "ushort";

    private static string ToManagedMemberName(string nativeName) => nativeName is
        "GetType" or "Equals" or "GetHashCode" or "ToString" or "Clone" or "Dispose" or
        "IsKind" or "ReferenceCount" or "TypeName"
            ? "Occt" + nativeName
            : nativeName;

    private static string ToSnakeCase(string value)
    {
        StringBuilder builder = new(value.Length + 8);
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            if (char.IsUpper(current) && index > 0 && !char.IsUpper(value[index - 1])) builder.Append('_');
            builder.Append(char.ToLowerInvariant(current));
        }
        return builder.ToString();
    }

    private static string Normalize(string value) => value
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .TrimEnd('\n') + "\n";

    private sealed record SharedTypeBinding(
        SharedHandleScopeConfiguration Scope,
        SharedConstructor[] Constructors,
        SharedMethod[] Methods,
        OcctProductModule ProductModule);

    private sealed record SharedConstructor(
        BindingDeclaration Declaration,
        GeneratedParameter[] Parameters,
        GeneratedParameter? PlacementAllocatorParameter,
        int OverloadIndex,
        string ExportName,
        string ManagedName);

    private sealed record SharedMethod(
        BindingDeclaration Declaration,
        GeneratedParameter[] Parameters,
        BindingTypeProjection? ReturnProjection,
        SharedHandleScopeConfiguration? ReturnSharedScope,
        bool ReturnsVoid,
        int OverloadIndex,
        string ExportName,
        string ManagedName,
        string MemberName,
        string ManagedMemberName);

    private sealed record GeneratedParameter(
        BindingParameter Parameter,
        string Name,
        BindingTypeProjection? ValueProjection,
        SharedHandleScopeConfiguration? SharedScope)
    {
        public bool IsShared => SharedScope is not null;

        public BindingTypeProjection Projection => ValueProjection
            ?? throw new InvalidOperationException("A shared-handle parameter has no value projection.");
    }
}
