using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchDCompletionTests
{
    [Fact]
    public void CompleteViewportReviewClosureRunsAcrossXdeTopologySelectionPresentationCameraAndImage()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchD.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        ViewerDetectionItem? detected = null;
        ViewerSelectionItem[] selectedItems = [];
        try
        {
            using Shape source = ShapeFactory.CreateBox(10, 20, 30);
            using OcctViewer viewer = OcctViewer.Create(window);

            ViewerPresentation occurrencePresentation;
            string expectedOccurrenceEntry;
            string expectedReferredEntry;
            string reviewStep = Path.Combine(directory, "batch-d-review.step");
            using (XdeDocument document = XdeDocument.Create())
            {
                using (XdeTransaction transaction = document.BeginTransaction())
                {
                    XdeLabel part = document.AddShape(source, "Batch D Part");
                    XdeLabel assembly = document.AddAssembly("Batch D Assembly");
                    using TopLocLocation identity = TopLocLocation.Identity;
                    _ = document.AddComponent(assembly, part, identity);
                    Assert.True(transaction.Commit());
                }
                document.WriteStep(reviewStep);
            }
            using (XdeDocument imported = XdeDocument.ReadStep(reviewStep))
            {
                XdeLabel importedAssembly = Assert.Single(imported.GetFreeShapes());
                using XdeOccurrence occurrence = Assert.Single(importedAssembly.GetOccurrences());
                expectedOccurrenceEntry = occurrence.OccurrenceLabel.Entry;
                expectedReferredEntry = occurrence.ReferredLabel.Entry;
                occurrencePresentation = viewer.Display(occurrence);
            }

            Assert.NotNull(occurrencePresentation.SourceIdentity);
            Assert.Equal(expectedOccurrenceEntry, occurrencePresentation.SourceIdentity.OccurrenceEntry);
            Assert.Equal(expectedReferredEntry, occurrencePresentation.SourceIdentity.ReferredEntry);
            Assert.Contains(expectedOccurrenceEntry, occurrencePresentation.SourceIdentity.OccurrencePath);

            ViewerPresentation secondPresentation;
            using (Shape secondSource = ShapeFactory.CreateBox(5, 5, 5))
            using (Shape second = secondSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(40, 0, 0, 0)))
                secondPresentation = viewer.Display(second);

            occurrencePresentation.SetSelectionKind(ShapeKind.Face);
            secondPresentation.SetSelectionKind(ShapeKind.Face);
            viewer.SetPixelTolerance(4);
            viewer.FitAll();
            viewer.Redraw();

            ViewerPixelPoint occurrencePixel = viewer.WorldToScreen(new GpPoint(5, 10, 15));
            Assert.True(viewer.MoveTo(occurrencePixel.X, occurrencePixel.Y));
            detected = Assert.IsType<ViewerDetectionItem>(viewer.GetDetectedItem());
            Assert.Same(occurrencePresentation, detected.Presentation);
            Assert.Equal(expectedOccurrenceEntry, detected.SourceIdentity?.OccurrenceEntry);
            Assert.Equal(ShapeKind.Face, detected.Shape.Kind);

            using Shape styledFace = detected.Shape.Clone();
            occurrencePresentation.SetSubshapeColor(styledFace, new ViewerColor(0.8, 0.15, 0.1));
            occurrencePresentation.SetSubshapeTransparency(styledFace, 0.25);
            occurrencePresentation.SetSubshapeWidth(styledFace, 2.5);
            occurrencePresentation.ClearSubshapeOverrides(styledFace);
            occurrencePresentation.SetSubshapeColor(styledFace, new ViewerColor(0.2, 0.7, 0.3));
            occurrencePresentation.ClearAllSubshapeOverrides();

            using (Shape outsider = ShapeFactory.CreateSphere(2))
                Assert.Throws<ArgumentException>(() => occurrencePresentation.SetSubshapeColor(
                    outsider, new ViewerColor(1, 0, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => occurrencePresentation.SetSubshapeTransparency(styledFace, 1.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => occurrencePresentation.SetSubshapeWidth(styledFace, 0));

            occurrencePresentation.SetSelectionKind(null);
            secondPresentation.SetSelectionKind(null);
            Assert.NotEmpty(viewer.SelectRectangle(0, 0, 255, 255));
            _ = viewer.SelectRectangle(255, 255, 0, 0, ViewerSelectionMode.Add);
            Assert.NotEmpty(viewer.SelectPolygon(
                [new GpPoint2d(0, 0), new GpPoint2d(255, 0), new GpPoint2d(255, 255), new GpPoint2d(0, 255)],
                ViewerSelectionMode.Replace));
            _ = viewer.SelectPolygon(
                [new GpPoint2d(0, 0), new GpPoint2d(255, 0), new GpPoint2d(255, 255), new GpPoint2d(0, 255)],
                ViewerSelectionMode.Toggle);
            Assert.NotEmpty(viewer.SelectPolygon(
                [new GpPoint2d(0, 0), new GpPoint2d(255, 0), new GpPoint2d(255, 255), new GpPoint2d(0, 255)],
                ViewerSelectionMode.Add));
            Assert.Empty(viewer.SelectPolygon(
                [new GpPoint2d(0, 0), new GpPoint2d(255, 0), new GpPoint2d(255, 255), new GpPoint2d(0, 255)],
                ViewerSelectionMode.Remove));

            occurrencePresentation.SetSelectionKind(ShapeKind.Face);
            viewer.SetShapeFilter(ShapeKind.Face);
            Assert.True(viewer.MoveTo(occurrencePixel.X, occurrencePixel.Y));
            viewer.ClearFilters();
            Assert.Throws<ArgumentOutOfRangeException>(() => viewer.SetPixelTolerance(101));
            Assert.Throws<ArgumentException>(() => viewer.SelectRectangle(0, 0, 0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => viewer.SelectPolygon(
                [new GpPoint2d(0, 0), new GpPoint2d(1, 1)]));

            Assert.NotEmpty(viewer.SelectRectangle(0, 0, 255, 255));
            BoundingBox3d bounds = Assert.IsType<BoundingBox3d>(viewer.GetSelectionBounds());
            Assert.True(bounds.Maximum.X > bounds.Minimum.X);
            Assert.True(viewer.FitSelected());
            viewer.IsolateSelected();
            secondPresentation.Dispose();
            Assert.True(viewer.RestoreIsolation());
            viewer.ClearSelection();
            Assert.Null(viewer.GetSelectionBounds());
            Assert.False(viewer.FitSelected());
            Assert.False(viewer.RestoreIsolation());

            Assert.Contains(occurrencePresentation, viewer.SelectAt(occurrencePixel.X, occurrencePixel.Y));
            selectedItems = [.. viewer.GetSelectedItems()];
            ViewerSelectionItem selected = Assert.Single(selectedItems);
            Assert.Equal(expectedOccurrenceEntry, selected.SourceIdentity?.OccurrenceEntry);
            Assert.Equal(ShapeKind.Face, selected.Shape.Kind);

            ViewerCameraState camera = viewer.GetCamera();
            viewer.SetCamera(camera);
            Assert.Equal(camera, viewer.GetCamera());
            GpPoint world = viewer.ScreenToWorld(128, 128);
            ViewerPixelPoint pixel = viewer.WorldToScreen(world);
            Assert.InRange(Math.Abs(pixel.X - 128), 0, 1);
            Assert.InRange(Math.Abs(pixel.Y - 128), 0, 1);
            ViewerPickRay ray = viewer.GetPickRay(128, 128);
            Assert.InRange(Math.Sqrt(ray.Direction.X * ray.Direction.X
                + ray.Direction.Y * ray.Direction.Y + ray.Direction.Z * ray.Direction.Z), 0.999999, 1.000001);
            viewer.ZoomWindow(220, 220, 32, 32);
            Assert.Throws<ArgumentException>(() => viewer.ZoomWindow(1, 1, 1, 4));
            Assert.ThrowsAny<Exception>(() => viewer.SetCamera(camera with { Target = camera.Eye }));

            viewer.SetBackgroundColor(new ViewerColor(0.05, 0.08, 0.12));
            using (ViewerClipPlane plane = viewer.CreateClipPlane(new ViewerPlaneEquation(1, 0, 0, -5)))
            {
                plane.Update(new ViewerPlaneEquation(0, 1, 0, -10));
                plane.SetEnabled(false);
                Assert.False(plane.IsEnabled);
                plane.SetEnabled(true);
                Assert.True(plane.IsEnabled);
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => viewer.CreateClipPlane(default));
            viewer.SetComputedHiddenLine(true);
            viewer.SetComputedHiddenLine(false);
            viewer.ShowTrihedron(
                ViewerTrihedronPosition.RightLower, new ViewerColor(0.9, 0.9, 0.9), 0.08);
            viewer.HideTrihedron();

            string screenshot = viewer.SaveScreenshot(
                Path.Combine(directory, "审阅截图.png"), ViewerScreenshotBuffer.Rgb);
            Assert.True(new FileInfo(screenshot).Length > 0);
            Assert.Throws<IOException>(() => viewer.SaveScreenshot(screenshot));
            Assert.Equal(screenshot, viewer.SaveScreenshot(screenshot, overwrite: true));
            Assert.Throws<ArgumentException>(() => viewer.SaveScreenshot(Path.Combine(directory, "bad.txt")));

            Exception? threadError = null;
            Thread worker = new(() =>
            {
                try { viewer.GetCamera(); }
                catch (Exception error) { threadError = error; }
            });
            worker.Start();
            worker.Join();
            Assert.IsType<InvalidOperationException>(threadError);

            nint otherWindow = CreateTestWindow();
            try
            {
                using OcctViewer otherViewer = OcctViewer.Create(otherWindow);
                using ViewerClipPlane foreignPlane = otherViewer.CreateClipPlane(new ViewerPlaneEquation(0, 0, 1, 0));
                using ViewerClipPlane localPlane = viewer.CreateClipPlane(new ViewerPlaneEquation(0, 0, 1, -1));
                Assert.Throws<ArgumentException>(() => otherViewer.RemoveClipPlane(localPlane));
                Assert.Throws<ArgumentException>(() => otherViewer.SetVisible(occurrencePresentation, true));
            }
            finally { Assert.True(NativeWindowMethods.DestroyWindow(otherWindow)); }
        }
        finally
        {
            foreach (ViewerSelectionItem item in selectedItems) item.Dispose();
            detected?.Dispose();
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp Batch D viewer test", 0x80000000u,
            -32000, -32000, 256, 256, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(
            uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height,
            nint parent, nint menu, nint instance, nint parameter);

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
