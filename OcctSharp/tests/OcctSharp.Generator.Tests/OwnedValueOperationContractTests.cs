using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Tests;

public sealed class OwnedValueOperationContractTests
{
    [Fact]
    public void ExactAuditedDeclarationRejectsSignatureIdentityAndAvailabilityDrift()
    {
        BindingDeclaration method = BoxGet();
        Assert.True(OwnedValueOperationContract.Contains(method));
        BindingDeclaration[] rejected =
        [
            method with { StableId = "unreviewed:mutable-reference" },
            method with { NativeSignature = method.NativeSignature + " changed" },
            method with { NativeName = "Bnd_Box::Unreviewed" },
            method with { SupportState = BindingSupportState.Manual },
            method with { SupportState = BindingSupportState.Skipped },
            method with { SkipReason = new("SK008", "ArtifactUnavailable", "No pinned SDK symbol") },
            method with { IsUnavailable = true },
            method with { IsDeleted = true },
            method with { IsTemplated = true },
            method with { IsPureVirtual = true },
            method with { IsVariadic = true },
            method with { IsOverloadedOperator = true },
            method with { Access = BindingAccess.Private },
            method with { Kind = BindingDeclarationKind.Function },
        ];
        Assert.All(rejected, declaration => Assert.False(OwnedValueOperationContract.Contains(declaration)));
    }

