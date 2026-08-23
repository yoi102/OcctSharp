using System.Runtime.InteropServices;
using OcctSharp.Generated;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class RuntimeTests
{
    [Fact]
    public void RuntimeInfoReportsExpectedVersions()
    {
        OcctRuntimeInfo info = OcctRuntime.Info;

        Assert.Equal(new Version(1, 17), info.AbiVersion);
        Assert.Equal("0.18.0", info.BridgeVersion);
        Assert.Equal("8.0.1", info.OcctVersion);
    }

    [Fact]
    public void CreateBoxReturnsSixFaces()
    {
        using Shape shape = ShapeFactory.CreateBox(10, 20, 30);

        Assert.Equal(6, shape.FaceCount);
    }

    [Fact]
    public void InvalidBoxDimensionsAreRejected()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => ShapeFactory.CreateBox(0, 20, 30));

        Assert.Contains("greater than zero", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DisposeIsIdempotentAndAccessAfterDisposeFails()
    {
        Shape shape = ShapeFactory.CreateBox(1, 2, 3);

        shape.Dispose();
        shape.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = shape.FaceCount);
    }

    [Fact]
    public void NativeReleaseIsIdempotentAndStaleHandlesAreRejected()
    {
        NativeStatus createStatus = NativeMethods.CreateBox(1, 2, 3, out nint nativeShape);
        Assert.Equal(NativeStatus.Success, createStatus);
        Assert.NotEqual(nint.Zero, nativeShape);

        NativeStatus firstAccess = NativeMethods.GetFaceCount(new ShapeHandle(nativeShape), out int faceCount);
        Assert.Equal(NativeStatus.Success, firstAccess);
        Assert.Equal(6, faceCount);

        NativeMethods.ReleaseShape(nativeShape);
        NativeMethods.ReleaseShape(nativeShape);

        NativeStatus staleAccess = NativeMethods.GetFaceCount(new ShapeHandle(nativeShape), out _);
        Assert.Equal(NativeStatus.InvalidHandle, staleAccess);
        Assert.Contains("already released", Marshal.PtrToStringUTF8(NativeMethods.GetLastError()), StringComparison.OrdinalIgnoreCase);

        NativeStatus arbitraryAccess = NativeMethods.GetFaceCount(new ShapeHandle((nint)0x1234), out _);
        Assert.Equal(NativeStatus.InvalidHandle, arbitraryAccess);
    }

    [Fact]
    public void SharedTransientCloneRetainsReferenceUntilLastWrapper()
    {
        using SharedTransient first = SharedTransient.Create();
        Assert.False(first.IsNull);
        Assert.Equal(1, first.ReferenceCount);

        using SharedTransient second = first.Clone();
        Assert.Equal(2, first.ReferenceCount);
        Assert.Equal(2, second.ReferenceCount);

        first.Dispose();
        Assert.Equal(1, second.ReferenceCount);
        Assert.Throws<ObjectDisposedException>(() => _ = first.ReferenceCount);
    }

    [Fact]
    public void SharedTransientNullHandleCopiesAsNull()
    {
        using SharedTransient first = SharedTransient.CreateNull();
        using SharedTransient second = first.Clone();

        Assert.True(first.IsNull);
        Assert.True(second.IsNull);
        Assert.Equal(0, first.ReferenceCount);
        Assert.Equal(0, second.ReferenceCount);
    }

    [Fact]
    public void SharedTransientReportsRuntimeTypeAndBaseKind()
    {
        using SharedTransient derived = SharedTransient.CreateDerived();

        Assert.Equal("OcctSharp_TransientDerived", derived.TypeName);
        Assert.True(derived.IsKind("OcctSharp_TransientDerived"));
        Assert.True(derived.IsKind("Standard_Transient"));
        Assert.False(derived.IsKind("OcctSharp_UnknownTransient"));
    }

    [Fact]
    public void SharedTransientCheckedCastRetainsDerivedObject()
    {
        using SharedTransient source = SharedTransient.CreateDerived();
        Assert.True(source.TryCastDerived(out SharedTransientDerived? typed));
        Assert.NotNull(typed);
        using (typed)
        {
            Assert.Equal("OcctSharp_TransientDerived", typed.TypeName);
            Assert.True(typed.IsKind("Standard_Transient"));
            Assert.Equal(2, source.ReferenceCount);
        }

        Assert.Equal(1, source.ReferenceCount);
    }

    [Fact]
    public void SharedTransientCheckedCastRejectsWrongAndNullKinds()
    {
        using SharedTransient baseOnly = SharedTransient.Create();
        Assert.False(baseOnly.TryCastDerived(out SharedTransientDerived? wrong));
        Assert.Null(wrong);
        Assert.Throws<InvalidCastException>(() => baseOnly.CastDerived());

        using SharedTransient nullHandle = SharedTransient.CreateNull();
        Assert.False(nullHandle.TryCastDerived(out SharedTransientDerived? nullResult));
        Assert.Null(nullResult);
    }

    [Fact]
    public void GeneratedGeomCartesianPointPreservesSharedLifetimeAndValues()
    {
        using GeomCartesianPoint source = new(1, 2, 3);

        Assert.Equal(1, source.X());
        Assert.Equal(2, source.Y());
        Assert.Equal(3, source.Z());
        Assert.Equal(new Point3d(1, 2, 3), source.Pnt());
        Assert.Equal("Geom_CartesianPoint", source.TypeName);
        Assert.True(source.IsKind("Geom_Point"));
        Assert.True(source.IsKind("Standard_Transient"));
        Assert.Equal(1, source.ReferenceCount);

        using GeomCartesianPoint clone = source.Clone();
        Assert.Equal(2, source.ReferenceCount);
        clone.SetCoord(4, 5, 6);
        Assert.Equal(new Point3d(4, 5, 6), source.Pnt());

        source.Dispose();
        Assert.Equal(1, clone.ReferenceCount);
        clone.SetPnt(new Point3d(7, 8, 9));
        Assert.Equal(new Point3d(7, 8, 9), clone.Pnt());
        Assert.Throws<ObjectDisposedException>(() => source.X());
    }

    [Fact]
    public void GeneratedGeomCartesianPointSupportsPointValueConstructor()
    {
        using GeomCartesianPoint point = new(new Point3d(10, 20, 30));

        point.SetX(11);
        point.SetY(21);
        point.SetZ(31);

        Assert.Equal(new Point3d(11, 21, 31), point.Pnt());
    }

    [Fact]
    public void GeneratedTopologyPreservesTopoDsCopyAndOrientationSemantics()
    {
        using Shape source = ShapeFactory.CreateBox(10, 20, 30);

        Assert.False(source.IsNull);
        Assert.Equal(ShapeKind.Solid, source.Kind);
        Assert.Equal(ShapeOrientation.Forward, source.Orientation);

        using Shape clone = source.Clone();
        Assert.True(source.IsPartner(clone));
        Assert.True(source.IsSame(clone));
        Assert.True(source.IsEqual(clone));

        using Shape reversed = source.Reversed();
        Assert.True(source.IsPartner(reversed));
        Assert.True(source.IsSame(reversed));
        Assert.False(source.IsEqual(reversed));
        Assert.Equal(ShapeOrientation.Reversed, reversed.Orientation);
        Assert.Equal(6, reversed.FaceCount);

        source.Dispose();
        Assert.Equal(ShapeKind.Solid, clone.Kind);
        Assert.Equal(6, clone.FaceCount);
        Assert.Throws<ObjectDisposedException>(() => _ = source.Kind);
    }

    [Fact]
    public void GeneratedTopologyCheckedCastsPreserveTypedKindsAndLifetime()
    {
        using Shape source = ShapeFactory.CreateBox(10, 20, 30);
        using Solid solid = source.CastSolid();

        Assert.Equal(ShapeKind.Solid, solid.Kind);
        Assert.Equal(6, solid.FaceCount);
        Assert.True(source.IsPartner(solid));
        Assert.True(source.IsSame(solid));
        Assert.True(source.IsEqual(solid));

        Assert.True(source.TryCastSolid(out Solid? secondSolid));
        Assert.NotNull(secondSolid);
        using (secondSolid)
        {
            Assert.Equal(ShapeKind.Solid, secondSolid.Kind);
        }

        Assert.False(source.TryCastFace(out Face? wrongFace));
        Assert.Null(wrongFace);
        Assert.Throws<InvalidCastException>(() => source.CastFace());

        source.Dispose();
        Assert.Equal(6, solid.FaceCount);
        Assert.Equal(ShapeKind.Solid, solid.Kind);
    }

    [Fact]
    public void GeneratedTopologyCompoundCastIsChecked()
    {
        using Shape first = ShapeFactory.CreateBox(1, 2, 3);
        using Shape second = ShapeFactory.CreateBox(4, 5, 6);
        using Shape compoundSource = ShapeAssembly.Create(
        [
            new ShapePlacement(first, ShapeTransform.Identity),
            new ShapePlacement(second, ShapeTransform.CreateTranslationAndRotationZ(10, 0, 0, 15)),
        ]);

        using Compound compound = compoundSource.CastCompound();
        Assert.Equal(ShapeKind.Compound, compound.Kind);
        Assert.Equal(12, compound.FaceCount);
        Assert.False(compoundSource.TryCastSolid(out Solid? wrongSolid));
        Assert.Null(wrongSolid);
    }

    [Fact]
    public void BoxRoundTripsThroughStep()
    {
        WithTemporaryDirectory(directory =>
        {
            string path = Path.Combine(directory, "box.step");
            using Shape source = ShapeFactory.CreateBox(10, 20, 30);
            ShapeExchange.WriteStep(source, path);

            using Shape roundTripped = ShapeExchange.ReadStep(path);
            Assert.Equal(6, roundTripped.FaceCount);
            Assert.True(new FileInfo(path).Length > 0);
        });
    }

    [Fact]
    public void BoxWritesStlAndIges()
    {
        WithTemporaryDirectory(directory =>
        {
            using Shape box = ShapeFactory.CreateBox(10, 20, 30);
            string stl = ShapeExchange.WriteStl(box, Path.Combine(directory, "box.stl"));
            string iges = ShapeExchange.WriteIges(box, Path.Combine(directory, "box.iges"));

            Assert.True(new FileInfo(stl).Length > 84);
            Assert.True(new FileInfo(iges).Length > 0);
        });
    }

    [Fact]
    public void TransformedCompoundRoundTripsThroughStep()
    {
        WithTemporaryDirectory(directory =>
        {
            using Shape first = ShapeFactory.CreateBox(10, 20, 30);
            using Shape second = ShapeFactory.CreateBox(5, 6, 7);
            using Shape compound = ShapeAssembly.Create(
            [
                new ShapePlacement(first, ShapeTransform.Identity),
                new ShapePlacement(
                    second,
                    ShapeTransform.CreateTranslationAndRotationZ(50, 25, 5, 30)),
            ]);

            Assert.Equal(12, compound.FaceCount);
            string path = ShapeExchange.WriteStep(compound, Path.Combine(directory, "assembly.step"));
            using Shape roundTripped = ShapeExchange.ReadStep(path);
            Assert.Equal(12, roundTripped.FaceCount);
        });
    }

    [Fact]
    public void XdeAssemblyWritesOneAssemblyWithTransformedComponents()
    {
        WithTemporaryDirectory(directory =>
        {
            string firstPath = Path.Combine(directory, "first.step");
            string secondPath = Path.Combine(directory, "second.step");
            string assemblyPath = Path.Combine(directory, "assembly.step");
            using (Shape first = ShapeFactory.CreateBox(10, 20, 30))
            using (Shape second = ShapeFactory.CreateBox(5, 6, 7))
            {
                ShapeExchange.WriteStep(first, firstPath);
                ShapeExchange.WriteStep(second, secondPath);
            }

            StepAssembly.WriteXde(
            [
                new StepAssemblyInput(firstPath, ShapeTransform.Identity),
                new StepAssemblyInput(
                    secondPath,
                    ShapeTransform.CreateTranslationAndRotationZ(50, 25, 5, 30)),
            ],
            assemblyPath);

            string stepText = File.ReadAllText(assemblyPath);
            Assert.Contains("NEXT_ASSEMBLY_USAGE_OCCURRENCE", stepText, StringComparison.OrdinalIgnoreCase);
            using Shape roundTripped = ShapeExchange.ReadStep(assemblyPath);
            Assert.Equal(12, roundTripped.FaceCount);
        });
    }

    [Fact]
    public void StepAssemblyInputAbiLayoutIsStable()
    {
        Assert.Equal(64, Marshal.SizeOf<NativeStepAssemblyInput>());
        Assert.Equal(0, Marshal.OffsetOf<NativeStepAssemblyInput>(nameof(NativeStepAssemblyInput.FilePath)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<NativeStepAssemblyInput>(nameof(NativeStepAssemblyInput.TranslationX)).ToInt32());
        Assert.Equal(
            56,
            Marshal.OffsetOf<NativeStepAssemblyInput>(nameof(NativeStepAssemblyInput.RotationAngleRadians)).ToInt32());
    }

    [Fact]
    public void GeneratedPointConstructorCopiesCoordinatesAcrossAbi()
    {
        Point3dRaw point = GeneratedNativeMethods.CreatePoint3d(1.25, -2.5, 9.75);

        Assert.Equal(24, Marshal.SizeOf<Point3dRaw>());
        Assert.Equal(1.25, point.X);
        Assert.Equal(-2.5, point.Y);
        Assert.Equal(9.75, point.Z);
    }

    [Fact]
    public void GeneratedPointDefaultAndCopyConstructorsPreserveValueCopySemantics()
    {
        Point3dRaw defaultPoint = GeneratedNativeMethods.CreatePoint3dDefault();
        Point3dRaw copiedPoint = GeneratedNativeMethods.CreatePoint3dCopy(
            new Point3dRaw(4.5, -6.25, 8.75));

        Assert.Equal(0.0, defaultPoint.X);
        Assert.Equal(0.0, defaultPoint.Y);
        Assert.Equal(0.0, defaultPoint.Z);
        Assert.Equal(4.5, copiedPoint.X);
        Assert.Equal(-6.25, copiedPoint.Y);
        Assert.Equal(8.75, copiedPoint.Z);
    }

    [Fact]
    public void GeneratedPrecisionStaticsExecuteThroughTheValueCopyAbi()
    {
        double angular = GeneratedNativeMethods.PrecisionAngular0();
        double confusion = GeneratedNativeMethods.PrecisionConfusion0();
        double parameterizedApproximation = GeneratedNativeMethods.PrecisionPApproximation1(100.0);

        Assert.True(double.IsFinite(angular) && angular > 0.0);
        Assert.True(double.IsFinite(confusion) && confusion > 0.0);
        Assert.True(double.IsFinite(parameterizedApproximation) && parameterizedApproximation > 0.0);
        Assert.Equal(1, GeneratedNativeMethods.PrecisionIsInfinite0(double.PositiveInfinity));
        Assert.Equal(0, GeneratedNativeMethods.PrecisionIsInfinite0(1.0));
    }

    [Fact]
    public void GeneratedTopAbsEnumStaticsExecuteThroughInt32Abi()
    {
        int composed = GeneratedNativeMethods.TopAbsCompose0(0, 0);
        int reversed = GeneratedNativeMethods.TopAbsReverse0(0);

        Assert.InRange(composed, 0, 3);
        Assert.InRange(reversed, 0, 3);
    }

    [Fact]
    public void GeneratedAdditionalScalarStaticsExecuteThroughValueCopyAbi()
    {
        double resolution = GeneratedNativeMethods.GpResolution0();
        double scalePrecision = GeneratedNativeMethods.TopLocLocationScalePrec0();
        int allocatorType = GeneratedNativeMethods.StandardGetAllocatorType0();
        int stackTraceLength = GeneratedNativeMethods.StandardFailureDefaultStackTraceLength0();
        int jsonKeyLength = GeneratedNativeMethods.StandardDumpJsonKeyLength0(0);

        Assert.True(double.IsFinite(resolution) && resolution > 0.0);
        Assert.True(double.IsFinite(scalePrecision) && scalePrecision > 0.0);
        Assert.InRange(allocatorType, 0, 8);
        Assert.True(stackTraceLength >= 0);
        Assert.True(jsonKeyLength >= 0);
    }

    [Fact]
    public void GpTrsfIdentityAndCompositionPreserveMatrixValues()
    {
        using GpTrsf identity = GpTrsf.Identity;
        Assert.Equal(1.0, identity.Value(1, 1));
        Assert.Equal(1.0, identity.Value(2, 2));
        Assert.Equal(1.0, identity.Value(3, 3));
        Assert.Equal(0.0, identity.Value(1, 4));

        using GpTrsf translation = GpTrsf.Create(10, 20, 30);
        using GpTrsf composed = identity.Multiplied(translation);
        Assert.Equal(10.0, composed.Value(1, 4));
        Assert.Equal(20.0, composed.Value(2, 4));
        Assert.Equal(30.0, composed.Value(3, 4));
    }

    [Fact]
    public void GpTrsfCloneAndInverseAreIndependentValues()
    {
        using GpTrsf source = GpTrsf.Create(10, 0, 0, 0, 0, 1, Math.PI / 2);
        using GpTrsf clone = source.Clone();
        using GpTrsf inverse = source.Inverted();

        Assert.Equal(source.Value(1, 4), clone.Value(1, 4));
        using Shape shape = ShapeFactory.CreateBox(1, 1, 1);
        using Shape transformed = source.Apply(shape);
        Assert.Equal(6, transformed.FaceCount);
        using GpTrsf roundTrip = inverse.Multiplied(source);
        Assert.Equal(1.0, roundTrip.Value(1, 1), 8);
    }

    [Fact]
    public void GpTrsfRejectsNonFiniteValuesAndInvalidMatrixIndices()
    {
        Assert.Throws<ArgumentException>(() => GpTrsf.Create(double.NaN, 0, 0));
        using GpTrsf identity = GpTrsf.Identity;
        Assert.Throws<ArgumentException>(() => identity.Value(0, 1));
        Assert.Throws<ArgumentException>(() => identity.Value(1, 5));
    }

    [Fact]
    public void TopLocLocationPreservesIdentityCompositionAndTransformValues()
    {
        using TopLocLocation identity = TopLocLocation.Identity;
        Assert.True(identity.IsIdentity);

        using GpTrsf translation = GpTrsf.Create(10, 20, 30);
        using TopLocLocation location = TopLocLocation.FromTransform(translation);
        Assert.False(location.IsIdentity);

        using TopLocLocation clone = location.Clone();
        using TopLocLocation inverse = location.Inverted();
        using TopLocLocation roundTrip = inverse.Multiplied(location);
        using GpTrsf roundTripTransform = roundTrip.ToTransform();
        using GpTrsf cloneTransform = clone.ToTransform();
        Assert.True(roundTrip.IsIdentity);
        Assert.Equal(0.0, roundTripTransform.Value(1, 4), 12);
        Assert.Equal(10.0, cloneTransform.Value(1, 4), 12);
    }

    [Fact]
    public void TopLocLocationCanLocateAndMoveShapes()
    {
        using Shape source = ShapeFactory.CreateBox(1, 1, 1);
        using GpTrsf translation = GpTrsf.Create(5, 0, 0);
        using TopLocLocation location = TopLocLocation.FromTransform(translation);
        using Shape located = source.Located(location);
        using Shape moved = source.Moved(location);

        Assert.Equal(6, located.FaceCount);
        Assert.Equal(6, moved.FaceCount);
        Assert.False(located.IsNull);
        Assert.False(moved.IsNull);
    }

    [Fact]
    public void VectorDirectionAxisAndMatrixValuesRoundTrip()
    {
        using GpVec vector = GpVec.Create(3, 4, 0);
        Assert.Equal(5.0, vector.Magnitude, 12);
        Assert.Equal(25.0, vector.Dot(vector), 12);
        using GpVec zAxis = GpVec.Create(0, 0, 1);
        using GpVec cross = vector.Crossed(zAxis);
        Assert.Equal((4.0, -3.0, 0.0), cross.Components);
        using GpTrsf translation = vector.ToTranslation();
        Assert.Equal(3.0, translation.Value(1, 4), 12);

        using GpDir direction = GpDir.Create(0, 0, 1);
        using GpDir reversed = direction.Reversed();
        Assert.Equal((0.0, 0.0, -1.0), reversed.Components);

        using GpAx1 axis = GpAx1.Create(1, 2, 3, 0, 0, 1);
        using GpAx1 reverseAxis = axis.Reversed();
        Assert.Equal((1.0, 2.0, 3.0, 0.0, 0.0, -1.0), reverseAxis.Components);
        using GpTrsf rotation = axis.ToRotation(Math.PI / 2);
        Assert.Equal(0.0, rotation.Value(1, 1), 12);
        Assert.Equal(-1.0, rotation.Value(1, 2), 12);

        using GpMat identity = GpMat.Identity;
        Assert.Equal(1.0, identity.Determinant, 12);
        Assert.Equal(1.0, identity.Value(2, 2), 12);
        using GpMat matrix = GpMat.Create([1, 2, 3, 0, 1, 4, 5, 6, 0]);
        Assert.Equal(1.0, matrix.Determinant, 12);
    }

    [Fact]
    public void DirectionAndMatrixValidationRejectInvalidValues()
    {
        Assert.Throws<OcctException>(() => GpDir.Create(0, 0, 0));
        Assert.Throws<ArgumentException>(() => GpMat.Create([1, 2, 3]));
        using GpMat identity = GpMat.Identity;
        Assert.Throws<ArgumentException>(() => identity.Value(0, 1));
    }

    [Fact]
    public void Utf8AsciiAndExtendedStringsRoundTripWithIndependentOwnership()
    {
        using OcctAsciiString ascii = OcctAsciiString.Create("Hello 世界");
        Assert.Equal("Hello 世界", ascii.Value);
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount("Hello 世界"), ascii.Length);

        ascii.Append("!");
        using OcctAsciiString asciiClone = ascii.Clone();
        Assert.Equal("Hello 世界!", asciiClone.Value);

        using OcctExtendedString extended = ascii.ToExtended();
        Assert.Equal("Hello 世界!", extended.Value);
        Assert.Equal('H', extended[0]);
        using OcctAsciiString roundTrip = extended.ToAscii();
        Assert.Equal(ascii.Value, roundTrip.Value);
    }

    [Fact]
    public void RealSequenceSupportsIndexedMutationAndIndependentClone()
    {
        using OcctRealSequence sequence = OcctRealSequence.Create([1, 2, 3]);
        Assert.Equal(3, sequence.Count);
        Assert.Equal(2.0, sequence[1], 12);

        sequence.Set(1, 20);
        sequence.Add(4);
        using OcctRealSequence clone = sequence.Clone();
        sequence.RemoveAt(0);

        Assert.Equal(3, sequence.Count);
        Assert.Equal(20.0, sequence[0], 12);
        Assert.Equal(4, clone.Count);
        Assert.Equal(1.0, clone[0], 12);
        Assert.Throws<ArgumentException>(() => sequence[99]);
        Assert.Throws<ArgumentException>(() => sequence.Add(double.NaN));
    }

    [Fact]
    public void RealArrayPreservesNativeBoundsAndCloneOwnership()
    {
        using OcctRealArray array = OcctRealArray.Create([1, 2, 3]);
        Assert.Equal(3, array.Count);
        Assert.Equal(1, array.LowerBound);
        Assert.Equal(2.0, array[1], 12);
        array.Set(1, 20);
        using OcctRealArray clone = array.Clone();
        array.Set(0, 10);
        Assert.Equal(10.0, array[0], 12);
        Assert.Equal(20.0, clone[1], 12);
        Assert.Equal(1.0, clone[0], 12);
        Assert.Throws<ArgumentException>(() => array[99]);
        Assert.Throws<ArgumentException>(() => array.Set(0, double.PositiveInfinity));
    }

    [Fact]
    public void RealVectorSupportsAppendMutationAndEnumeration()
    {
        using OcctRealVector vector = OcctRealVector.Create([1, 2]);
        vector.Add(3);
        vector.Set(1, 20);
        using OcctRealVector clone = vector.Clone();
        vector.Set(0, 10);
        Assert.Equal([10.0, 20.0, 3.0], vector.ToArray());
        Assert.Equal([1.0, 20.0, 3.0], clone.ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() => vector[-1]);
        Assert.Throws<ArgumentException>(() => vector.Set(99, 1));
        Assert.Throws<ArgumentException>(() => vector.Add(double.NaN));
    }

    [Fact]
    public void IntegerRealMapSupportsLookupMutationRemovalAndClone()
    {
        using OcctIntRealMap map = OcctIntRealMap.Create([new(7, 1.5), new(9, 2.5)]);
        Assert.Equal(2, map.Count);
        Assert.True(map.ContainsKey(7));
        Assert.Equal(1.5, map[7], 12);
        map[7] = 10;
        map[11] = 3;
        using OcctIntRealMap clone = map.Clone();
        Assert.Equal(10.0, clone[7], 12);
        Assert.True(map.Remove(9));
        Assert.False(map.Remove(99));
        Assert.Equal(3, clone.Count);
        Assert.Throws<ArgumentException>(() => map[99]);
        Assert.Throws<ArgumentException>(() => map[12] = double.NaN);
        Assert.Throws<ArgumentException>(() => OcctIntRealMap.Create([new(1, 1), new(1, 2)]));
    }

    [Fact]
    public void IntegerIndexedMapPreservesOrderAndCloneIndependence()
    {
        using OcctIntIndexedMap map = OcctIntIndexedMap.Create([4, 8]);
        Assert.Equal([4, 8], map.ToArray());
        Assert.Equal(1, map.FindIndex(8));
        Assert.Equal(-1, map.FindIndex(99));
        Assert.True(map.Add(12));
        Assert.False(map.Add(12));
        using OcctIntIndexedMap clone = map.Clone();
        Assert.Equal(12, map.RemoveLast());
        Assert.Equal([4, 8], map.ToArray());
        Assert.Equal([4, 8, 12], clone.ToArray());
        Assert.Throws<ArgumentException>(() => map[99]);
        Assert.Throws<ArgumentException>(() => OcctIntIndexedMap.Create([1, 1]));
    }

    private static void WithTemporaryDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"occtsharp-runtime-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            action(directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
