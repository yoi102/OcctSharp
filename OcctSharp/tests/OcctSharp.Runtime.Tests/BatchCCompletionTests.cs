using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchCCompletionTests
{
    [Fact]
    public void StepReaderSessionCopiesUnitsTransfersSelectedRootsAndOwnsResults()
    {
        ExecuteInTempDirectory(directory =>
        {
            string inputPath = Path.Combine(directory, "session-input.step");
            using Shape left = ShapeFactory.CreateBox(2, 3, 4);
            using Shape rightSource = ShapeFactory.CreateBox(2, 3, 4);
            using Shape right = rightSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(6, 0, 0, 0));
            using Shape compound = ShapeFactory.CreateCompound([left, right]);
            ShapeExchange.WriteStep(compound, inputPath);

            StepReadSession session = StepReadSession.Open(inputPath, targetSystemLengthUnit: 2.5);
            Assert.Equal(StepReadStatus.Done, session.Info.ReadStatus);
            Assert.True(session.Info.CandidateRootCount > 0);
            Assert.Equal(2.5, session.Info.SystemLengthUnit, 10);
            Assert.NotEmpty(session.Info.FileUnits.Length);
            Assert.All(session.Info.FileUnits.Length, unit => Assert.False(string.IsNullOrWhiteSpace(unit)));

            Shape first = session.TransferRoot(0);
            Shape[] selected = session.TransferRoots([0]);
            session.Dispose();
            session.Dispose();
            Assert.Throws<ObjectDisposedException>(() => session.TransferRoot(0));

            using (first)
            {
                Assert.True(first.IsValid);
                Assert.True(first.CountSubShapes(ShapeKind.Solid) >= 2);
            }
            Assert.Single(selected);
            selected[0].Dispose();

            Assert.Throws<ArgumentOutOfRangeException>(() => StepReadSession.Open(inputPath, 0));
            Assert.Throws<FileNotFoundException>(() => StepReadSession.Open(Path.Combine(directory, "missing.step")));
        });

        Assert.Equal(32, Marshal.SizeOf<StepReaderInfoRaw>());
    }

    [Fact]
    public void ViewerSubshapeSelectionAndInputForwardingEnforceOwnershipAndThreadAffinity()
    {
        nint window = CreateTestWindow();
        ViewerSelectionItem? selectedItem = null;
        try
        {
            OcctViewer viewer = OcctViewer.Create(window);
            using Shape box = ShapeFactory.CreateBox(10, 20, 30);
            ViewerPresentation presentation = viewer.Display(box);
            presentation.SetSelectionKind(ShapeKind.Face);
            Assert.Throws<ArgumentOutOfRangeException>(() => presentation.SetSelectionKind(ShapeKind.Shape));
            viewer.FitAll();
            viewer.Redraw();

            ViewerInputController input = viewer.Input;
            Assert.True(input.PointerMoved(128, 128));
            input.PointerPressed(ViewerPointerButton.Left, 128, 128);
            Assert.Contains(presentation, input.PointerReleased(ViewerPointerButton.Left, 128, 128));
            IReadOnlyList<ViewerSelectionItem> selection = viewer.GetSelectedItems();
            selectedItem = Assert.Single(selection);
            Assert.Same(presentation, selectedItem.Presentation);
            Assert.Equal(ShapeKind.Face, selectedItem.Shape.Kind);

            input.MouseWheel(120, 128, 128);
            input.PointerPressed(ViewerPointerButton.Middle, 128, 128);
            Assert.False(input.PointerMoved(132, 126, ViewerPointerButtons.Middle));
            _ = input.PointerReleased(ViewerPointerButton.Middle, 132, 126);
            input.PointerPressed(ViewerPointerButton.Right, 128, 128);
            Assert.False(input.PointerMoved(136, 132, ViewerPointerButtons.Right));
            _ = input.PointerReleased(ViewerPointerButton.Right, 136, 132);
            Assert.True(input.KeyDown(ViewerInputKey.Front));
            Assert.True(input.KeyDown(ViewerInputKey.Axonometric));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                input.PointerMoved(0, 0, (ViewerPointerButtons)8));

            Exception? threadError = null;
            Thread worker = new(() =>
            {
                try { input.KeyDown(ViewerInputKey.FitAll); }
                catch (Exception error) { threadError = error; }
            });
            worker.Start();
            worker.Join();
            Assert.IsType<InvalidOperationException>(threadError);

            viewer.Dispose();
            Assert.Equal(ShapeKind.Face, selectedItem.Shape.Kind);
            Assert.Throws<ObjectDisposedException>(() => input.KeyDown(ViewerInputKey.FitAll));
        }
        finally
        {
            selectedItem?.Dispose();
            Assert.True(NativeWindowMethods.DestroyWindow(window));
        }
    }

    [Fact]
    public void SelectiveStepImportEditExportAndViewerWorkflowIsEndToEnd()
    {
        ExecuteInTempDirectory(directory =>
        {
            string inputPath = Path.Combine(directory, "batch-c-input.step");
            string outputPath = Path.Combine(directory, "batch-c-output.step");
            using Shape first = ShapeFactory.CreateBox(4, 5, 6);
            using Shape secondSource = ShapeFactory.CreateBox(4, 5, 6);
            using Shape second = secondSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(10, 0, 0, 0));
            using Shape source = ShapeFactory.CreateCompound([first, second]);
            ShapeExchange.WriteStep(source, inputPath);

            Shape imported;
            using (StepReadSession session = StepReadSession.Open(inputPath, 1.0))
                imported = session.TransferRoot(0);
            using (imported)
            {
                Shape[] solids = imported.GetSubShapes(ShapeKind.Solid);
                Assert.True(solids.Length >= 2);
                Shape edited;
                try { edited = imported.RemoveSubshape(solids[1]); }
                finally { foreach (Shape solid in solids) solid.Dispose(); }

                using (edited)
                {
                    Assert.Equal(1, edited.CountSubShapes(ShapeKind.Solid));
                    ShapeExchange.WriteStep(edited, outputPath);
                }
            }

            using Shape reread = ShapeExchange.ReadStep(outputPath);
            Assert.True(reread.IsValid);
            Assert.Equal(1, reread.CountSubShapes(ShapeKind.Solid));

            nint window = CreateTestWindow();
            try
            {
                using OcctViewer viewer = OcctViewer.Create(window);
                ViewerPresentation presentation = viewer.Display(reread);
                presentation.SetSelectionKind(ShapeKind.Face);
                viewer.FitAll();
                viewer.Redraw();
                viewer.Input.PointerPressed(ViewerPointerButton.Left, 128, 128);
                Assert.Contains(
                    presentation,
                    viewer.Input.PointerReleased(ViewerPointerButton.Left, 128, 128));
                IReadOnlyList<ViewerSelectionItem> selected = viewer.GetSelectedItems();
                try { Assert.Equal(ShapeKind.Face, Assert.Single(selected).Shape.Kind); }
                finally { foreach (ViewerSelectionItem item in selected) item.Dispose(); }
            }
            finally
            {
                Assert.True(NativeWindowMethods.DestroyWindow(window));
            }
        });
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp Batch C viewer test", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static void ExecuteInTempDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchC.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try { action(directory); }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(
            uint extendedStyle,
            string className,
            string windowName,
            uint style,
            int x,
            int y,
            int width,
            int height,
            nint parent,
            nint menu,
            nint instance,
            nint parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