    [Fact]
    public void AuditedMutableCopiesRemainParametersOnlyAndDoNotBroadenInitialTypeMap()
    {
        InitialTypeMap map = new(["GccEnt_Position"]);
        foreach (string native in new[] { "double", "int", "bool", "gp_Pnt", "gp_XYZ", "GccEnt_Position" })
        {
            BindingType reference = Type(native, reference: true);
            Assert.False(map.TryMap(reference, BindingTypeUsage.Parameter, out _));
            Assert.True(OwnedValueOperationContract.TryMap(BoxGet(), reference, BindingTypeUsage.Parameter, map, out var projection));
            Assert.Equal("ValueCopy", projection!.Ownership);
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), reference, BindingTypeUsage.ReturnValue, map, out _));
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet() with { StableId = "unlisted" }, reference,
                BindingTypeUsage.Parameter, map, out _));
        }
    }

    [Fact]
    public void ConstOwnerInputsLeaseAcrossAuditedTypesWithoutCopyingAlgorithms()
    {
        InitialTypeMap map = new();
        foreach (string owner in new[] { "Bnd_Box", "GccEnt_QualifiedCirc", "GeomAPI_ProjectPointOnCurve" })
        {
            Assert.True(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner, reference: true, isConst: true),
                BindingTypeUsage.Parameter, map, out var projection));
            Assert.Equal("TM010", projection!.RuleId);
            Assert.Equal("OwnedValue", projection.Ownership);
            Assert.Equal("OcctSharp_Owned_" + owner + "Handle*", projection.AbiType);
            Assert.Equal(OwnedValueOperationContract.ManagedName(owner) + ".OwnedHandle", projection.ManagedRawType);
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner, reference: true),
                BindingTypeUsage.Parameter, map, out _));
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner), BindingTypeUsage.Parameter, map, out _));
        }
    }

    [Fact]
    public void OnlyAuditedSimpleValuesCanCreateIndependentOwningResults()
    {
        InitialTypeMap map = new();
        foreach (string owner in new[] { "Bnd_Box", "GProp_PrincipalProps", "Extrema_POnCurv", "IntAna2d_IntPoint" })
        {
            Assert.True(OwnedValueOperationContract.IsCopyable(owner));
            Assert.True(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner), BindingTypeUsage.ReturnValue, map, out _));
            Assert.True(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner, reference: true, isConst: true),
                BindingTypeUsage.ReturnValue, map, out _));
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner, reference: true),
                BindingTypeUsage.ReturnValue, map, out _));
        }
        foreach (string owner in new[] { "GeomAPI_ProjectPointOnCurve", "Extrema_ExtElCS", "GC_MakeCircle", "IntAna_IntQuadQuad" })
        {
            Assert.False(OwnedValueOperationContract.IsCopyable(owner));
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner), BindingTypeUsage.ReturnValue, map, out _));
            Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), Type(owner, reference: true, isConst: true),
                BindingTypeUsage.ReturnValue, map, out _));
        }
    }

    [Fact]
    public void SharedInputsAndResultsKeepIntrusiveOwnershipAndRejectMutableHandleReferences()
    {
        InitialTypeMap map = new();
        BindingType shared = Type("opencascade::handle<Geom_Curve>") with
            { IsOcctHandle = true, HandleTargetType = "Geom_Curve" };
        Assert.True(OwnedValueOperationContract.TryMap(BoxGet(), shared, BindingTypeUsage.Parameter, map, out var parameter));
        Assert.Equal("Shared", parameter!.Ownership);
        Assert.True(OwnedValueOperationContract.TryMap(BoxGet(), shared, BindingTypeUsage.ReturnValue, map, out var result));
        Assert.Equal("Shared", result!.Ownership);
        BindingType mutable = shared with { Layers = [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, false)] };
        Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), mutable, BindingTypeUsage.Parameter, map, out _));
        Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), mutable, BindingTypeUsage.ReturnValue, map, out _));
    }

    [Fact]
    public void PointersArraysAndBorrowedAdaptorsHaveNoOwnedProjection()
    {
        InitialTypeMap map = new();
        BindingType pointer = Type("double") with
            { Layers = [new(BindingTypeLayerKind.PointerIndirection, false), new(BindingTypeLayerKind.Value, false)] };
        Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), pointer, BindingTypeUsage.Parameter, map, out _));
        Assert.False(OwnedValueOperationContract.TryMap(BoxGet(), Type("NCollection_Array1<double>", reference: true, isConst: true),
            BindingTypeUsage.Parameter, map, out _));
        Assert.DoesNotContain("Extrema_ExtPC", OwnedValueOperationContract.Owners);
        Assert.DoesNotContain("Extrema_ExtSS", OwnedValueOperationContract.Owners);
        Assert.DoesNotContain("IntAna_Curve", OwnedValueOperationContract.Owners);
    }

    [Fact]
    public void SupportingFactoriesLeaveManualIdsAndUndefinedDefaultsOutsideMigration()
    {
        Assert.Contains("Bnd_Box2d", OwnedValueOperationContract.SupportingDefaultOwners);
        Assert.Contains("Bnd_OBB", OwnedValueOperationContract.SupportingDefaultOwners);
        Assert.DoesNotContain("c:@S@Bnd_OBB@F@Bnd_OBB#", OwnedValueOperationContract.StableIds);
        foreach (string owner in new[] { "GProp_PrincipalProps", "gp_QuaternionSLerp", "gp_QuaternionNLerp", "IntAna_Quadric", "IntAna2d_IntPoint" })
        {
            Assert.DoesNotContain(owner, OwnedValueOperationContract.SupportingDefaultOwners);
            Assert.DoesNotContain("c:@S@" + owner + "@F@" + owner + "#", OwnedValueOperationContract.StableIds);
        }
        Assert.DoesNotContain("GC_Root", OwnedValueOperationContract.Owners);
        Assert.DoesNotContain("gce_Root", OwnedValueOperationContract.Owners);
        Assert.DoesNotContain("c:@S@Bnd_Box@F@Bnd_Box#&1$@S@Bnd_Box#", OwnedValueOperationContract.StableIds);
    }

    private static BindingDeclaration BoxGet() => new("c:@S@Bnd_Box@F@Get#&d#S0_#S0_#S0_#S0_#S0_#1",
        "Bnd_Box::Get", BindingDeclarationKind.Method, "Bnd_Box.hxx", 1, 1)
    {
        NativeSignature = "Bnd_Box::Get: void (double &, double &, double &, double &, double &, double &) const",
        Access = BindingAccess.Public,
        ProductModule = OcctProductModule.Geometry,
        ReturnType = Type("void"),
        IsConst = true,
    };

    private static BindingType Type(string native, bool reference = false, bool isConst = false) => new(
        (isConst ? "const " : "") + native + (reference ? " &" : ""),
        (isConst ? "const " : "") + native + (reference ? " &" : ""),
        (isConst ? "const " : "") + native, (isConst ? "const " : "") + native,
        reference ? [new(BindingTypeLayerKind.LValueReference, false), new(BindingTypeLayerKind.Value, isConst)]
            : [new(BindingTypeLayerKind.Value, isConst)], null, [], false, null);
}
