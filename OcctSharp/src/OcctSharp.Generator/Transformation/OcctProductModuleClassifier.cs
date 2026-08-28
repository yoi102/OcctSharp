using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Transformation;

public static class OcctProductModuleClassifier
{
    private static readonly (OcctProductModule Module, string[] Prefixes)[] PackageRules =
    [
        (OcctProductModule.IVtk, ["IVtk"]),
        (OcctProductModule.OpenGles, ["OpenGles"]),
        (OcctProductModule.Draw,
        [
            "DBRep", "DDataStd", "DDF", "DDocStd", "DNaming", "DPrsStd", "Draw", "MeshTest", "QADNaming",
            "QADraw", "QABugs", "SWDRAW", "ViewerTest", "XDEDRAW", "XSDRAW",
        ]),
        (OcctProductModule.Xde, ["BinMXCAF", "BinXCAF", "DEXCAF", "STEPCAF", "XCAF", "XmlMXCAF", "XmlXCAF"]),
        (OcctProductModule.Mesh, ["BRepMesh", "IMesh", "Poly", "XBRepMesh"]),
        (OcctProductModule.Documents,
        [
            "AppStd", "BinDrivers", "BinLDrivers", "BinMData", "BinMDF", "BinMDocStd", "BinMFunction",
            "BinMNaming", "BinObjMgt", "BinTObj", "BinTools", "CDF", "CDM", "FSD", "LDOM", "PCDM",
            "ShapePersistent", "StdDrivers", "StdLDrivers", "StdObject", "StdPersistent", "StdLPersistent",
            "StdObjMgt", "StdStorage", "Storage", "TData", "TDF", "TDocStd", "TFunction", "TNaming", "TObj",
            "XmlDrivers", "XmlLDrivers", "XmlMData", "XmlMDF", "XmlMDocStd", "XmlMFunction", "XmlMNaming",
            "XmlObjMgt", "XmlTObj",
        ]),
        (OcctProductModule.DataExchange,
        [
            "APIHeaderSection", "DE", "HeaderSection", "IFGraph", "IFSelect", "IGES", "Interface", "MoniTool",
            "RW", "Step", "STEP", "StlAPI", "Transfer", "TopoDSToStep", "Vrml", "XS", "igesread", "step.tab",
        ]),
        (OcctProductModule.Visualization,
        [
            "AIS", "Aspect", "Cocoa", "D3DHost", "DsgPrs", "Font", "Graphic3d", "Image", "Media", "MeshVS",
            "OpenGl", "Prs", "Select", "StdPrs", "StdSelect", "TPrsStd", "V3d", "Wasm", "WNT", "Xw",
        ]),
        (OcctProductModule.Foundation,
        [
            "BVH", "Expr", "FEmTool", "FlexLexer", "Math", "Message", "NCollection", "OSD", "Plugin", "Precision",
            "Quantity", "Resource", "Standard", "StdFail", "TColStd", "TCollection", "UTL", "Units", "math",
        ]),
        (OcctProductModule.Geometry,
        [
            "Adaptor", "AdvApp2Var", "AdvApprox", "AppBlend", "AppCont", "AppDef", "AppParCurves", "Approx",
            "Bisector", "BiTgte", "Bnd", "BSplCLib", "BSplSLib", "Convert", "CPnts", "CSLib", "ElCLib", "ElSLib",
            "Extrema", "FairCurve", "GC", "Gcc", "gce", "GCPnts", "Geom", "gp", "GProp", "Hatch", "Hermit",
            "LProp", "MAT", "NLPlate", "Plate", "PLib", "ProjLib",
        ]),
        (OcctProductModule.Modeling,
        [
            "Blend", "BOP", "BRep", "ChFi", "Contap", "Draft", "FilletSurf", "Helix", "HLR", "Int", "Intrv",
            "Law", "LocalAnalysis", "LocOpe", "Shape", "Sweep", "Top", "TopOpe",
        ]),
    ];

    public static OcctProductModule Classify(string sourcePackage, string? sourceToolkit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePackage);

        foreach ((OcctProductModule module, string[] prefixes) in PackageRules)
        {
            if (prefixes.Any(prefix => sourcePackage.StartsWith(prefix, StringComparison.Ordinal)))
            {
                return module;
            }
        }

        return ClassifyToolkit(sourceToolkit);
    }

    public static OcctProductModule ClassifyOrThrow(string sourcePackage, string? sourceToolkit = null)
    {
        OcctProductModule module = Classify(sourcePackage, sourceToolkit);
        return module != OcctProductModule.Unassigned
            ? module
            : throw new InvalidDataException(
                $"OCCT source package '{sourcePackage}' in toolkit '{sourceToolkit ?? "(unknown)"}' has no product-module assignment.");
    }

    private static OcctProductModule ClassifyToolkit(string? toolkit) => toolkit switch
    {
        "TKernel" => OcctProductModule.Foundation,
        "TKMath" or "TKG2d" or "TKG3d" or "TKGeomBase" or "TKGeomAlgo" => OcctProductModule.Geometry,
        "TKBRep" or "TKTopAlgo" or "TKPrim" or "TKBO" or "TKShHealing" or "TKBool" or "TKHLR" or
            "TKHelix" or "TKFillet" or "TKOffset" or "TKFeat" or "TKExpress" => OcctProductModule.Modeling,
        "TKMesh" or "TKXMesh" => OcctProductModule.Mesh,
        "TKCDF" or "TKLCAF" or "TKCAF" or "TKBinL" or "TKXmlL" or "TKBin" or "TKXml" or
            "TKStdL" or "TKStd" or "TKTObj" or "TKBinTObj" or "TKXmlTObj" => OcctProductModule.Documents,
        "TKDE" or "TKXSBase" or "TKDESTEP" or "TKDEIGES" or "TKDESTL" or "TKDEVRML" or "TKRWMesh" or
            "TKDECascade" or "TKDEOBJ" or "TKDEGLTF" or "TKDEPLY" => OcctProductModule.DataExchange,
        "TKXCAF" or "TKBinXCAF" or "TKXmlXCAF" => OcctProductModule.Xde,
        "TKService" or "TKV3d" or "TKOpenGl" or "TKMeshVS" or "TKD3DHost" or "TKVCAF" => OcctProductModule.Visualization,
        "TKIVtk" => OcctProductModule.IVtk,
        "TKOpenGles" => OcctProductModule.OpenGles,
        null or "" => OcctProductModule.Unassigned,
        _ when toolkit.Contains("Draw", StringComparison.OrdinalIgnoreCase) || toolkit.EndsWith("Test", StringComparison.Ordinal) => OcctProductModule.Draw,
        _ => OcctProductModule.Unassigned,
    };
}
