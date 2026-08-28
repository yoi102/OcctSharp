using ClangSharp;
using ClangSharp.Interop;
using System.Globalization;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;

namespace OcctSharp.Generator.Discovery;

public sealed class ClangAstDiscovery
{
    public static DiscoveryReport Discover(DiscoveryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string occtRoot = Path.GetFullPath(options.OcctRoot);
        string includeRoot = Path.Combine(occtRoot, "inc");
        string versionHeader = Path.Combine(includeRoot, "Standard_Version.hxx");

        if (!Directory.Exists(includeRoot) || !File.Exists(versionHeader))
        {
            throw new DirectoryNotFoundException(
                $"'{occtRoot}' is not an OCCT installation with inc/Standard_Version.hxx.");
        }

        string[] headers = options.Headers
            .Select(NormalizeHeader)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static header => header, StringComparer.Ordinal)
            .ToArray();
        string[] preambleHeaders = options.PreambleHeaders
            .Select(NormalizeHeader)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static header => header, StringComparer.Ordinal)
            .ToArray();

        if (headers.Length == 0)
        {
            throw new ArgumentException("At least one OCCT header is required.", nameof(options));
        }

        foreach (string header in preambleHeaders.Concat(headers))
        {
            string fullHeader = Path.GetFullPath(Path.Combine(includeRoot, header));
            if (!fullHeader.StartsWith(includeRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || !File.Exists(fullHeader))
            {
                throw new FileNotFoundException($"OCCT header '{header}' was not found under '{includeRoot}'.");
            }
        }

        string sourcePath = Path.Combine(Path.GetTempPath(), $"occtsharp-{Guid.NewGuid():N}.cpp");
        File.WriteAllLines(
            sourcePath,
            preambleHeaders.Concat(headers).Select(static header => $"#include <{header}>"));

        try
        {
            return ParseTranslationUnit(
                includeRoot,
                sourcePath,
                headers,
                options.ToolkitByPackage ?? new Dictionary<string, string>(StringComparer.Ordinal));
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }

    private static DiscoveryReport ParseTranslationUnit(
        string includeRoot,
        string sourcePath,
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, string> toolkitByPackage)
    {
        string[] arguments = BuildCompilerArguments(includeRoot);

        using CXIndex index = CXIndex.Create(excludeDeclarationsFromPch: false, displayDiagnostics: false);
        CXTranslationUnit handle = CXTranslationUnit.Parse(
            index,
            sourcePath,
            arguments,
            ReadOnlySpan<CXUnsavedFile>.Empty,
            CXTranslationUnit_Flags.CXTranslationUnit_SkipFunctionBodies
            | CXTranslationUnit_Flags.CXTranslationUnit_KeepGoing);

        using TranslationUnit translationUnit = TranslationUnit.GetOrCreate(handle);

        DiscoveryDiagnostic[] diagnostics = ReadDiagnostics(handle);
        DiscoveryDiagnostic[] errors = diagnostics
            .Where(static diagnostic => diagnostic.Severity is "Error" or "Fatal")
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                "Clang failed to parse the selected OCCT headers:" + Environment.NewLine
                + string.Join(Environment.NewLine, errors.Select(static error => error.Message)));
        }

        List<BindingDeclaration> declarations = [];
        CollectDeclarations(
            translationUnit.TranslationUnitDecl.Decls,
            includeRoot,
            toolkitByPackage,
            declarations);

        string occtVersion = ReadOcctVersion(Path.Combine(includeRoot, "Standard_Version.hxx"));

        BindingModel classifiedModel = SupportClassificationPass.Apply(new BindingModel(declarations));
        BindingModel valueEligibleModel = SimpleBindingEligibilityPass.Apply(classifiedModel);
        BindingModel eligibleModel = SharedHandleBindingEligibilityPass.Apply(valueEligibleModel);

        return new DiscoveryReport(
            "1.3",
            occtVersion,
            "ClangSharp/libClangSharp 21.1.8",
            headers,
            diagnostics,
            eligibleModel,
            BindingSupportSummary.Create(eligibleModel));
    }

    private static string[] BuildCompilerArguments(string includeRoot)
    {
        List<string> arguments =
        [
            "-x",
            "c++",
            "-std=c++20",
            "-fms-compatibility",
            "-fms-extensions",
            "-fms-compatibility-version=19.51",
            "-D_WIN32",
            "-DWIN32",
            "-D_WINDOWS",
            $"-I{includeRoot}",
        ];

        string? includeEnvironment = Environment.GetEnvironmentVariable("INCLUDE");
        if (!string.IsNullOrWhiteSpace(includeEnvironment))
        {
            foreach (string include in includeEnvironment.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Directory.Exists(include))
                {
                    arguments.Add("-isystem");
                    arguments.Add(include);
                }
            }
        }

        return [.. arguments];
    }

    private static DiscoveryDiagnostic[] ReadDiagnostics(CXTranslationUnit translationUnit)
    {
        List<DiscoveryDiagnostic> diagnostics = [];
        CXDiagnosticDisplayOptions displayOptions =
            CXDiagnosticDisplayOptions.CXDiagnostic_DisplaySourceLocation
            | CXDiagnosticDisplayOptions.CXDiagnostic_DisplayColumn
            | CXDiagnosticDisplayOptions.CXDiagnostic_DisplayCategoryName;

        for (uint index = 0; index < translationUnit.NumDiagnostics; index++)
        {
            using CXDiagnostic diagnostic = translationUnit.GetDiagnostic(index);
            diagnostics.Add(new DiscoveryDiagnostic(
                diagnostic.Severity.ToString().Replace("CXDiagnostic_", string.Empty, StringComparison.Ordinal),
                diagnostic.Format(displayOptions).CString));
        }

        return [.. diagnostics.OrderBy(static diagnostic => diagnostic.Message, StringComparer.Ordinal)];
    }

    private static void CollectDeclarations(
        IReadOnlyList<Decl> source,
        string includeRoot,
        IReadOnlyDictionary<string, string> toolkitByPackage,
        ICollection<BindingDeclaration> target)
    {
        foreach (Decl declaration in source)
        {
            if (TryCreateDeclaration(
                declaration,
                includeRoot,
                toolkitByPackage,
                out BindingDeclaration? bindingDeclaration))
            {
                target.Add(bindingDeclaration!);
            }

            if (declaration.Decls.Count > 0)
            {
                CollectDeclarations(declaration.Decls, includeRoot, toolkitByPackage, target);
            }
        }
    }

    private static bool TryCreateDeclaration(
        Decl declaration,
        string includeRoot,
        IReadOnlyDictionary<string, string> toolkitByPackage,
        out BindingDeclaration? bindingDeclaration)
    {
        bindingDeclaration = null;

        if (declaration is not NamedDecl namedDeclaration
            || string.IsNullOrWhiteSpace(namedDeclaration.Name)
            || (!declaration.IsCanonicalDecl && !HasResolvedDefinition(declaration)))
        {
            return false;
        }

        BindingDeclarationKind? kind = declaration switch
        {
            CXXConstructorDecl => BindingDeclarationKind.Constructor,
            CXXMethodDecl => BindingDeclarationKind.Method,
            CXXRecordDecl => BindingDeclarationKind.Record,
            EnumDecl => BindingDeclarationKind.Enum,
            FunctionDecl => BindingDeclarationKind.Function,
            _ => null,
        };

        if (kind is null)
        {
            return false;
        }

        Decl factDeclaration = declaration;
        if (declaration is CXXRecordDecl record && record.Definition is { } definition)
        {
            factDeclaration = definition;
        }
        else if (declaration is EnumDecl enumDeclaration && enumDeclaration.Definition is { } resolvedEnumDefinition)
        {
            factDeclaration = resolvedEnumDefinition;
        }
        factDeclaration.Location.GetSpellingLocation(out CXFile file, out uint line, out uint column, out _);
        string filePath = file.Name.CString;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        string normalizedFile = Path.GetFullPath(filePath);
        if (!normalizedFile.StartsWith(includeRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string header = Path.GetRelativePath(includeRoot, normalizedFile).Replace('\\', '/');
        string qualifiedName = string.IsNullOrWhiteSpace(namedDeclaration.QualifiedName)
            ? namedDeclaration.Name
            : namedDeclaration.QualifiedName;
        string usr = declaration.Handle.Usr.CString;
        string stableId = string.IsNullOrWhiteSpace(usr)
            ? $"{kind}:{qualifiedName}@{header}:{line}:{column}"
            : usr;
        string sourcePackage = GetSourcePackage(header);
        toolkitByPackage.TryGetValue(sourcePackage, out string? sourceToolkit);

        FunctionDecl? function = factDeclaration as FunctionDecl;
        CXXMethodDecl? method = factDeclaration as CXXMethodDecl;
        CXXRecordDecl? recordDefinition = factDeclaration as CXXRecordDecl;
        EnumDecl? enumDefinition = factDeclaration as EnumDecl;
        BindingParameter[] parameters = function is null
            ? []
            : function.Parameters
                .Select(static (parameter, position) => new BindingParameter(
                    position,
                    parameter.Name,
                    CreateBindingType(parameter.Type),
                    parameter.HasDefaultArg))
                .ToArray();
        BindingBaseType[] baseTypes = recordDefinition is null
            ? []
            : recordDefinition.Bases
                .Select(static baseType => new BindingBaseType(
                    CreateBindingType(baseType.Type),
                    MapAccess(baseType.AccessSpecifier),
                    baseType.IsVirtual))
                .OrderBy(static baseType => baseType.Type.CanonicalSpelling, StringComparer.Ordinal)
                .ToArray();
        BindingEnumValue[] enumValues = enumDefinition is null
            ? []
            : enumDefinition.Enumerators
                .Select(static enumerator => new BindingEnumValue(
                    enumerator.Name,
                    enumerator.IsUnsigned
                        ? enumerator.UnsignedInitVal.ToString(CultureInfo.InvariantCulture)
                        : enumerator.InitVal.ToString(CultureInfo.InvariantCulture),
                    enumerator.IsUnsigned))
                .ToArray();
        bool isTemplated = factDeclaration.IsTemplated;
        uint templateParameterListCount = factDeclaration switch
        {
            FunctionDecl functionDeclaration => functionDeclaration.NumTemplateParameterLists,
            CXXRecordDecl recordDeclaration => recordDeclaration.NumTemplateParameterLists,
            _ => 0,
        };
        CX_TemplateSpecializationKind specializationKind = factDeclaration switch
        {
            FunctionDecl functionDeclaration => functionDeclaration.TemplateSpecializationKind,
            CXXRecordDecl recordDeclaration => recordDeclaration.TemplateSpecializationKind,
            _ => CX_TemplateSpecializationKind.CX_TSK_Undeclared,
        };

        bindingDeclaration = new BindingDeclaration(
            stableId,
            qualifiedName,
            kind.Value,
            header,
            checked((int)line),
            checked((int)column))
        {
            NativeSignature = CreateNativeSignature(qualifiedName, function),
            SourcePackage = sourcePackage,
            SourceToolkit = sourceToolkit,
            ProductModule = string.IsNullOrWhiteSpace(sourcePackage)
                ? OcctProductModule.Unassigned
                : OcctProductModuleClassifier.ClassifyDeclaration(
                    sourcePackage,
                    qualifiedName,
                    sourceToolkit),
            Access = MapAccess(factDeclaration.Access),
            ReturnType = function is null || factDeclaration is CXXConstructorDecl
                ? null
                : CreateBindingType(function.ReturnType),
            Parameters = parameters,
            BaseTypes = baseTypes,
            EnumValues = enumValues,
            EnumUnderlyingType = enumDefinition?.IntegerType.AsString,
            IsConst = method?.IsConst ?? false,
            IsStatic = function?.IsStatic ?? false,
            IsVariadic = function?.IsVariadic ?? false,
            IsVirtual = method?.IsVirtual ?? false,
            IsPureVirtual = method?.IsPure ?? false,
            IsAbstract = recordDefinition?.IsAbstract ?? false,
            IsTemplated = isTemplated,
            TemplateParameterListCount = checked((int)templateParameterListCount),
            TemplateSpecializationKind = specializationKind.ToString()
                .Replace("CX_TSK_", string.Empty, StringComparison.Ordinal),
            IsDeleted = function?.IsDeleted ?? false,
            IsUnavailable = factDeclaration.IsUnavailable,
            IsDeprecated = factDeclaration.IsDeprecated,
            IsOverloadedOperator = function?.IsOverloadedOperator ?? false,
        };
        return true;
    }

    private static bool HasResolvedDefinition(Decl declaration) => declaration switch
    {
        CXXRecordDecl { Definition: not null } => true,
        EnumDecl { Definition: not null } => true,
        _ => false,
    };

    private static BindingType CreateBindingType(ClangSharp.Type type)
    {
        List<BindingTypeLayer> layers = [];
        ClangSharp.Type current = type;

        while (true)
        {
            switch (current)
            {
                case LValueReferenceType reference:
                    layers.Add(new BindingTypeLayer(
                        BindingTypeLayerKind.LValueReference,
                        current.IsLocalConstQualified));
                    current = reference.PointeeType;
                    continue;
                case RValueReferenceType reference:
                    layers.Add(new BindingTypeLayer(
                        BindingTypeLayerKind.RValueReference,
                        current.IsLocalConstQualified));
                    current = reference.PointeeType;
                    continue;
                case PointerType pointer:
                    layers.Add(new BindingTypeLayer(
                        BindingTypeLayerKind.PointerIndirection,
                        current.IsLocalConstQualified));
                    current = pointer.PointeeType;
                    continue;
                default:
                    layers.Add(new BindingTypeLayer(
                        BindingTypeLayerKind.Value,
                        current.IsLocalConstQualified));
                    break;
            }

            break;
        }

        TemplateSpecializationType? template = FindTemplateSpecialization(current);
        string? templateName = template?.TemplateName.AsTemplateDecl?.QualifiedName;
        BindingTemplateArgument[] templateArguments = template is null
            ? []
            : template.Args.Select(CreateTemplateArgument).ToArray();
        bool isOcctHandle = string.Equals(templateName, "opencascade::handle", StringComparison.Ordinal)
            || (templateName?.EndsWith("::handle", StringComparison.Ordinal) ?? false);
        string? handleTargetType = isOcctHandle
            ? templateArguments.FirstOrDefault(static argument => argument.Kind == "Type")?.Spelling
            : null;

        return new BindingType(
            type.AsString,
            type.CanonicalType.AsString,
            current.AsString,
            current.CanonicalType.AsString,
            layers,
            templateName,
            templateArguments,
            isOcctHandle,
            handleTargetType);
    }

    private static TemplateSpecializationType? FindTemplateSpecialization(ClangSharp.Type type)
    {
        ClangSharp.Type current = type;
        for (int depth = 0; depth < 16; depth++)
        {
            if (current is TemplateSpecializationType template)
            {
                return template;
            }

            ClangSharp.Type? next = current switch
            {
                ElaboratedType elaborated => elaborated.NamedType,
                TypedefType typedef => typedef.Desugar,
                _ when current.IsSugared => current.Desugar,
                _ => null,
            };
            if (next is null || next.Handle.Equals(current.Handle))
            {
                return null;
            }

            current = next;
        }

        return null;
    }

    private static BindingTemplateArgument CreateTemplateArgument(TemplateArgument argument)
    {
        return argument.Kind switch
        {
            CXTemplateArgumentKind.CXTemplateArgumentKind_Type =>
                new BindingTemplateArgument("Type", argument.AsType.AsString),
            CXTemplateArgumentKind.CXTemplateArgumentKind_Declaration =>
                new BindingTemplateArgument("Declaration", argument.AsDecl.QualifiedName),
            CXTemplateArgumentKind.CXTemplateArgumentKind_Integral =>
                new BindingTemplateArgument("Integral", argument.AsIntegral.ToString(CultureInfo.InvariantCulture)),
            CXTemplateArgumentKind.CXTemplateArgumentKind_Template =>
                new BindingTemplateArgument(
                    "Template",
                    argument.AsTemplate.AsTemplateDecl?.QualifiedName ?? string.Empty),
            CXTemplateArgumentKind.CXTemplateArgumentKind_Pack =>
                new BindingTemplateArgument(
                    "Pack",
                    string.Join(",", argument.PackElements.Select(CreateTemplateArgument).Select(static item => item.Spelling))),
            _ => new BindingTemplateArgument(
                argument.Kind.ToString().Replace("CXTemplateArgumentKind_", string.Empty, StringComparison.Ordinal),
                string.Empty),
        };
    }

    private static BindingAccess MapAccess(CX_CXXAccessSpecifier access)
    {
        return access switch
        {
            CX_CXXAccessSpecifier.CX_CXXPublic => BindingAccess.Public,
            CX_CXXAccessSpecifier.CX_CXXProtected => BindingAccess.Protected,
            CX_CXXAccessSpecifier.CX_CXXPrivate => BindingAccess.Private,
            _ => BindingAccess.None,
        };
    }

    private static string CreateNativeSignature(string qualifiedName, FunctionDecl? function)
    {
        return function is null ? qualifiedName : $"{qualifiedName}: {function.Type.AsString}";
    }

    private static string GetSourcePackage(string header)
    {
        string fileName = Path.GetFileNameWithoutExtension(header);
        int separator = fileName.IndexOf('_');
        return separator > 0 ? fileName[..separator] : fileName;
    }

    private static string NormalizeHeader(string header)
    {
        string normalized = header.Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || Path.IsPathRooted(normalized)
            || normalized.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Header path '{header}' is not a safe relative path.", nameof(header));
        }

        return normalized;
    }

    private static string ReadOcctVersion(string versionHeader)
    {
        const string prefix = "#define OCC_VERSION_COMPLETE \"";
        string? line = File.ReadLines(versionHeader)
            .FirstOrDefault(static line => line.StartsWith(prefix, StringComparison.Ordinal));

        if (line is null || !line.EndsWith('"'))
        {
            throw new InvalidDataException("OCC_VERSION_COMPLETE was not found in Standard_Version.hxx.");
        }

        return line[prefix.Length..^1];
    }
}
