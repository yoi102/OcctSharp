namespace OcctSharp.Runtime.Tests;

public sealed class BatchICompletionTests
{
    [Fact]
    public void LabelAttributesTraversalAndOwningSnapshotsRemainSafe()
    {
        OcafDocument document = OcafDocument.Create();
        OcafLabel root = document.RootLabel;
        OcafLabel child;
        OcafLabel grandchild;
        using (OcafTransaction command = document.BeginTransaction("seed typed state"))
        {
            child = root.AddChild();
            grandchild = child.AddChild();
            child.Name = "部件 α";
            child.Comment = "履歴コメント";
            child.AsciiString = "batch-i";
            child.IntegerValue = int.MinValue;
            child.RealValue = 12.5;
            child.SetIntegerArray(-2, [3, 5, 8]);
            child.SetRealArray(4, [1.25, 2.5]);
            child.Reference = grandchild;
            child.SetReferences([grandchild, root, grandchild]);
            grandchild.ReparentTree(child);
            using Shape box = ShapeFactory.CreateBox(2, 3, 4);
            child.SetNamedShape(box);
            Assert.True(command.Commit());
        }

        Assert.Throws<ArgumentException>(() => child.IntegerValue = 10);
        using (DocumentLabelSnapshot identity = child.CreateSnapshot()) Assert.Equal(2, identity.Depth);
        Assert.Equal([child.Entry], root.GetChildren().Select(static label => label.Entry));
        Assert.Equal([child.Entry, grandchild.Entry], root.GetDescendants().Select(static label => label.Entry));
        Assert.Equal("部件 α", child.Name);
        Assert.Equal("履歴コメント", child.Comment);
        Assert.Equal("batch-i", child.AsciiString);
        Assert.Equal(int.MinValue, child.IntegerValue);
        Assert.Equal(12.5, child.RealValue);
        Assert.Equal((-2, 0), (child.IntegerArray!.LowerBound, child.IntegerArray.UpperBound));
        Assert.Equal([3, 5, 8], child.IntegerArray.Values);
        Assert.Equal((4, 5), (child.RealArray!.LowerBound, child.RealArray.UpperBound));
        Assert.Equal([1.25, 2.5], child.RealArray.Values);
        Assert.Equal(grandchild.Entry, child.Reference!.Entry);
        Assert.Equal([grandchild.Entry, root.Entry, grandchild.Entry], child.References.Select(static label => label.Entry));
        Assert.Equal(child.Entry, grandchild.Tree!.ParentEntry);

        using (OcafTransaction aborted = document.BeginTransaction("abort replacements"))
        {
            child.Comment = null;
            child.IntegerValue = 99;
            child.SetIntegerArray(0, []);
            child.Reference = null;
            grandchild.DetachTree();
            Assert.Throws<ArgumentOutOfRangeException>(() => child.SetRealArray(0, [double.NaN]));
            aborted.Abort();
        }
        Assert.Equal("履歴コメント", child.Comment);
        Assert.Equal(int.MinValue, child.IntegerValue);
        Assert.Equal([3, 5, 8], child.IntegerArray!.Values);
        Assert.Equal(grandchild.Entry, child.Reference!.Entry);
        Assert.Equal(child.Entry, grandchild.Tree!.ParentEntry);

        DocumentSnapshot snapshot = document.CreateSnapshot();
        DocumentLabelSnapshot childSnapshot = snapshot.GetLabel(child.Entry);
        Assert.Contains(snapshot.Labels, static label => label.IsRoot && label.Entry == "0");
        Assert.Contains(childSnapshot.Attributes, static attribute =>
            attribute.Kind == DocumentAttributeKind.Name && attribute.TextValue == "部件 α");
        Shape copiedTopology = Assert.Single(childSnapshot.Attributes,
            static attribute => attribute.Kind == DocumentAttributeKind.NamedShape).NamedShape!;

        document.Dispose();
        Assert.Equal(ShapeKind.Solid, copiedTopology.Kind);
        Assert.Throws<ObjectDisposedException>(() => _ = child.Name);
        snapshot.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _ = copiedTopology.Kind);
    }

    [Fact]
    public void DependencyGraphNamedHistoryUndoRedoBranchingAndSavepointAreCoherent()
    {
        using OcafDocument document = OcafDocument.Create();
        document.UndoLimit = -1;
        OcafLabel root = document.RootLabel;
        OcafLabel first;
        OcafLabel second;
        OcafLabel third;
        using (OcafTransaction command = document.BeginTransaction("create graph"))
        {
            first = root.AddChild();
            second = root.AddChild();
            third = root.AddChild();
            first.Name = "first";
            first.Reference = second;
            first.SetReferences([second, third, second]);
            second.ReparentTree(first);
            third.Reference = first;
            Assert.True(command.Commit());
        }

        DocumentDependencyGraph cyclic = document.CreateDependencyGraph();
        Assert.False(cyclic.IsAcyclic);
        Assert.Contains(cyclic.CyclicGroups, group => group.Contains(first.Entry) && group.Contains(third.Entry));
        Assert.Contains(cyclic.GetIncoming(second.Entry), edge => edge.SourceEntry == first.Entry);
        Assert.Contains(cyclic.GetOutgoing(first.Entry), edge => edge.Kind == DocumentDependencyEdgeKind.ReferenceArray);
        Assert.Empty(cyclic.TopologicalOrder);

        DocumentHistoryEntry created = Assert.Single(document.UndoHistory);
        Assert.Equal("create graph", created.Name);
        Assert.True(created.AttributeDeltaCount > 0);
        Assert.Contains(first.Entry, created.ChangedLabelEntries);
        document.MarkSaved();
        Assert.False(document.IsChanged);
        Assert.True(document.Undo());
        Assert.True(document.IsChanged);
        Assert.Equal(1, document.HistoryState.AvailableRedos);
        Assert.True(document.Redo());
        Assert.False(document.IsChanged);

        Assert.True(document.Undo());
        using (OcafTransaction command = document.BeginTransaction("branch history"))
        {
            root.Comment = "branch";
            Assert.True(command.Commit());
        }
        Assert.Empty(document.RedoHistory);
        Assert.False(document.Redo());
        Assert.Equal("branch history", document.UndoHistory[0].Name);

        document.UndoLimit = 2;
        Assert.InRange(document.HistoryState.AvailableUndos, 0, 2);
        document.ClearRedoHistory();
        document.ClearUndoHistory();
        Assert.Empty(document.UndoHistory);
        document.UndoLimit = 0;
        Assert.Equal(0, document.UndoLimit);
        document.UndoLimit = -1;
        Assert.Equal(-1, document.UndoLimit);
    }

    [Fact]
    public void GenericAndXdeDocumentsRoundTripThroughAllFourPersistenceFormats()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string binOcaf = Path.Combine(directory, "generic.cbf");
            string xmlOcaf = Path.Combine(directory, "generic.xml");
            using (OcafDocument source = OcafDocument.Create())
            {
                using OcafTransaction command = source.BeginTransaction("persist generic");
                OcafLabel label = source.RootLabel.AddChild();
                label.Name = "永続化";
                label.IntegerValue = 42;
                label.SetRealArray(7, [2.25, 4.5]);
                Assert.True(command.Commit());
                source.Save(binOcaf, DocumentStorageFormat.BinOcaf);
                source.Save(xmlOcaf, DocumentStorageFormat.XmlOcaf);
            }

            using OcafDocument binaryGeneric = OcafDocument.Open(binOcaf);
            using OcafDocument xmlGeneric = OcafDocument.Open(xmlOcaf);
            AssertEquivalentGeneric(binaryGeneric.CreateSnapshot(), xmlGeneric.CreateSnapshot());
            Assert.False(binaryGeneric.IsChanged);
            Assert.False(xmlGeneric.IsChanged);

            string binXcaf = Path.Combine(directory, "scene.xbf");
            string xmlXcaf = Path.Combine(directory, "scene.xml");
            using (XdeDocument source = XdeDocument.Create())
            using (Shape box = ShapeFactory.CreateBox(3, 4, 5))
            {
                using XdeTransaction command = source.BeginTransaction("persist xde");
                XdeLabel part = source.AddShape(box, "XDE 部件");
                XdeLabel assembly = source.AddAssembly("XDE Assembly");
                using TopLocLocation identity = TopLocLocation.Identity;
                XdeLabel occurrence = source.AddComponent(assembly, part, identity);
                part.Comment = "metadata";
                part.IntegerValue = 17;
                Assert.True(command.Commit());
                DocumentDependencyGraph graph = source.CreateDependencyGraph();
                Assert.Contains(graph.GetOutgoing(assembly.Entry), edge =>
                    edge.Kind == DocumentDependencyEdgeKind.XdeOccurrence && edge.TargetEntry == occurrence.Entry);
                Assert.Contains(graph.GetOutgoing(occurrence.Entry), edge =>
                    edge.Kind == DocumentDependencyEdgeKind.XdeOccurrence && edge.TargetEntry == part.Entry);
                source.Save(binXcaf, DocumentStorageFormat.BinXcaf);
                source.Save(xmlXcaf, DocumentStorageFormat.XmlXcaf);
            }

            using XdeDocument binaryXde = XdeDocument.Open(binXcaf);
            using XdeDocument xmlXde = XdeDocument.Open(xmlXcaf);
            XdeLabel binaryPart = Assert.Single(Assert.Single(binaryXde.GetFreeShapes()).GetComponents()).ReferredShape;
            XdeLabel xmlPart = Assert.Single(Assert.Single(xmlXde.GetFreeShapes()).GetComponents()).ReferredShape;
            Assert.Equal("XDE 部件", binaryPart.Name);
            Assert.Equal(binaryPart.Name, xmlPart.Name);
            Assert.Equal(17, binaryPart.IntegerValue);
            Assert.Equal(binaryPart.IntegerValue, xmlPart.IntegerValue);
            using Shape binaryShape = binaryPart.Shape;
            using Shape xmlShape = xmlPart.Shape;
            Assert.Equal(ShapeKind.Solid, binaryShape.Kind);
            Assert.Equal(binaryShape.Kind, xmlShape.Kind);
            Assert.False(binaryXde.IsChanged);
            Assert.False(xmlXde.IsChanged);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void StepXdeMutationGraphHistoryPersistenceAndExportSurviveSourceDisposal()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string inputStep = Path.Combine(directory, "input.step");
            string persisted = Path.Combine(directory, "mutated.xml");
            string outputStep = Path.Combine(directory, "output.step");
            using (Shape sourceShape = ShapeFactory.CreateBox(5, 6, 7))
                ShapeExchange.WriteStep(sourceShape, inputStep);

            using (XdeDocument document = XdeDocument.ReadStep(inputStep))
            {
                XdeLabel imported = Assert.Single(document.GetFreeShapes());
                using (XdeTransaction command = document.BeginTransaction("annotate imported step"))
                {
                    imported.Name = "Imported Assembly Root";
                    imported.Comment = "Batch I";
                    imported.Reference = imported;
                    Assert.True(command.Commit());
                }
                DocumentDependencyGraph graph = document.CreateDependencyGraph();
                Assert.Contains(graph.GetOutgoing(imported.Entry), edge =>
                    edge.Kind == DocumentDependencyEdgeKind.DirectReference && edge.TargetEntry == imported.Entry);
                Assert.False(graph.IsAcyclic);
                Assert.Equal("annotate imported step", Assert.Single(document.UndoHistory).Name);
                document.Save(persisted, DocumentStorageFormat.XmlXcaf);
            }

            using XdeDocument reopened = XdeDocument.Open(persisted);
            XdeLabel reloaded = Assert.Single(reopened.GetFreeShapes());
            Assert.Equal("Imported Assembly Root", reloaded.Name);
            Assert.Equal("Batch I", reloaded.Comment);
            using Shape owningCopy = reloaded.Shape;
            reopened.WriteStep(outputStep);
            reopened.Dispose();
            Assert.Equal(ShapeKind.Solid, owningCopy.Kind);
            using Shape exported = ShapeExchange.ReadStep(outputStep);
            Assert.Equal(ShapeKind.Solid, exported.Kind);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void AssertEquivalentGeneric(DocumentSnapshot left, DocumentSnapshot right)
    {
        using (left)
        using (right)
        {
            DocumentLabelSnapshot leftValue = Assert.Single(left.Labels, static label =>
                label.Attributes.Any(static attribute => attribute.Kind == DocumentAttributeKind.Name));
            DocumentLabelSnapshot rightValue = right.GetLabel(leftValue.Entry);
            Assert.Equal(leftValue.Tag, rightValue.Tag);
            Assert.Equal("永続化", Assert.Single(leftValue.Attributes,
                static attribute => attribute.Kind == DocumentAttributeKind.Name).TextValue);
            Assert.Equal(42, Assert.Single(rightValue.Attributes,
                static attribute => attribute.Kind == DocumentAttributeKind.IntegralValue).IntegerValue);
            DocumentRealArray array = Assert.Single(rightValue.Attributes,
                static attribute => attribute.Kind == DocumentAttributeKind.RealArray).RealArray!;
            Assert.Equal(7, array.LowerBound);
            Assert.Equal([2.25, 4.5], array.Values);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchI.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }
}
