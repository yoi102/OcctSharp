namespace OcctSharp.Runtime.Tests;

public sealed class GeneratedStepModelHandleTests
{
    [Fact]
    public void GeneratedStepGeometryAndRepresentationHandlesRetainRuntimeType()
    {
        using StepGeomCartesianPoint point = new();
        point.SetNbCoordinates(3);
        Assert.Equal(3, point.NbCoordinates());
        Assert.Equal("StepGeom_CartesianPoint", point.TypeName);
        Assert.True(point.IsKind("StepRepr_RepresentationItem"));

        using StepGeomCartesianPoint clone = point.Clone();
        Assert.Equal(2, point.ReferenceCount);
        Assert.Equal(2, clone.ReferenceCount);

        using StepReprRepresentationItem item = new();
        Assert.Equal("StepRepr_RepresentationItem", item.TypeName);
        Assert.True(item.IsKind("Standard_Transient"));
    }

    [Fact]
    public void GeneratedStepShapeAndVisualScalarMembersRoundTripAndDisposeSafely()
    {
        using StepShapeBoxDomain box = new();
        box.SetXlength(12.5);
        box.SetYlength(8.25);
        box.SetZlength(3.75);
        Assert.Equal(12.5, box.Xlength(), 12);
        Assert.Equal(8.25, box.Ylength(), 12);
        Assert.Equal(3.75, box.Zlength(), 12);
        Assert.Equal("StepShape_BoxDomain", box.TypeName);
        Assert.True(box.IsKind("Standard_Transient"));

        StepVisualColourRgb colour = new();
        colour.SetRed(0.2);
        colour.SetGreen(0.4);
        colour.SetBlue(0.8);
        Assert.Equal(0.2, colour.Red(), 12);
        Assert.Equal(0.4, colour.Green(), 12);
        Assert.Equal(0.8, colour.Blue(), 12);
        colour.Dispose();
        colour.Dispose();
        Assert.Throws<ObjectDisposedException>(() => colour.Red());
    }

    [Fact]
    public void GeneratedCrossTypeHandlesRoundTripNullAndRetainIndependentReferences()
    {
        using StepBasicAction action = new();
        Assert.Null(action.ChosenMethod());

        StepBasicActionMethod method = new();
        action.SetChosenMethod(method);
        using StepBasicActionMethod returned = Assert.IsType<StepBasicActionMethod>(action.ChosenMethod());
        method.Dispose();
        Assert.Equal("StepBasic_ActionMethod", returned.TypeName);

        action.SetChosenMethod(null);
        Assert.Null(action.ChosenMethod());

        using StepBasicActionAssignment assignment = new();
        StepBasicAction assigned = new();
        assignment.Init(assigned);
        using StepBasicAction retained = Assert.IsType<StepBasicAction>(assignment.AssignedAction());
        assigned.Dispose();
        Assert.Equal("StepBasic_Action", retained.TypeName);

        StepBasicAction disposed = new();
        disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => assignment.SetAssignedAction(disposed));
    }

    [Fact]
    public void GeneratedExtendedStepFamiliesConstructCloneAndRetainAllPublicTypes()
    {
        (string Prefix, int ExpectedCount)[] families =
        [
            ("StepAP203", 11),
            ("StepAP214", 27),
            ("StepAP242", 4),
            ("StepDimTol", 50),
            ("StepElement", 21),
            ("StepFEA", 55),
            ("StepKinematics", 81),
        ];
        Type[] exportedTypes = typeof(StepAP203Change).Assembly.GetExportedTypes();

        foreach ((string prefix, int expectedCount) in families)
        {
            Type[] familyTypes = exportedTypes
                .Where(type => type.IsClass
                    && type.Name.StartsWith(prefix, StringComparison.Ordinal)
                    && typeof(IDisposable).IsAssignableFrom(type)
                    && type.GetConstructor(Type.EmptyTypes) is not null)
                .OrderBy(static type => type.FullName, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expectedCount, familyTypes.Length);

            foreach (Type type in familyTypes)
            {
                using IDisposable instance = Assert.IsAssignableFrom<IDisposable>(Activator.CreateInstance(type));
                using IDisposable clone = Assert.IsAssignableFrom<IDisposable>(
                    type.GetMethod("Clone")!.Invoke(instance, null));
                Assert.Equal(2, type.GetProperty("ReferenceCount")!.GetValue(instance));
                Assert.StartsWith("Step", Assert.IsType<string>(type.GetProperty("TypeName")!.GetValue(instance)));
            }
        }
    }

    [Fact]
    public void GeneratedExtendedStepMembersRoundTripValuesAndCrossPackageHandles()
    {
        using StepAP214RepItemGroup group = new();
        StepReprRepresentationItem item = new();
        group.SetRepresentationItem(item);
        using StepReprRepresentationItem retainedItem = Assert.IsType<StepReprRepresentationItem>(
            group.RepresentationItem());
        item.Dispose();
        Assert.Equal("StepRepr_RepresentationItem", retainedItem.TypeName);
        group.SetRepresentationItem(null);
        Assert.Null(group.RepresentationItem());

        using StepElementCurveElementEndReleasePacket release = new();
        release.SetReleaseStiffness(125.5);
        Assert.Equal(125.5, release.ReleaseStiffness(), 12);

        using StepFEACurveElementLocation location = new();
        StepFEAFeaParametricPoint coordinate = new();
        location.SetCoordinate(coordinate);
        using StepFEAFeaParametricPoint retainedCoordinate = Assert.IsType<StepFEAFeaParametricPoint>(
            location.Coordinate());
        coordinate.Dispose();
        Assert.Equal("StepFEA_FeaParametricPoint", retainedCoordinate.TypeName);

        using StepKinematicsGearPairValue gear = new();
        gear.SetActualRotation1(0.75);
        Assert.Equal(0.75, gear.ActualRotation1(), 12);
    }
}
