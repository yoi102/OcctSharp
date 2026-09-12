using OcctSharp.OwnedValues;
using OcctSharp.ValueOperations;
using V = OcctSharp.Values;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchACOtherTests
{
    [Fact]
    public void StepSelectRetainsTypedEntityThroughSourceAndContainerDisposal()
    {
        using StepBasicDateTimeSelect selection = StepBasicDateTimeSelect.Create();
        Assert.True(selection.IsNull());
        Assert.Null(selection.Date());
        Assert.Null(selection.Value());

        using (StepBasicDate source = new())
        {
            source.SetYearComponent(2026);
            selection.SetValue(source);
        }

        using StandardTransient generic = Assert.IsType<StandardTransient>(selection.Value());
        Assert.Equal(1, selection.CaseNum(generic));
        Assert.True(selection.Matches(generic));
        Assert.Null(selection.LocalTime());
        using StepBasicDate retained = Assert.IsType<StepBasicDate>(selection.Date());
        selection.Nullify();
        Assert.True(selection.IsNull());
        selection.Dispose();
        Assert.Equal(2026, retained.YearComponent());

        using StepBasicDateTimeSelect another = StepBasicDateTimeSelect.Create();
        another.SetValue(generic);
        retained.SetYearComponent(2031);
        using StepBasicDate shared = Assert.IsType<StepBasicDate>(another.Date());
        generic.Dispose();
        retained.Dispose();
        another.Dispose();
        Assert.Equal(2031, shared.YearComponent());
    }

    [Fact]
    public void StepSelectRejectsWrongEntityAndDisposedTypedInput()
    {
        using StepBasicPersonOrganizationSelect people = StepBasicPersonOrganizationSelect.Create();
        using StepBasicPerson person = new();
        people.SetValue(person);
        using StandardTransient wrongType = Assert.IsType<StandardTransient>(people.Value());
        using StepBasicDateTimeSelect dates = StepBasicDateTimeSelect.Create();
        Assert.Equal(0, dates.CaseNum(wrongType));
        Assert.False(dates.Matches(wrongType));
        Assert.Throws<OcctException>(() => dates.SetValue(wrongType));
        Assert.True(dates.IsNull());

        using StepBasicDate disposed = new();
        disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => dates.SetValue(disposed));
        Assert.Throws<ArgumentNullException>(() => dates.SetValue((StepBasicDate)null!));
        dates.Dispose();
        Assert.Throws<ObjectDisposedException>(() => dates.IsNull());
    }

    [Fact]
    public void GridOwnsIndependentColorsAndRejectsInvalidScaleWithoutMutation()
    {
        using AspectGridParams grid = AspectGridParams.Create();
        using QuantityColor input = QuantityColor.Create(0.2, 0.4, 0.6,
            QuantityTypeOfColor.Quantity_TOC_RGB);
        grid.SetColor(input);
        using QuantityColor earlier = grid.Color();
        input.SetValues(0.9, 0.8, 0.7, QuantityTypeOfColor.Quantity_TOC_RGB);
        input.Dispose();
        using QuantityColor stored = grid.Color();
        AssertRgb(stored, 0.2, 0.4, 0.6);

        grid.SetScale(0.25);
        Assert.Throws<OcctException>(() => grid.SetScale(-1));
        Assert.Equal(0.25, grid.Scale());
        grid.SetAngularDivisions(12);
        grid.SetArcRange(0, Math.PI / 2);
        Assert.True(grid.IsCircular());
        Assert.True(grid.IsArc());
        grid.Dispose();
        AssertRgb(earlier, 0.2, 0.4, 0.6);
    }

    [Fact]
    public void PhysicallyBasedMaterialCopiesRetainIndependentAlphaAndColor()
    {
        using QuantityColor color = QuantityColor.Create(0.1, 0.3, 0.7,
            QuantityTypeOfColor.Quantity_TOC_RGB);
        using QuantityColorRGBA rgba = QuantityColorRGBA.Create(color, 0.4f);
        using Graphic3dPBRMaterial pbr = Graphic3dPBRMaterial.Create();
        pbr.SetColor(rgba);
        pbr.SetMetallic(0.8f);
        pbr.SetRoughness(0.25f);
        using Graphic3dMaterialAspect material = Graphic3dMaterialAspect.Create();
        material.SetPBRMaterial(pbr);
        using Graphic3dPBRMaterial independent = material.PBRMaterial();

        rgba.SetAlpha(0.9f);
        pbr.SetAlpha(0.6f);
        rgba.Dispose();
        color.Dispose();
        pbr.Dispose();
        material.Dispose();
        Assert.Equal(0.4f, independent.Alpha(), 6);
        Assert.Equal(0.8f, independent.Metallic(), 6);
        Assert.Equal(0.25f, independent.NormalizedRoughness(), 6);
        using QuantityColorRGBA copied = independent.Color();
        independent.Dispose();
        using QuantityColor rgb = copied.GetRGB();
        copied.Dispose();
        AssertRgb(rgb, 0.1, 0.3, 0.7);
    }

    [Fact]
    public void VrmlLightCopiesColorAndTransformCopiesRotation()
    {
        using QuantityColor source = QuantityColor.Create(0.15, 0.35, 0.55,
            QuantityTypeOfColor.Quantity_TOC_RGB);
        using VrmlPointLight light = VrmlPointLight.Create(true, 0.75, source,
            new V.Vector3d(2, 3, 5));
        source.Dispose();
        using QuantityColor copy = light.Color();
        Assert.Equal(new V.Vector3d(2, 3, 5), light.Location());
        light.Dispose();
        AssertRgb(copy, 0.15, 0.35, 0.55);

        using VrmlSFRotation rotation = VrmlSFRotation.Create(0, 0, 1, Math.PI / 3);
        using VrmlTransform transform = VrmlTransform.Create();
        transform.SetRotation(rotation);
        using VrmlSFRotation retained = transform.Rotation();
        rotation.SetAngle(Math.PI);
        rotation.Dispose();
        transform.Dispose();
        Assert.Equal(Math.PI / 3, retained.Angle(), 12);
        Assert.Equal(1, retained.RotationZ());
    }

    [Fact]
    public void MeshUnitConversionPublishesPositionAndTransformationCopies()
    {
        using RWMeshCoordinateSystemConverter converter = RWMeshCoordinateSystemConverter.Create();
        converter.SetInputLengthUnit(0.001);
        converter.SetOutputLengthUnit(1);
        V.Coordinates3d position = new(1000, 2000, 3000);
        converter.TransformPosition(ref position);
        Assert.Equal(new V.Coordinates3d(1, 2, 3), position);

        V.Transform3d transform = Transform3dOperations.Create();
        Transform3dOperations.SetTranslation(ref transform, new V.Vector3d(1000, 2000, 3000));
        converter.TransformTransformation(ref transform);
        Assert.Equal((1d, 2d, 3d), (transform.M14, transform.M24, transform.M34));
        Assert.Equal((1d, 1d, 1d), (transform.M11, transform.M22, transform.M33));

        V.Coordinates3d invalid = new(double.NaN, 7, 11);
        Assert.Throws<OcctException>(() => converter.TransformPosition(ref invalid));
        Assert.True(double.IsNaN(invalid.X));
        Assert.Equal((7d, 11d), (invalid.Y, invalid.Z));
        converter.Dispose();
        V.Coordinates3d afterDisposal = new(13, 17, 19);
        Assert.Throws<ObjectDisposedException>(() => converter.TransformPosition(ref afterDisposal));
        Assert.Equal(new V.Coordinates3d(13, 17, 19), afterDisposal);
    }

    [Fact]
    public void MeshTriangleAndEdgePreserveIndexAndOrientationSemantics()
    {
        using PolyTriangle triangle = PolyTriangle.Create(3, 5, 7);
        triangle.Set(2, 11);
        int a = -1, b = -1, c = -1;
        triangle.Get(ref a, ref b, ref c);
        Assert.Equal((3, 11, 7), (a, b, c));
        Assert.Throws<OcctException>(() => triangle.Set(0, 99));
        Assert.Throws<OcctException>(() => triangle.Value(4));
        triangle.Get(ref a, ref b, ref c);
        Assert.Equal((3, 11, 7), (a, b, c));

        using BRepMeshEdge forward = BRepMeshEdge.Create(3, 7, BRepMeshDegreeOfFreedom.BRepMesh_Free);
        using BRepMeshEdge reverse = BRepMeshEdge.Create(7, 3, BRepMeshDegreeOfFreedom.BRepMesh_Free);
        Assert.True(forward.IsEqual(reverse));
        Assert.False(forward.IsSameOrientation(reverse));
        Assert.Equal((3, 7), (forward.FirstNode(), forward.LastNode()));
        reverse.Dispose();
        Assert.Throws<ObjectDisposedException>(() => forward.IsEqual(reverse));
    }

    private static void AssertRgb(QuantityColor value, double red, double green, double blue)
    {
        Assert.Equal(red, value.Red(), 6);
        Assert.Equal(green, value.Green(), 6);
        Assert.Equal(blue, value.Blue(), 6);
    }
}
