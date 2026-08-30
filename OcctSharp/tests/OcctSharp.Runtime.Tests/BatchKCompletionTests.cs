using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchKCompletionTests
{
    [Fact]
    public void NestedSharedAssemblyCopiesPathsGraphBomReferencesMetadataAndPhysicalRollup()
    {
        using Shape source = ShapeFactory.CreateBox(2, 3, 4);
        using XdeDocument document = XdeDocument.Create();
        XdeLabel part;
        XdeLabel root;
        XdeLabel nestedOccurrence;
        XdeLabel subassemblyOccurrence;
        XdeLabel directOccurrence;
        using (XdeTransaction transaction = document.BeginTransaction("build shared assembly"))
        {
            part = document.AddShape(source, "Shared Part");
            part.Color = new(0.15, 0.35, 0.75, 1);
            part.SetLayer("Mechanical");
            part.Material = new XdeMaterial("Steel", "Batch K", 2, "Density", "u/mm3");
            part.VisualMaterial = Visual("Definition blue", new(0.15, 0.35, 0.75, 1));
            XdeLabel nested = document.AddAssembly("Nested Assembly");
            using (TopLocLocation nestedPartLocation = Location(2, 3, 4))
                nestedOccurrence = document.AddComponent(nested, part, nestedPartLocation);

            root = document.AddAssembly("Root Assembly");
            using (TopLocLocation nestedLocation = Location(10, 0, 0))
                subassemblyOccurrence = document.AddComponent(root, nested, nestedLocation);
            using (TopLocLocation directLocation = Location(30, 0, 0))
                directOccurrence = document.AddComponent(root, part, directLocation);
            document.SetExternalReferences(directOccurrence, ["parts/shared.step", "urn:occtsharp:shared-part"]);

            document.SetOccurrenceMetadata(directOccurrence, new(
                "Direct Override",
                new XdeColor(0.8, 0.2, 0.1, 1),
                ["Override Layer"],
                new XdeMaterial("Dense Steel", "Occurrence override", 3, "Density", "u/mm3"),
                Visual("Override red", new(0.8, 0.2, 0.1, 1))));

            AssemblyItemReference itemReference = document.SetAssemblyItemReference(
                root, [subassemblyOccurrence.Entry, nestedOccurrence.Entry]);
            Assert.Equal([subassemblyOccurrence.Entry, nestedOccurrence.Entry], itemReference.Path);
            AssemblyShuo shuo = document.CreateShuo([subassemblyOccurrence, nestedOccurrence]);
            Assert.False(string.IsNullOrWhiteSpace(shuo.Entry));
            Assert.True(transaction.Commit());
        }

        source.Dispose();
        Assert.Equal(["parts/shared.step", "urn:occtsharp:shared-part"], document.GetExternalReferences(directOccurrence));
        AssemblyItemReference copiedReference = Assert.IsType<AssemblyItemReference>(document.GetAssemblyItemReference(root));
        Assert.Equal([subassemblyOccurrence.Entry, nestedOccurrence.Entry], copiedReference.Path);

        AssemblyEffectiveMetadata definitionFallback = document.GetEffectiveMetadata(nestedOccurrence);
        Assert.Equal("Shared Part", definitionFallback.Name);
        Assert.Equal("Steel", definitionFallback.Material?.Name);
        AssemblyEffectiveMetadata occurrenceOverride = document.GetEffectiveMetadata(directOccurrence);
        Assert.Equal("Direct Override", occurrenceOverride.Name);
        Assert.Equal("Dense Steel", occurrenceOverride.Material?.Name);
        Assert.Equal(["Override Layer"], occurrenceOverride.Layers);

        using (AssemblyOccurrenceResolution resolution = document.ResolveOccurrencePath(
                   root, [subassemblyOccurrence.Entry, nestedOccurrence.Entry]))
        {
            Assert.Equal(part.Entry, resolution.Definition.Entry);
            Assert.True(resolution.LocatedShape.IsValid);
            BoundingBox3d bounds = resolution.LocatedShape.GetBoundingBox();
            AssertPoint(new(12, 3, 4), bounds.Minimum);
            AssertPoint(new(14, 6, 8), bounds.Maximum);
        }
        Assert.Throws<KeyNotFoundException>(() => document.ResolveOccurrencePath(root, ["0:404"]));

        AssemblyStructureSnapshot graph = document.CreateAssemblyStructureSnapshot(root);
        Assert.Equal(6, graph.Nodes.Count);
        Assert.Equal(6, graph.Links.Count);
        Assert.Empty(graph.Diagnostics);
        Assert.Equal(2, document.GetWhereUsed(part).Count);

        AssemblyBomReport structured = document.CreateBom(root);
        Assert.False(structured.IsFlattened);
        Assert.Equal(3, structured.Items.Count);
        AssemblyBomReport flattened = document.CreateBom(root, flattened: true);
        Assert.True(flattened.IsFlattened);
        Assert.Equal(2, flattened.Items.Count);
        Assert.Equal(2, Assert.Single(flattened.Items, item => item.DefinitionEntry == part.Entry).Quantity);

        AssemblyPropertyRollup rollup = document.GetAssemblyPropertyRollup(root);
        Assert.Equal(2, rollup.OccurrenceCount);
        Assert.Equal(120, rollup.Mass, 8);
        AssertPoint(new(12, 0, 0), rollup.Bounds.Minimum);
        AssertPoint(new(32, 6, 8), rollup.Bounds.Maximum);
        AssemblyPropertyGroup partGroup = Assert.Single(rollup.Groups);
        Assert.Equal(2, partGroup.Quantity);
    }

    [Fact]
    public void RelocateRelinkCloneReparentRemoveAndDefinitionReplacementAreAtomicAndUndoable()
    {
        using Shape firstShape = ShapeFactory.CreateBox(3, 4, 5);
        using Shape secondShape = ShapeFactory.CreateCylinder(2, 6);
        using Shape replacementShape = ShapeFactory.CreateBox(7, 8, 9);
        using XdeDocument document = XdeDocument.Create();
        document.UndoLimit = 16;
        XdeLabel first;
        XdeLabel second;
        XdeLabel root;
        XdeLabel occurrence;
        using (XdeTransaction transaction = document.BeginTransaction("initial assembly"))
        {
            first = document.AddShape(firstShape, "First");
            second = document.AddShape(secondShape, "Second");
            root = document.AddAssembly("Editable Root");
            using TopLocLocation identity = TopLocLocation.Identity;
            occurrence = document.AddComponent(root, first, identity);
            occurrence.Name = "Occurrence metadata";
            Assert.True(transaction.Commit());
        }

        string cloneEntry;
        string movedEntry;
        using (XdeTransaction transaction = document.BeginTransaction("Batch K structural edit"))
        {
            XdeLabel clone = document.CloneSubtree(root, "Independent Clone");
            cloneEntry = clone.Entry;
            using TopLocLocation relocatedPosition = Location(11, 12, 13);
            XdeLabel relocated = document.RelocateOccurrence(occurrence, relocatedPosition);
            XdeLabel relinked = document.ReplaceOccurrence(relocated, second);
            Assert.Equal("Occurrence metadata", relinked.Name);
            XdeLabel moved = document.ReparentOccurrence(relinked, clone);
            movedEntry = moved.Entry;
            document.UpdateDefinitionShape(second, replacementShape);
            Assert.True(transaction.Commit());
        }

        firstShape.Dispose();
        secondShape.Dispose();
        replacementShape.Dispose();
        Assert.Empty(root.GetComponents());
        XdeLabel editedClone = document.GetLabel(cloneEntry);
        Assert.Equal(2, editedClone.GetComponents().Count);
        using (Shape copiedReplacement = second.Shape)
        {
            GpPoint maximum = copiedReplacement.GetBoundingBox().Maximum;
            Assert.Equal(7, maximum.X, 6);
            Assert.Equal(8, maximum.Y, 6);
            Assert.Equal(9, maximum.Z, 6);
        }

        Assert.Equal("Batch K structural edit", document.UndoHistory[0].Name);
        Assert.True(document.Undo());
        Assert.Single(root.GetComponents());
        Assert.True(document.Redo());
        Assert.Empty(root.GetComponents());
        editedClone = document.GetLabel(cloneEntry);
        Assert.Equal(2, editedClone.GetComponents().Count);

        using (XdeTransaction aborted = document.BeginTransaction("aborted removal"))
        {
            document.RemoveOccurrence(document.GetLabel(movedEntry));
            aborted.Abort();
        }
        Assert.Equal(2, document.GetLabel(cloneEntry).GetComponents().Count);

        using (XdeTransaction transaction = document.BeginTransaction("remove used definition"))
        {
            Assert.Throws<InvalidOperationException>(() => document.RemoveDefinition(second));
            document.RemoveDefinition(second, AssemblyDefinitionRemovalPolicy.RemoveOccurrences);
            Assert.True(transaction.Commit());
        }
        Assert.Single(document.GetLabel(cloneEntry).GetComponents());
        using Shape cloneShape = document.GetLabel(cloneEntry).Shape;
        Assert.True(cloneShape.IsValid);
    }

    [Fact]
    public void DiagnosticsSnapshotsAndCopiedResultsSurviveSourceAndDocumentDisposal()
    {
        AssemblyStructureSnapshot copiedGraph;
        AssemblyBomReport copiedBom;
        AssemblyOccurrenceResolution owningResolution;
        XdeLabel disposedLabel;
        using Shape partShape = ShapeFactory.CreateBox(4, 5, 6);
        using Shape orphanShape = ShapeFactory.CreateSphere(1);
        using (XdeDocument document = XdeDocument.Create())
        {
            XdeLabel part;
            XdeLabel root;
            XdeLabel occurrence;
            using (XdeTransaction transaction = document.BeginTransaction("copy snapshots"))
            {
                part = document.AddShape(partShape, "Reachable Part");
                _ = document.AddShape(orphanShape, "Orphan Part");
                root = document.AddAssembly("Snapshot Root");
                using TopLocLocation identity = TopLocLocation.Identity;
                occurrence = document.AddComponent(root, part, identity);
                Assert.True(transaction.Commit());
            }
            copiedGraph = document.CreateAssemblyStructureSnapshot(root);
            copiedBom = document.CreateBom(root);
            owningResolution = document.ResolveOccurrencePath(root, [occurrence.Entry]);
            disposedLabel = occurrence;
            Assert.Contains(copiedGraph.Diagnostics, item => item.Code == AssemblyDiagnosticCode.OrphanDefinition);
        }

        partShape.Dispose();
        Assert.NotEmpty(copiedGraph.Nodes);
        Assert.Single(copiedBom.Items);
        Assert.True(owningResolution.LocatedShape.IsValid);
        Assert.Throws<ObjectDisposedException>(() => _ = disposedLabel.Name);
        owningResolution.Dispose();
    }

    [Fact]
    public void AssemblyRoundTripsThroughStepAndRealHwndViewerWithOccurrencePaths()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchK.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        try
        {
            string step = Path.Combine(directory, "batch-k.step");
            using (Shape shape = ShapeFactory.CreateBox(8, 6, 4))
            using (XdeDocument source = XdeDocument.Create())
            {
                using XdeTransaction transaction = source.BeginTransaction("STEP assembly authoring");
                XdeLabel part = source.AddShape(shape, "STEP Part");
                part.Color = new(0.2, 0.7, 0.4, 1);
                part.Material = new XdeMaterial("STEP Steel", "Round trip", 7.85, "Density", "g/cm3");
                source.SetExternalReferences(part, ["urn:occtsharp:batch-k"]);
                XdeLabel root = source.AddAssembly("STEP Root");
                using TopLocLocation placement = Location(9, 10, 11);
                XdeLabel occurrence = source.AddComponent(root, part, placement);
                source.SetOccurrenceMetadata(occurrence, new(
                    "STEP Occurrence", new XdeColor(0.9, 0.3, 0.2, 1), ["STEP Layer"],
                    null, null));
                Assert.True(transaction.Commit());
                source.WriteStep(step);
            }

            using XdeDocument imported = XdeDocument.ReadStep(step);
            XdeLabel importedRoot = Assert.Single(imported.GetFreeShapes());
            Assert.True(importedRoot.IsAssembly);
            AssemblyBomItem importedItem = Assert.Single(imported.CreateBom(importedRoot).Items);
            Assert.Equal("STEP Occurrence", importedItem.Name);
            using OcctViewer viewer = OcctViewer.Create(window);
            IReadOnlyList<AssemblyViewerPresentation> presentations = imported.DisplayAssembly(importedRoot, viewer);
            try
            {
                Assert.Single(presentations);
                Assert.Single(presentations[0].Path);
                viewer.FitAll();
                viewer.Redraw();
                string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-k.png"));
                Assert.True(new FileInfo(screenshot).Length > 0);
            }
            finally
            {
                foreach (AssemblyViewerPresentation presentation in presentations) presentation.Dispose();
            }
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static XdeVisualMaterial Visual(string name, XdeColor color) =>
        new(name, color, 0.25, 0.55, new GpXyz(0.01, 0.02, 0.03));

    private static void AssertPoint(GpPoint expected, GpPoint actual)
    {
        Assert.Equal(expected.X, actual.X, 6);
        Assert.Equal(expected.Y, actual.Y, 6);
        Assert.Equal(expected.Z, actual.Z, 6);
    }

    private static TopLocLocation Location(double x, double y, double z)
    {
        using GpTrsf transform = GpTrsf.Create(x, y, z);
        return TopLocLocation.FromTransform(transform);
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch K", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
