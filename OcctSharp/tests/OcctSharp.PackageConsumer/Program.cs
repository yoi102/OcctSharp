using OcctSharp;

string nativeDirectory = Path.Combine(AppContext.BaseDirectory, "occt");
string nativeBridge = Path.Combine(nativeDirectory, "OcctSharp.Native.dll");
string misplacedNativeBridge = Path.Combine(AppContext.BaseDirectory, "OcctSharp.Native.dll");

if (!Directory.Exists(nativeDirectory) || !File.Exists(nativeBridge))
{
    throw new InvalidOperationException(
        $"The packaged native runtime was not copied to '{nativeDirectory}'.");
}

if (File.Exists(misplacedNativeBridge))
{
    throw new InvalidOperationException(
        $"The native bridge must not be copied beside the application at '{misplacedNativeBridge}'.");
}

string[] nativeFiles = Directory.GetFiles(nativeDirectory, "*.dll");
if (nativeFiles.Length < 2)
{
    throw new InvalidOperationException("The complete OCCT native dependency closure was not copied.");
}

OcctRuntimeInfo runtime = OcctRuntime.Info;
if (runtime.AbiVersion != new Version(1, 17)
    || runtime.BridgeVersion != "0.18.0"
    || runtime.OcctVersion != "8.0.1")
{
    throw new InvalidOperationException(
        $"Unexpected packaged runtime: ABI {runtime.AbiVersion}, OCCT {runtime.OcctVersion}.");
}

using Shape box = ShapeFactory.CreateBox(10, 20, 30);
if (box.FaceCount != 6)
{
    throw new InvalidOperationException($"Expected 6 box faces, received {box.FaceCount}.");
}

if (box.IsNull || box.Kind != ShapeKind.Solid || box.Orientation != ShapeOrientation.Forward)
{
    throw new InvalidOperationException("The packaged generated topology binding returned unexpected box semantics.");
}

using Shape clone = box.Clone();
using Shape reversed = box.Reversed();
if (!box.IsPartner(clone)
    || !box.IsSame(clone)
    || !box.IsEqual(clone)
    || !box.IsPartner(reversed)
    || !box.IsSame(reversed)
    || box.IsEqual(reversed)
    || reversed.Orientation != ShapeOrientation.Reversed)
{
    throw new InvalidOperationException("The packaged TopoDS_Shape copy/orientation semantics are invalid.");
}

using GeomCartesianPoint point = new(1, 2, 3);
point.SetPnt(new Point3d(4, 5, 6));
if (point.X() != 4 || point.Y() != 5 || point.Z() != 6
    || point.TypeName != "Geom_CartesianPoint")
{
    throw new InvalidOperationException(
        "The packaged generated shared-handle binding returned unexpected point data.");
}

using GpTrsf translation = GpTrsf.Create(10, 20, 30);
using GpTrsf inverse = translation.Inverted();
if (translation.Value(1, 4) != 10 || translation.Value(2, 4) != 20
    || translation.Value(3, 4) != 30
    || Math.Abs(inverse.Value(1, 4) + 10) > 1e-12)
{
    throw new InvalidOperationException("The packaged GpTrsf value bridge returned unexpected matrix data.");
}

using TopLocLocation location = TopLocLocation.FromTransform(translation);
using TopLocLocation inverseLocation = location.Inverted();
using TopLocLocation identityLocation = inverseLocation.Multiplied(location);
if (location.IsIdentity || !identityLocation.IsIdentity)
{
    throw new InvalidOperationException("The packaged TopLoc_Location bridge returned unexpected identity semantics.");
}

using GpVec vector = GpVec.Create(3, 4, 0);
using GpDir direction = GpDir.Create(0, 0, 1);
using GpAx1 axis = GpAx1.Create(0, 0, 0, 0, 0, 1);
using GpMat matrix = GpMat.Identity;
using GpTrsf vectorTranslation = vector.ToTranslation();
using GpTrsf axisRotation = axis.ToRotation(Math.PI / 2);
if (vector.Magnitude != 5 || direction.Dot(direction) != 1 || axis.Components.DirectionZ != 1 || matrix.Determinant != 1
    || Math.Abs(vectorTranslation.Value(1, 4) - 3) > 1e-12 || Math.Abs(axisRotation.Value(1, 1)) > 1e-12)
{
    throw new InvalidOperationException("The packaged gp value bridge returned unexpected vector/axis/matrix data.");
}

using OcctAsciiString ascii = OcctAsciiString.Create("包");
ascii.Append(" ok");
using OcctExtendedString extended = ascii.ToExtended();
if (extended.Value != "包 ok" || extended[0] != '包')
{
    throw new InvalidOperationException("The packaged UTF-8 string bridge returned unexpected text.");
}

using OcctRealSequence sequence = OcctRealSequence.Create([1, 2, 3]);
sequence.Set(1, 20);
sequence.Add(4);
if (sequence.Count != 4 || sequence[1] != 20)
{
    throw new InvalidOperationException("The packaged real sequence bridge returned unexpected values.");
}

using OcctRealArray array = OcctRealArray.Create([1, 2, 3]);
array.Set(1, 20);
using OcctRealVector vectorValues = OcctRealVector.Create([4, 5]);
vectorValues.Add(6);
if (array.LowerBound != 1 || array[1] != 20 || vectorValues.Count != 3 || vectorValues[2] != 6)
{
    throw new InvalidOperationException("The packaged array/vector bridge returned unexpected values.");
}

using OcctIntRealMap map = OcctIntRealMap.Create([new(1, 2.0)]);
map[2] = 3.0;
using OcctIntIndexedMap indexedMap = OcctIntIndexedMap.Create([5, 6]);
indexedMap.Add(7);
if (map[2] != 3.0 || indexedMap[2] != 7 || indexedMap.FindIndex(5) != 0)
{
    throw new InvalidOperationException("The packaged map bridge returned unexpected values.");
}

Console.WriteLine(
    $"Package consumer passed with {nativeFiles.Length} DLLs in 'occt', "
    + $"ABI {runtime.AbiVersion}, bridge {runtime.BridgeVersion}, OCCT {runtime.OcctVersion}.");
