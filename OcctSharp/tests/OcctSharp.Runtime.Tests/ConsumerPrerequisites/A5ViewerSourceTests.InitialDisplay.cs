using System.IO;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp.Runtime.Tests;

public sealed partial class A5ViewerSourceTests
{
    [Theory]
    [InlineData("shape", 0)] [InlineData("shape", 1)]
    [InlineData("label", 0)] [InlineData("label", 1)]
    [InlineData("occurrence", 0)] [InlineData("occurrence", 1)]
    [InlineData("source", 0)] [InlineData("source", 1)]
    [InlineData("styled-source", 0)] [InlineData("styled-source", 1)]
    public void InitialModeMatchesCompleteLegacyRequestedFrameAndKeepsLegacyDefault(string family, int requested)
    {
        using var window = new Window(); using var fixture = new InitialFixture(window.Viewer, family);
        var mode = (ViewerDisplayMode)requested; var capture = new ViewerCaptureOptions(320, 320);
        byte[] reference; ViewerCameraState camera;
        using (var legacy = fixture.Display(false, mode))
        {
            window.Viewer.FitAll(); camera = window.Viewer.GetCamera();
            byte[] initial = window.Viewer.Rendering.CaptureColor(capture).CopyPixels();
            legacy.SetDisplayMode(ViewerDisplayMode.Wireframe);
            Assert.Equal(initial, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
            legacy.SetDisplayMode(ViewerDisplayMode.Shaded);
            Assert.NotEqual(initial, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
            legacy.SetDisplayMode(mode); window.Viewer.SetCamera(camera);
            reference = window.Viewer.Rendering.CaptureColor(capture).CopyPixels();
            InitialImage(window.Viewer, family + "-" + requested + "-legacy.png");
        }
        using var direct = fixture.Display(true, mode); window.Viewer.SetCamera(camera);
        Assert.Equal(reference, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        InitialImage(window.Viewer, family + "-" + requested + "-direct.png");
        if (family == "occurrence")
        {
            Assert.NotNull(direct.SourceIdentity);
            Assert.Equal(fixture.Occurrence.Path, direct.SourceIdentity.OccurrencePath);
            Assert.Equal(fixture.Occurrence.OccurrenceLabel.Entry, direct.SourceIdentity.OccurrenceEntry);
            Assert.Equal(fixture.Occurrence.ReferredLabel.Entry, direct.SourceIdentity.ReferredEntry);
        }
        else if (family.Contains("source", StringComparison.Ordinal)) Assert.Same(fixture.Identity, direct.SourceIdentity);
    }

    [Theory]
    [InlineData(false, 0, 0)] [InlineData(false, 40, .4)] [InlineData(false, 80, -.2)]
    [InlineData(true, 0, 0)] [InlineData(true, 40, .4)] [InlineData(true, 80, -.2)]
    public void InitialModeSharedSelectionRetainsExactOriginalMembersAndCopiedIdentity(bool styled, double offset, double angle)
    {
        using var window = new Window(); using var shape = ShapeFactory.CreateBox(20, 10, 5);
        using var document = XdeDocument.Create(); XdeLabel label;
        using (var tx = document.BeginTransaction())
        { label = document.AddShape(shape, "Initial-mode selection source"); label.SetPresentationColor(XdeColorChannel.Surface, new(.05, .15, .8)); tx.Commit(); }
        using var source = styled ? window.Viewer.CreateSource(label) : window.Viewer.CreateSource(shape);
        using var transform = GpTrsf.Create(offset, 0, 0, rotationAngleRadians: angle);
        var identity = new ViewerSourceIdentity([label.Entry], label.Entry, label.Entry);
        using var instance = window.Viewer.DisplayInMode(source, transform, ViewerDisplayMode.Shaded, identity);
        source.Dispose(); document.Dispose();
        window.Viewer.SetProjection(ViewerProjection.Top); window.Viewer.FitAll();
        foreach (ShapeKind? kind in new ShapeKind?[] { null, ShapeKind.Face, ShapeKind.Edge })
        {
            instance.SetSelectionKind(kind); GpPoint local = kind == ShapeKind.Edge ? new(10, 0, 5) : new(10, 5, 5);
            var world = new GpPoint(
                transform.Value(1,1)*local.X + transform.Value(1,2)*local.Y + transform.Value(1,3)*local.Z + transform.Value(1,4),
                transform.Value(2,1)*local.X + transform.Value(2,2)*local.Y + transform.Value(2,3)*local.Z + transform.Value(2,4),
                transform.Value(3,1)*local.X + transform.Value(3,2)*local.Y + transform.Value(3,3)*local.Z + transform.Value(3,4));
            var pixel = window.Viewer.WorldToScreen(world);
            Assert.Same(instance, Assert.Single(window.Viewer.SelectAt(pixel.X, pixel.Y)));
            using var detected = window.Viewer.GetDetectedItem(); Assert.NotNull(detected);
            Assert.Same(instance, detected.Presentation); Assert.Same(identity, detected.SourceIdentity);
            Assert.True(RepairSnapshot.FindTopologyPartnerIndex(shape, detected.Shape) >= 0);
            var selected = window.Viewer.GetSelectedItems();
            try
            {
                var item = Assert.Single(selected); Assert.Same(identity, item.SourceIdentity);
                Assert.True(RepairSnapshot.FindTopologyPartnerIndex(shape, item.Shape) >= 0);
                if (offset == 0 && angle == 0) Assert.True(RepairSnapshot.FindTopologyIndex(shape, item.Shape) >= 0);
            }
            finally { foreach (var item in selected) item.Dispose(); }
            window.Viewer.ClearSelection();
        }
    }

    [Theory]
    [InlineData("shape")] [InlineData("label")] [InlineData("occurrence")] [InlineData("source")] [InlineData("styled-source")]
    public void InitialModeCallerReleaseAndIndependentAppearanceRestoreCompleteFrame(string family)
    {
        using var window = new Window(); using var fixture = new InitialFixture(window.Viewer, family);
        using var presentation = fixture.Display(true, ViewerDisplayMode.Shaded); window.Viewer.FitAll();
        var capture = new ViewerCaptureOptions(320, 320); byte[] baseline = window.Viewer.Rendering.CaptureColor(capture).CopyPixels();
        fixture.Dispose();
        Assert.Equal(baseline, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        window.Viewer.Rendering.SetAppearance(presentation, new() { Front = new(new(.9, .1, .05)), Shading = ViewerShading.Unlit });
        Assert.NotEqual(baseline, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        window.Viewer.Rendering.ResetAppearance(presentation); Assert.Equal(baseline, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        presentation.Hide(); Assert.NotEqual(baseline, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        presentation.Show(); Assert.Equal(baseline, window.Viewer.Rendering.CaptureColor(capture).CopyPixels());
        window.Viewer.Dispose(); Assert.Throws<ObjectDisposedException>(() => presentation.Show());
    }

    [Theory]
    [InlineData("shape")] [InlineData("label")] [InlineData("occurrence")] [InlineData("source")] [InlineData("styled-source")]
    public void InitialModeManagedWrongThreadInvalidModeAndDisposedOwnersAreRejected(string family)
    {
        using var window = new Window(); using var fixture = new InitialFixture(window.Viewer, family);
        Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Display(true, (ViewerDisplayMode)99));
        Exception? wrong = null;
        var thread = new Thread(() => wrong = Record.Exception(() => fixture.Display(true, ViewerDisplayMode.Shaded)));
        thread.Start(); thread.Join();
        Assert.IsType<InvalidOperationException>(wrong);
        fixture.Dispose(); Assert.Throws<ObjectDisposedException>(() => fixture.Display(true, ViewerDisplayMode.Shaded));
    }

    [Fact]
    public void InitialModeForeignAndRemovedSharedSourcesAreRejectedAndIsolationRestoresNewPresentation()
    {
        using var first = new Window(); using var other = new Window(); using var box = ShapeFactory.CreateBox(20, 10, 5);
        using var source = first.Viewer.CreateSource(box); using var identity = GpTrsf.Identity;
        Assert.Throws<ArgumentException>(() => other.Viewer.DisplayInMode(source, identity, ViewerDisplayMode.Shaded));
        using var selected = first.Viewer.DisplayInMode(source, identity, ViewerDisplayMode.Shaded);
        first.Viewer.SetProjection(ViewerProjection.Top); first.Viewer.FitAll();
        var pixel = first.Viewer.WorldToScreen(new(10, 5, 5)); Assert.Same(selected, Assert.Single(first.Viewer.SelectAt(pixel.X, pixel.Y)));
        first.Viewer.IsolateSelected(); using var transform = GpTrsf.Create(40, 0, 0);
        using var hidden = first.Viewer.DisplayInMode(source, transform, ViewerDisplayMode.Shaded);
        Assert.False(hidden.IsVisible); Assert.True(selected.IsVisible); Assert.True(first.Viewer.RestoreIsolation()); Assert.True(hidden.IsVisible);
        source.Dispose(); Assert.Throws<ObjectDisposedException>(() => first.Viewer.DisplayInMode(source, identity, ViewerDisplayMode.Shaded));
    }

    [Theory]
    [InlineData("shape")] [InlineData("label")] [InlineData("source")]
    public unsafe void InitialModeRawCAbiFailuresZeroOutputAndDoNotRegisterPresentation(string family)
    {
        using var window = new Window(); using var fixture = new InitialFixture(window.Viewer, family);
        nint library = NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "occt", "OcctSharp.Native.dll"));
        try
        {
            nint viewer = window.Viewer.Handle.DangerousGetHandle();
            var shapeCall = Marshal.GetDelegateForFunctionPointer<InitialShapeCall>(NativeLibrary.GetExport(library, "occtsharp_viewer_display_shape_in_mode"));
            var labelCall = Marshal.GetDelegateForFunctionPointer<InitialLabelCall>(NativeLibrary.GetExport(library, "occtsharp_viewer_display_xde_label_in_mode"));
            var sourceCall = Marshal.GetDelegateForFunctionPointer<InitialSourceCall>(NativeLibrary.GetExport(library, "occtsharp_viewer_source_display_in_mode"));
            int Invoke(nint owner, nint input, int mode, nint output) => family switch
            {
                "shape" => shapeCall(owner, input, mode, output),
                "label" => labelCall(owner, input, fixture.Label.Entry, mode, output),
                _ => sourceCall(owner, fixture.Source.Id, input, mode, output)
            };
            nint borrowed = family switch { "shape" => fixture.Shape.Handle.DangerousGetHandle(), "label" => fixture.Document.Handle.DangerousGetHandle(), _ => fixture.Transform.Handle.DangerousGetHandle() };
            foreach (int invalid in new[] { -1, 2, int.MaxValue })
            { long output = 123; Assert.NotEqual(0, Invoke(viewer, borrowed, invalid, (nint)(&output))); Assert.Equal(0, output); }
            long nullOwner = 123; Assert.NotEqual(0, Invoke(0, borrowed, 1, (nint)(&nullOwner))); Assert.Equal(0, nullOwner);
            long nullInput = 123; Assert.NotEqual(0, Invoke(viewer, 0, 1, (nint)(&nullInput))); Assert.Equal(0, nullInput);
            Assert.NotEqual(0, Invoke(viewer, borrowed, 1, 0));
            using var healthy = fixture.Display(true, ViewerDisplayMode.Shaded); window.Viewer.FitAll();
            Assert.NotEmpty(window.Viewer.Rendering.CaptureColor(new(320, 320)).CopyPixels());
        }
        finally { NativeLibrary.Free(library); }
    }

    private static void InitialImage(OcctViewer viewer, string name)
    {
        string? directory = Environment.GetEnvironmentVariable("OCCTSHARP_INITIAL_DISPLAY_EVIDENCE_DIR");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory); string path = Path.Combine(directory, name); Assert.False(File.Exists(path));
        viewer.SaveScreenshot(path, overwrite: false);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int InitialShapeCall(nint viewer, nint shape, int mode, nint output);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int InitialLabelCall(nint viewer, nint document, [MarshalAs(UnmanagedType.LPUTF8Str)] string entry, int mode, nint output);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int InitialSourceCall(nint viewer, long source, nint transform, int mode, nint output);

    private sealed class InitialFixture : IDisposable
    {
        private readonly OcctViewer viewer; private readonly string family;
        public Shape Shape { get; } = ShapeFactory.CreateBox(20, 10, 5);
        public XdeDocument Document { get; } = XdeDocument.Create();
        public GpTrsf Transform { get; } = GpTrsf.Create(40, 0, 0, rotationAngleRadians: .4);
        public XdeLabel Label { get; }
        public XdeOccurrence Occurrence { get; }
        public ViewerSource Source { get; }
        public ViewerSourceIdentity Identity { get; }
        public InitialFixture(OcctViewer viewer, string family)
        {
            this.viewer = viewer; this.family = family; XdeLabel assembly;
            using (var tx = Document.BeginTransaction())
            {
                Label = Document.AddShape(Shape, "Initial-mode part"); Label.SetPresentationColor(XdeColorChannel.Surface, new(.05, .15, .8));
                assembly = Document.AddAssembly("Initial-mode assembly"); using var location = TopLocLocation.FromTransform(Transform);
                Document.AddComponent(assembly, Label, location); tx.Commit();
            }
            Occurrence = Assert.Single(assembly.GetOccurrences());
            Source = family == "styled-source" ? viewer.CreateSource(Label) : viewer.CreateSource(Shape);
            Identity = new(Occurrence.Path, Occurrence.OccurrenceLabel.Entry, Occurrence.ReferredLabel.Entry);
        }
        public ViewerPresentation Display(bool direct, ViewerDisplayMode mode) => family switch
        {
            "shape" => direct ? viewer.DisplayInMode(Shape, mode) : viewer.Display(Shape),
            "label" => direct ? viewer.DisplayInMode(Label, mode) : viewer.Display(Label),
            "occurrence" => direct ? viewer.DisplayInMode(Occurrence, mode) : viewer.Display(Occurrence),
            _ => direct ? viewer.DisplayInMode(Source, Transform, mode, Identity) : viewer.Display(Source, Transform, Identity)
        };
        public void Dispose() { Source.Dispose(); Occurrence.Dispose(); Transform.Dispose(); Document.Dispose(); Shape.Dispose(); }
    }
}
