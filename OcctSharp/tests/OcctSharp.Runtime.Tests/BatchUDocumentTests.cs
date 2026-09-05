using static OcctSharp.Runtime.Tests.BatchTRecomputeTests;
using static OcctSharp.Runtime.Tests.BatchUFinishingTests;

namespace OcctSharp.Runtime.Tests;

#pragma warning disable CA1861
public sealed class BatchUDocumentTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChangedOccurrenceOrDefinitionCannotPublishAStaleCandidate(bool changeDefinition)
    {
        using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(10, 12, 15);
        var (definition, assembly, occurrence) = Fixture(document, box);
        using var session = new LocalFeatureDocumentSession(document, assembly, [occurrence.Entry]);
        using var result = ContourFilletRecipe.Create(session.Source, [FilletContourProgram.Constant(Edge(session.Source), 1)]).Build(session.Source);
        using (var command = document.BeginTransaction("External edit"))
        {
            if (changeDefinition) { using var replacement = ShapeFactory.CreateBox(20, 12, 15); document.UpdateDefinitionShape(definition, replacement); }
            else
            {
                using var transform = ShapeTransform.CreateTranslation(25, 0, 0).ToGpTrsf(); using var location = TopLocLocation.FromTransform(transform);
                document.RelocateOccurrence(occurrence, location);
            }
            command.Commit();
        }
        Assert.Throws<InvalidOperationException>(() => session.Publish(result));
        using var current = definition.Shape; Assert.Equal(changeDefinition ? 3600 : 1800, Mass(current), 5);
    }

    [Fact]
    public void AmbiguousMetadataForeignSourcesAndDisposedParentsRejectPublication()
    {
        using var document = XdeDocument.Create(); using var box = ShapeFactory.CreateBox(10, 12, 15);
        var (definition, assembly, occurrence) = Fixture(document, box);
        using var session = new LocalFeatureDocumentSession(document, assembly, [occurrence.Entry]);
        var edge = Edge(session.Source);
        using (var command = document.BeginTransaction("Metadata on changed edge"))
        { var label = session.GetOrCreateSubshapeLabel(edge); label.Name = "ambiguous edge"; command.Commit(); }
        using var result = ContourFilletRecipe.Create(session.Source, [FilletContourProgram.Constant(edge, 1)]).Build(session.Source);
        var review = session.Review(result); Assert.False(review.CanPublish);
        Assert.Throws<InvalidOperationException>(() => session.Publish(result));
        using var current = definition.Shape; Assert.Equal(1800, Mass(current), 5);
        using var other = RepairSnapshot.Create(box);
        using var foreign = ContourFilletRecipe.Create(other, [FilletContourProgram.Constant(Edge(other), 1)]).Build(other);
        Assert.Throws<ArgumentException>(() => session.Review(foreign));
        document.Dispose(); Assert.Throws<ObjectDisposedException>(() => session.Review(result));
        Assert.True(result.RequireShape().IsValid);
        session.Dispose(); Assert.Throws<ObjectDisposedException>(() => session.Review(result));
    }

    private static (XdeLabel Definition, XdeLabel Assembly, XdeLabel Occurrence) Fixture(XdeDocument document, Shape box)
    {
        using var command = document.BeginTransaction("Local edit fixture"); var definition = document.AddShape(box, "part");
        var assembly = document.AddAssembly("assembly"); using var location = TopLocLocation.Identity;
        var occurrence = document.AddComponent(assembly, definition, location); command.Commit(); return (definition, assembly, occurrence);
    }
}
