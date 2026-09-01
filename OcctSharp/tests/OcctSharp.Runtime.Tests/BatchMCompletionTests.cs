using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchMCompletionTests
{
    [Fact]
    public void PresentationTransformAndManipulatorConfigurationRoundTripAsCopiedValues()
    {
        Assert.Equal(144, Marshal.SizeOf<OcctSharp.Interop.ViewerManipulatorStateRaw>());
        nint window = CreateTestWindow();
        try
        {
            using OcctViewer viewer = OcctViewer.Create(window);
            using Shape shape = ShapeFactory.CreateBox(10, 20, 30);
            using ViewerPresentation presentation = viewer.Display(shape);
            using GpTrsf translation = GpTrsf.Create(7, 8, 9);
            presentation.SetTransform(translation);
            using (GpTrsf copied = presentation.GetTransform())
            {
                Assert.Equal(7, copied.Value(1, 4), 10);
                Assert.Equal(8, copied.Value(2, 4), 10);
                Assert.Equal(9, copied.Value(3, 4), 10);
            }
            presentation.ResetTransform();
            using (GpTrsf reset = presentation.GetTransform()) Assert.Equal(0, reset.Value(1, 4), 10);

            GpAx2Value position = GpAx2Value.Create(new(1, 2, 3), new(0, 0, 1), new(1, 0, 0));
            using ViewerManipulator manipulator = presentation.CreateManipulator(new()
            {
                ActivationOnDetection = true,
                ZoomPersistence = true,
                Skin = ViewerManipulatorSkin.Flat,
                EnabledModes = ViewerManipulatorModes.All,
                Size = 120,
                Gap = 12,
                Position = position
            });
            manipulator.SetPart(ViewerManipulatorAxis.X, ViewerManipulatorMode.Scaling, false);
            manipulator.SetPart(ViewerManipulatorAxis.X, ViewerManipulatorMode.Scaling, true);
            manipulator.EnableMode(ViewerManipulatorMode.Translation);
            manipulator.EnableMode(ViewerManipulatorMode.Rotation);
            manipulator.EnableMode(ViewerManipulatorMode.Scaling);
            manipulator.EnableMode(ViewerManipulatorMode.TranslationPlane);
            ViewerManipulatorState state = manipulator.State;
            Assert.True(state.IsAttached);
            Assert.True(state.ActivationOnDetection);
            Assert.True(state.ZoomPersistence);
            Assert.Equal(ViewerManipulatorSkin.Flat, state.Skin);
            Assert.Equal(120, state.Size, 5);
            Assert.Equal(12, state.Gap, 5);
            Assert.Equal(position.Origin, state.Position.Origin);
            Assert.Throws<ArgumentException>(() => viewer.FitAll());
            manipulator.Dispose();
            viewer.FitAll();
            viewer.Redraw();
        }
        finally { Assert.True(NativeWindowMethods.DestroyWindow(window)); }
    }

    [Fact]
    public void ManipulatorCustomPreviewAppliesOrCancelsAndMousePathReturnsOwningTransform()
    {
        nint window = CreateTestWindow();
        try
        {
            using OcctViewer viewer = OcctViewer.Create(window);
            using Shape shape = ShapeFactory.CreateBox(10, 10, 10);
            using ViewerPresentation presentation = viewer.Display(shape);
            using ViewerManipulator manipulator = presentation.CreateManipulator(new()
            {
                EnabledModes = ViewerManipulatorModes.Rigid,
                ActivationOnDetection = true,
                Skin = ViewerManipulatorSkin.Flat,
                Size = 120,
                Gap = 12
            });
            using GpTrsf translation = GpTrsf.Create(15, 0, 0);

            manipulator.Start(128, 128);
            manipulator.Preview(translation);
            Assert.True(manipulator.State.HasActiveTransformation);
            using (GpTrsf preview = presentation.GetTransform()) Assert.Equal(15, preview.Value(1, 4), 8);
            manipulator.Stop(apply: false);
            using (GpTrsf cancelled = presentation.GetTransform()) Assert.Equal(0, cancelled.Value(1, 4), 8);

            manipulator.Start(128, 128);
            using (GpTrsf mouse = manipulator.Transform(132, 128)) Assert.True(double.IsFinite(mouse.Value(1, 4)));
            manipulator.Preview(translation);
            manipulator.Stop(apply: true);
            using (GpTrsf applied = presentation.GetTransform()) Assert.Equal(15, applied.Value(1, 4), 8);
            Assert.False(manipulator.State.HasActiveTransformation);
            Assert.Throws<ArgumentException>(() => viewer.FitAll());
            manipulator.Dispose();
            viewer.FitAll();
            viewer.Redraw();
        }
        finally { Assert.True(NativeWindowMethods.DestroyWindow(window)); }
    }

    [Fact]
    public void ManipulatorLifetimeParentRemovalAndThreadAffinityFailClosed()
    {
        nint window = CreateTestWindow();
        try
        {
            using OcctViewer viewer = OcctViewer.Create(window);
            using Shape shape = ShapeFactory.CreateBox(1, 2, 3);
            ViewerPresentation presentation = viewer.Display(shape);
            ViewerManipulator manipulator = presentation.CreateManipulator();
            Exception? threadError = null;
            Thread thread = new(() =>
            {
                try { _ = manipulator.State; }
                catch (Exception error) { threadError = error; }
            });
            thread.Start();
            thread.Join();
            Assert.IsType<InvalidOperationException>(threadError);

            presentation.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _ = manipulator.State);
            manipulator.Dispose();
            presentation.Dispose();
        }
        finally { Assert.True(NativeWindowMethods.DestroyWindow(window)); }
    }

    [Fact]
    public void OccurrencePreviewCommitHistoryDmuStepAndRealHwndFormOneClosure()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchM.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        try
        {
            string step = Path.Combine(directory, "batch-m.step");
            using Shape partShape = ShapeFactory.CreateBox(10, 10, 10);
            using XdeDocument document = XdeDocument.Create();
            document.UndoLimit = 8;
            XdeLabel root;
            XdeLabel moving;
            using (XdeTransaction transaction = document.BeginTransaction("Batch M assembly"))
            {
                XdeLabel part = document.AddShape(partShape, "Batch M Part");
                root = document.AddAssembly("Batch M Root");
                using TopLocLocation first = Location(0, 0, 0);
                using TopLocLocation second = Location(30, 0, 0);
                moving = document.AddComponent(root, part, first);
                _ = document.AddComponent(root, part, second);
                Assert.True(transaction.Commit());
            }

            using OcctViewer viewer = OcctViewer.Create(window);
            IReadOnlyList<XdeOccurrence> occurrenceSnapshots = root.GetOccurrences();
            using XdeOccurrence occurrence = occurrenceSnapshots.Single(item => item.OccurrenceLabel.Entry == moving.Entry);
            foreach (XdeOccurrence other in occurrenceSnapshots)
                if (!ReferenceEquals(other, occurrence)) other.Dispose();
            using ViewerPresentation presentation = viewer.Display(occurrence);
            using ViewerManipulator manipulator = presentation.CreateManipulator(new()
            {
                EnabledModes = ViewerManipulatorModes.Rigid,
                ActivationOnDetection = true
            });

            using (GpTrsf cancelledPlacement = GpTrsf.Create(5, 0, 0))
            using (XdePlacementEditSession cancelled = document.BeginPlacementEdit(moving, presentation, "cancelled move"))
            {
                cancelled.Preview(cancelledPlacement);
                Assert.True(cancelled.HasPreview);
            }
            using (GpTrsf restored = presentation.GetTransform()) Assert.Equal(0, restored.Value(1, 4), 8);

            XdeLabel replacement;
            using (GpTrsf movedPlacement = GpTrsf.Create(25, 0, 0))
            using (XdePlacementEditSession edit = document.BeginPlacementEdit(moving, presentation, "Batch M placement"))
            {
                edit.Preview(movedPlacement);
                replacement = edit.Commit();
                Assert.True(edit.IsCompleted);
                Assert.Equal(replacement.Entry, presentation.SourceIdentity!.OccurrenceEntry);
            }
            Assert.Equal("Batch M placement", document.UndoHistory[0].Name);
            using (DigitalMockupReport report = DigitalMockupAnalyzer.AnalyzeAssembly(
                       root, new DigitalMockupPolicy { ExactDistanceForAllPairs = true }))
                Assert.Equal(DigitalMockupPairState.Interfering, Assert.Single(report.Pairs).State);

            Assert.True(document.Undo());
            Assert.True(document.Redo());
            document.WriteStep(step);
            using XdeDocument reread = XdeDocument.ReadStep(step);
            IReadOnlyList<XdeOccurrence> rereadOccurrences = Assert.Single(reread.GetFreeShapes()).GetOccurrences();
            try { Assert.Equal(2, rereadOccurrences.Count); }
            finally { foreach (XdeOccurrence item in rereadOccurrences) item.Dispose(); }
            viewer.FitAll();
            string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "batch-m.png"));
            Assert.True(new FileInfo(screenshot).Length > 0);
            manipulator.Dispose();
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static TopLocLocation Location(double x, double y, double z)
    {
        using GpTrsf transform = GpTrsf.Create(x, y, z);
        return TopLocLocation.FromTransform(transform);
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(0, "STATIC", "OcctSharp Batch M", 0x80000000u,
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
