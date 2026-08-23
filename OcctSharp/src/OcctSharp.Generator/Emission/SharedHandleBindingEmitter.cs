using System.Text;
using OcctSharp.Generator.Discovery;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

#pragma warning disable CA1305 // Interpolated values are normalized identifiers and signatures, not locale-sensitive data.

namespace OcctSharp.Generator.Emission;

public static class SharedHandleBindingEmitter
{
    private const string NativeHeaderPath = "src/OcctSharp.Native/generated/OcctSharp.SharedHandles.Generated.h";
    private const string NativeSourcePath = "src/OcctSharp.Native/generated/OcctSharp.SharedHandles.Generated.cpp";
    private const string ManagedRawPath = "src/OcctSharp/Generated/SharedHandlesRaw.Generated.cs";
    private const string ManagedFriendlyPath = "src/OcctSharp/Generated/SharedHandles.Generated.cs";

    public static GeneratedBindingSet Emit(
        string occtVersion,
        BindingModel model,
        IReadOnlyList<SharedHandleScopeConfiguration> scopes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occtVersion);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(scopes);

        if (scopes.Count == 0)
        {
            return new GeneratedBindingSet(occtVersion, [], []);
        }

        ValidateScopes(scopes);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        SharedTypeBinding[] bindings = scopes
            .OrderBy(static scope => scope.NativeType, StringComparer.Ordinal)
            .Select(scope => CreateBinding(model, typeMap, scope))
            .ToArray();
        BindingDeclaration[] declarations = bindings
            .SelectMany(static binding => binding.Constructors
                .Select(static constructor => constructor.Declaration)
                .Concat(binding.Methods.Select(static method => method.Declaration)))
            .OrderBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .ToArray();

