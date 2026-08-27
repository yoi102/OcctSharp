namespace OcctSharp.Runtime.Tests;

public sealed class GeneratedIgesModelHandleTests
{
    [Fact]
    public void GeneratedIgesFamiliesConstructCloneAndRetainAllPublicTypes()
    {
        (string Prefix, int ExpectedCount, int ExpectedDefaultConstructibleCount)[] families =
        [
            ("IGESAppli", 23, 23),
            ("IGESBasic", 25, 20),
            ("IGESDefs", 12, 11),
            ("IGESDimen", 27, 27),
            ("IGESDraw", 18, 18),
            ("IGESGeom", 27, 27),
            ("IGESGraph", 18, 18),
            ("IGESSolid", 28, 28),
        ];
        Type[] exportedTypes = typeof(IGESGeomDirection).Assembly.GetExportedTypes();

        foreach ((string prefix, int expectedCount, int expectedDefaultConstructibleCount) in families)
        {
            Type[] familyTypes = exportedTypes
                .Where(type => type.IsClass
                    && type.Name.StartsWith(prefix, StringComparison.Ordinal)
                    && typeof(IDisposable).IsAssignableFrom(type))
                .OrderBy(static type => type.FullName, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expectedCount, familyTypes.Length);
            Type[] defaultConstructibleTypes = familyTypes
                .Where(static type => type.GetConstructor(Type.EmptyTypes) is not null)
                .ToArray();
            Assert.Equal(expectedDefaultConstructibleCount, defaultConstructibleTypes.Length);

            foreach (Type type in defaultConstructibleTypes)
            {
                using IDisposable instance = Assert.IsAssignableFrom<IDisposable>(Activator.CreateInstance(type));
                using IDisposable clone = Assert.IsAssignableFrom<IDisposable>(
                    type.GetMethod("Clone")!.Invoke(instance, null));
                Assert.Equal(2, type.GetProperty("ReferenceCount")!.GetValue(instance));
                Assert.StartsWith("IGES", Assert.IsType<string>(type.GetProperty("TypeName")!.GetValue(instance)));
            }
        }
    }

    [Fact]
    public void GeneratedIgesDimensionValueMembersRoundTrip()
    {
        using IGESDimenDimensionTolerance tolerance = new();
        tolerance.Init(8, 1, 2, 3, 0.25, -0.125, true, 4, 5);

        Assert.Equal(8, tolerance.NbPropertyValues());
        Assert.Equal(1, tolerance.SecondaryToleranceFlag());
        Assert.Equal(2, tolerance.ToleranceType());
        Assert.Equal(3, tolerance.TolerancePlacementFlag());
        Assert.Equal(0.25, tolerance.UpperTolerance(), 12);
        Assert.Equal(-0.125, tolerance.LowerTolerance(), 12);
        Assert.True(tolerance.SignSuppressionFlag());
        Assert.Equal(4, tolerance.FractionFlag());
        Assert.Equal(5, tolerance.Precision());
    }
}