        return new GeneratedBindingSet(
            occtVersion,
            declarations.Select(static declaration => declaration.StableId).ToArray(),
            [
                new GeneratedFile(NativeHeaderPath, EmitNativeHeader(bindings, declarations)),
                new GeneratedFile(NativeSourcePath, EmitNativeSource(bindings, declarations)),
                new GeneratedFile(ManagedRawPath, EmitManagedRaw(bindings, declarations)),
                new GeneratedFile(ManagedFriendlyPath, EmitManagedFriendly(bindings, declarations)),
            ]);
    }

    private static SharedTypeBinding CreateBinding(
        BindingModel model,
        InitialTypeMap typeMap,
        SharedHandleScopeConfiguration scope)
    {
        string constructorName = scope.NativeType + "::" + GetUnqualifiedName(scope.NativeType);
        SharedConstructor[] constructors = model.Declarations
            .Where(declaration => declaration.SupportState == BindingSupportState.Supported
                && declaration.Kind == BindingDeclarationKind.Constructor
                && string.Equals(declaration.SourcePackage, scope.SourcePackage, StringComparison.Ordinal)
                && string.Equals(declaration.NativeName, constructorName, StringComparison.Ordinal)
                && declaration.Parameters.All(parameter => TryValueProjection(
                    parameter.Type,
                    BindingTypeUsage.Parameter,
                    typeMap,
                    out _)))
            .OrderBy(static declaration => declaration.NativeSignature, StringComparer.Ordinal)
            .ThenBy(static declaration => declaration.StableId, StringComparer.Ordinal)
            .Select((declaration, index) => new SharedConstructor(
                declaration,
                CreateParameters(declaration.Parameters, typeMap),
                index,
                $"occtsharp_generated_{scope.ExportNamePrefix}_create_{index}",
                $"{scope.ManagedTypeName}Create{index}"))
            .ToArray();

        if (constructors.Length == 0)
        {
            throw new InvalidDataException(
                $"Shared-handle scope '{scope.NativeType}' has no supported public value-copy constructor.");
        }

        SharedMethod[] methods = model.Declarations
            .Where(declaration => IsSupportedMethod(declaration, scope, typeMap))
            .GroupBy(static declaration => declaration.NativeName, StringComparer.Ordinal)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .SelectMany(group => group
                .OrderBy(static declaration => declaration.NativeSignature, StringComparer.Ordinal)
                .ThenBy(static declaration => declaration.StableId, StringComparer.Ordinal)
                .Select((declaration, index) =>
                {
                    bool returnsVoid = IsVoid(declaration.ReturnType!);
                    BindingTypeProjection? returnProjection = returnsVoid
                        ? null
                        : GetValueProjection(declaration.ReturnType!, BindingTypeUsage.ReturnValue, typeMap);
                    string memberName = declaration.NativeName[(declaration.NativeName.LastIndexOf("::", StringComparison.Ordinal) + 2)..];
                    return new SharedMethod(
                        declaration,
                        CreateParameters(declaration.Parameters, typeMap),
                        returnProjection,
                        returnsVoid,
                        index,
                        $"occtsharp_generated_{scope.ExportNamePrefix}_{ToSnakeCase(memberName)}_{index}",
                        $"{scope.ManagedTypeName}{memberName}{index}",
                        memberName);
                }))
            .ToArray();

        return new SharedTypeBinding(scope, constructors, methods);
    }

    private static bool IsSupportedMethod(
        BindingDeclaration declaration,
        SharedHandleScopeConfiguration scope,
        InitialTypeMap typeMap)
    {
        if (declaration.SupportState != BindingSupportState.Supported
            || declaration.Kind != BindingDeclarationKind.Method
            || declaration.IsStatic
            || declaration.IsPureVirtual
            || declaration.IsOverloadedOperator
            || !string.Equals(declaration.SourcePackage, scope.SourcePackage, StringComparison.Ordinal)
            || !declaration.NativeName.StartsWith(scope.NativeType + "::", StringComparison.Ordinal)
            || declaration.NativeName.Contains("::~", StringComparison.Ordinal)
            || declaration.ReturnType is null
            || (!IsVoid(declaration.ReturnType)
                && !TryValueProjection(declaration.ReturnType, BindingTypeUsage.ReturnValue, typeMap, out _)))
        {
            return false;
        }

        return declaration.Parameters.All(parameter => TryValueProjection(
            parameter.Type,
            BindingTypeUsage.Parameter,
            typeMap,
            out _));
    }

    private static GeneratedParameter[] CreateParameters(
        IReadOnlyList<BindingParameter> parameters,
        InitialTypeMap typeMap) => parameters
        .Select(parameter => new GeneratedParameter(
            parameter,
            ToParameterName(parameter.Name, parameter.Position),
            GetValueProjection(parameter.Type, BindingTypeUsage.Parameter, typeMap)))
        .ToArray();

    private static string EmitNativeHeader(
        IReadOnlyList<SharedTypeBinding> bindings,
        IReadOnlyList<BindingDeclaration> declarations)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine("#pragma once");
        builder.AppendLine();
        builder.AppendLine("#include \"OcctSharp.Generated.h\"");
        builder.AppendLine();
        builder.AppendLine("#ifdef __cplusplus");
        builder.AppendLine("extern \"C\" {");
        builder.AppendLine("#endif");

        foreach (SharedTypeBinding binding in bindings)
        {
            string nativeHandle = NativeHandleName(binding.Scope);
            builder.AppendLine();
            builder.AppendLine($"typedef struct {nativeHandle} {nativeHandle};");
            foreach (SharedConstructor constructor in binding.Constructors)
            {
                builder.AppendLine();
                AppendNativeFunctionDeclaration(builder, constructor.ExportName, constructor.Parameters, null, nativeHandle);
            }

            foreach (SharedMethod method in binding.Methods)
            {
                builder.AppendLine();
                AppendNativeFunctionDeclaration(
                    builder,
                    method.ExportName,
                    method.Parameters,
                    method.ReturnProjection,
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
        return Normalize(builder.ToString());
    }

    private static string EmitNativeSource(
        IReadOnlyList<SharedTypeBinding> bindings,
        IReadOnlyList<BindingDeclaration> declarations)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine("#include \"OcctSharp.SharedHandles.Generated.h\"");
        builder.AppendLine("#include \"../include/OcctSharp.Native.Internal.hxx\"");
        foreach (string header in bindings.Select(static binding => binding.Scope.Header)
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
        builder.AppendLine("namespace");
        builder.AppendLine("{");
        builder.AppendLine("class GeneratedOperationFailure final : public std::runtime_error");
        builder.AppendLine("{");
        builder.AppendLine("public:");
        builder.AppendLine("  GeneratedOperationFailure(const OcctSharp_Status status, const char* message)");
        builder.AppendLine("    : std::runtime_error(message), Status(status) {}");
        builder.AppendLine("  OcctSharp_Status Status;");
        builder.AppendLine("};");
        builder.AppendLine();
        builder.AppendLine("template <typename TAction>");
        builder.AppendLine("OcctSharp_Status GeneratedGuard(TAction&& action)");
        builder.AppendLine("{");
        builder.AppendLine("  OcctSharp_Internal_SetLastError(\"\");");
        builder.AppendLine("  try { action(); return OCCTSHARP_STATUS_SUCCESS; }");
        builder.AppendLine("  catch (const Standard_Failure& error) { OcctSharp_Internal_SetLastError(error.GetMessageString()); return OCCTSHARP_STATUS_OCCT_FAILURE; }");
        builder.AppendLine("  catch (const GeneratedOperationFailure& error) { OcctSharp_Internal_SetLastError(error.what()); return error.Status; }");
        builder.AppendLine("  catch (const std::exception& error) { OcctSharp_Internal_SetLastError(error.what()); return OCCTSHARP_STATUS_STANDARD_EXCEPTION; }");
        builder.AppendLine("  catch (...) { OcctSharp_Internal_SetLastError(\"Unknown C++ exception in generated shared binding.\"); return OCCTSHARP_STATUS_UNKNOWN_EXCEPTION; }");
        builder.AppendLine("}");
        builder.AppendLine("}");

        foreach (SharedTypeBinding binding in bindings)
        {
            AppendNativeTypeImplementation(builder, binding);
        }

        return Normalize(builder.ToString());
    }

    private static void AppendNativeTypeImplementation(StringBuilder builder, SharedTypeBinding binding)
    {
        SharedHandleScopeConfiguration scope = binding.Scope;
        string handleType = NativeHandleName(scope);
        string helperPrefix = scope.ManagedTypeName;
        builder.AppendLine();
        builder.AppendLine($"struct {handleType}");
        builder.AppendLine("{");
        builder.AppendLine($"  explicit {handleType}(opencascade::handle<{scope.NativeType}> value) : Value(std::move(value)) {{}}");
        builder.AppendLine($"  opencascade::handle<{scope.NativeType}> Value;");
        builder.AppendLine("};");
        builder.AppendLine();
        builder.AppendLine("namespace");
        builder.AppendLine("{");
        builder.AppendLine($"std::mutex {helperPrefix}Mutex;");
        builder.AppendLine($"std::unordered_set<const {handleType}*> Live{helperPrefix}Handles;");
        builder.AppendLine();
        builder.AppendLine($"{handleType}* Allocate{helperPrefix}(opencascade::handle<{scope.NativeType}> value)");
        builder.AppendLine("{");
        builder.AppendLine($"  {handleType}* handle = new {handleType}(std::move(value));");
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
        builder.AppendLine($"  return GeneratedGuard([&] {{ const {handleType}* value = Validate{helperPrefix}(source); *out_handle = Allocate{helperPrefix}(value->Value); }});");
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
        string arguments = string.Join(", ", constructor.Parameters.Select(RenderNativeArgument));
        builder.AppendLine("  return GeneratedGuard([&]");
        builder.AppendLine("  {");
        builder.AppendLine($"    opencascade::handle<{binding.Scope.NativeType}> value = new {binding.Scope.NativeType}({arguments});");
        builder.AppendLine($"    *out_handle = Allocate{binding.Scope.ManagedTypeName}(std::move(value));");
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
        List<string> trailing = method.Parameters.Select(parameter =>
            $"{parameter.Projection.AbiType} {parameter.Name}").ToList();
        if (!method.ReturnsVoid)
        {
            trailing.Add($"{method.ReturnProjection!.AbiType}* out_value");
        }
        builder.AppendLine($"  const {handleType}* handle{(trailing.Count == 0 ? ")" : ",")}");
        for (int index = 0; index < trailing.Count; index++)
        {
            builder.AppendLine($"  {trailing[index]}{(index == trailing.Count - 1 ? ")" : ",")}");
        }
        builder.AppendLine("{");
        if (!method.ReturnsVoid)
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
        else if (method.ReturnProjection!.RuleId == "TM005")
        {
            builder.AppendLine($"    const gp_Pnt value = {invocation};");
            builder.AppendLine("    *out_value = {value.X(), value.Y(), value.Z()};");
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
                    .Select(parameter => $"{parameter.Projection.ManagedRawType} {parameter.Name}")
                    .ToList();
                parameters.Add("out nint handle");
                builder.Append(string.Join(", ", parameters));
                builder.AppendLine(");");
            }

            foreach (SharedMethod method in binding.Methods)
            {
                AppendLibraryImport(builder, method.ExportName);
                builder.Append($"    internal static partial global::OcctSharp.Interop.NativeStatus {method.ManagedName}({handleName} handle");
                foreach (GeneratedParameter parameter in method.Parameters)
                {
                    builder.Append($", {parameter.Projection.ManagedRawType} {parameter.Name}");
                }
                if (!method.ReturnsVoid)
                {
                    builder.Append($", out {method.ReturnProjection!.ManagedRawType} value");
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
        IReadOnlyList<BindingDeclaration> declarations)
    {
        StringBuilder builder = StartFile(declarations);
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine("using OcctSharp.Generated;");
        builder.AppendLine();
        builder.AppendLine("namespace OcctSharp;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>A copied OCCT three-dimensional point value.</summary>");
        builder.AppendLine("public readonly record struct Point3d(double X, double Y, double Z);");

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
                builder.Append(string.Join(", ", constructor.Parameters.Select(RenderFriendlyParameter)));
                builder.AppendLine(")");
                builder.AppendLine("    {");
                builder.AppendLine("        OcctRuntime.EnsureCompatible();");
                builder.Append($"        Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{constructor.ManagedName}(");
                builder.Append(string.Join(", ", constructor.Parameters.Select(RenderManagedRawArgument)));
                if (constructor.Parameters.Length > 0) builder.Append(", ");
                builder.AppendLine("out nint nativeHandle), \"generated_shared_create\");");
                builder.AppendLine($"        handle = CreateHandle(nativeHandle, \"{constructor.ManagedName}\");");
                builder.AppendLine("    }");
            }

            foreach (SharedMethod method in binding.Methods)
            {
                builder.AppendLine();
                builder.AppendLine($"    /// <summary>Invokes OCCT {binding.Scope.NativeType}::{method.MemberName}.</summary>");
                string returnType = method.ReturnsVoid ? "void" : method.ReturnProjection!.ManagedFriendlyType;
                builder.Append($"    public {returnType} {method.MemberName}(");
                builder.Append(string.Join(", ", method.Parameters.Select(RenderFriendlyParameter)));
                builder.AppendLine(")");
                builder.AppendLine("    {");
                builder.AppendLine("        ObjectDisposedException.ThrowIf(handle.IsClosed, this);");
                builder.Append($"        Interop.NativeError.ThrowIfFailed(GeneratedNativeMethods.{method.ManagedName}(handle");
                foreach (GeneratedParameter parameter in method.Parameters)
                {
                    builder.Append(", " + RenderManagedRawArgument(parameter));
                }
                if (!method.ReturnsVoid)
                {
                    builder.Append($", out {method.ReturnProjection!.ManagedRawType} value");
                }
                builder.AppendLine($"), \"{method.ExportName}\");");
                if (!method.ReturnsVoid)
                {
                    builder.AppendLine("        return " + RenderFriendlyReturn("value", method.ReturnProjection!) + ";");
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
        string handleType,
        bool hasReceiver = false,
        bool returnsVoid = false)
    {
        builder.AppendLine($"OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL {exportName}(");
        List<string> items = [];
        if (hasReceiver) items.Add($"const {handleType}* handle");
        items.AddRange(parameters.Select(parameter => $"{parameter.Projection.AbiType} {parameter.Name}"));
        if (returnProjection is not null)
        {
            items.Add($"{returnProjection.AbiType}* out_value");
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
        List<string> items = parameters.Select(parameter =>
            $"{parameter.Projection.AbiType} {parameter.Name}").ToList();
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

    private static string RenderNativeArgument(GeneratedParameter parameter) => parameter.Projection.RuleId switch
    {
        "TM003" => $"({parameter.Name} != 0)",
        "TM004" => $"static_cast<{parameter.Parameter.Type.BaseCanonicalSpelling}>({parameter.Name})",
        "TM005" => $"gp_Pnt({parameter.Name}.x, {parameter.Name}.y, {parameter.Name}.z)",
        _ => parameter.Name,
    };

    private static string RenderFriendlyParameter(GeneratedParameter parameter) =>
        $"{parameter.Projection.ManagedFriendlyType} {parameter.Name}";

    private static string RenderManagedRawArgument(GeneratedParameter parameter) => parameter.Projection.RuleId switch
    {
        "TM003" => $"{parameter.Name} ? 1 : 0",
        "TM005" => $"new Point3dRaw({parameter.Name}.X, {parameter.Name}.Y, {parameter.Name}.Z)",
        _ => parameter.Name,
    };

    private static string RenderFriendlyReturn(string value, BindingTypeProjection projection) => projection.RuleId switch
    {
        "TM003" => value + " != 0",
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

    private static string ToParameterName(string name, int position)
    {
        if (string.IsNullOrWhiteSpace(name)) return "value" + position;
        string value = char.ToLowerInvariant(name[0]) + name[1..];
        return value is "event" or "params" or "string" or "object" or "ref" or "out" or "in"
            ? "@" + value
            : value;
    }

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
        SharedMethod[] Methods);

    private sealed record SharedConstructor(
        BindingDeclaration Declaration,
        GeneratedParameter[] Parameters,
        int OverloadIndex,
        string ExportName,
        string ManagedName);

    private sealed record SharedMethod(
        BindingDeclaration Declaration,
        GeneratedParameter[] Parameters,
        BindingTypeProjection? ReturnProjection,
        bool ReturnsVoid,
        int OverloadIndex,
        string ExportName,
        string ManagedName,
        string MemberName);

    private sealed record GeneratedParameter(
        BindingParameter Parameter,
        string Name,
        BindingTypeProjection Projection);
}
