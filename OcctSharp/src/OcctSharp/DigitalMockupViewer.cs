namespace OcctSharp;

/// <summary>Viewer-parent-bound presentations for deterministic digital mock-up issue review.</summary>
public sealed class DigitalMockupReviewSession : IDisposable
{
    private readonly OcctViewer _viewer;
    private readonly Dictionary<DigitalMockupPairId, ViewerPresentation[]> _presentations;
    private bool _disposed;

    private DigitalMockupReviewSession(
        OcctViewer viewer,
        Dictionary<DigitalMockupPairId, ViewerPresentation[]> presentations)
    {
        _viewer = viewer;
        _presentations = presentations;
    }

    /// <summary>Displays independently owned issue/contact/support topology and applies severity colors.</summary>
    public static DigitalMockupReviewSession Display(
        DigitalMockupReport report,
        OcctViewer viewer,
        bool includeClearanceViolations = true)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(viewer);
        report.ThrowIfDisposed();
        Dictionary<DigitalMockupPairId, ViewerPresentation[]> created = [];
        try
        {
            foreach (DigitalMockupPairResult pair in report.Pairs
                .Where(pair => pair.IsIssue && (includeClearanceViolations || pair.State != DigitalMockupPairState.ClearanceViolation))
                .OrderByDescending(pair => pair.Severity)
                .ThenBy(pair => pair.Id.ToString(), StringComparer.Ordinal))
            {
                List<Shape> shapes = [];
                if (pair.IssueTopology is not null)
                    shapes.Add(pair.IssueTopology);
                else if (pair.Witnesses.Count > 0)
                {
                    shapes.Add(pair.Witnesses[0].FirstSupport);
                    shapes.Add(pair.Witnesses[0].SecondSupport);
                }
                if (shapes.Count == 0) continue;
                ViewerPresentation[] pairPresentations = shapes.Select(viewer.Display).ToArray();
                ViewerColor color = GetSeverityColor(pair.Severity);
                foreach (ViewerPresentation presentation in pairPresentations)
                {
                    presentation.SetColor(color);
                    presentation.SetDisplayMode(ViewerDisplayMode.Shaded);
                    presentation.SetSelectionKind(null);
                }
                created.Add(pair.Id, pairPresentations);
            }
            return new(viewer, created);
        }
        catch
        {
            foreach (ViewerPresentation presentation in created.Values.SelectMany(value => value)) presentation.Dispose();
            throw;
        }
    }

    /// <summary>Gets stable IDs for all displayed issue pairs.</summary>
    public IReadOnlyCollection<DigitalMockupPairId> IssueIds => _presentations.Keys;

    /// <summary>Returns parent-bound presentations for one stable pair.</summary>
    public IReadOnlyList<ViewerPresentation> GetPresentations(DigitalMockupPairId pairId)
    {
        ThrowIfDisposed();
        return _presentations.TryGetValue(pairId, out ViewerPresentation[]? value) ? value : [];
    }

    /// <summary>Shows only the requested issue pairs; an empty collection hides every issue.</summary>
    public void Isolate(IEnumerable<DigitalMockupPairId> pairIds)
    {
        ArgumentNullException.ThrowIfNull(pairIds);
        ThrowIfDisposed();
        HashSet<DigitalMockupPairId> selected = new(pairIds);
        foreach ((DigitalMockupPairId id, ViewerPresentation[] presentations) in _presentations)
            foreach (ViewerPresentation presentation in presentations)
                if (selected.Contains(id)) presentation.Show(); else presentation.Hide();
        _viewer.Redraw();
    }

    /// <summary>Shows every issue presentation.</summary>
    public void ShowAll()
    {
        ThrowIfDisposed();
        foreach (ViewerPresentation presentation in _presentations.Values.SelectMany(value => value)) presentation.Show();
        _viewer.Redraw();
    }

    /// <summary>Enables whole-issue or subshape selection for one stable pair.</summary>
    public void EnableSelection(DigitalMockupPairId pairId, ShapeKind? shapeKind = null)
    {
        ThrowIfDisposed();
        if (!_presentations.TryGetValue(pairId, out ViewerPresentation[]? presentations))
            throw new KeyNotFoundException($"The review session does not contain pair '{pairId}'.");
        foreach (ViewerPresentation presentation in presentations) presentation.SetSelectionKind(shapeKind);
    }

    /// <summary>Fits all currently visible issue presentations.</summary>
    public void FitAll()
    {
        ThrowIfDisposed();
        _viewer.FitAll();
        _viewer.Redraw();
    }

    /// <summary>Isolates the requested stable pairs and writes a durable keyed screenshot.</summary>
    public string SaveKeyedScreenshot(
        string filePath,
        IEnumerable<DigitalMockupPairId>? pairIds = null,
        bool overwrite = false)
    {
        ThrowIfDisposed();
        if (pairIds is not null) Isolate(pairIds);
        FitAll();
        return _viewer.SaveScreenshot(filePath, overwrite: overwrite);
    }

    private static ViewerColor GetSeverityColor(DigitalMockupSeverity severity) => severity switch
    {
        DigitalMockupSeverity.Critical => new(0.95, 0.08, 0.05),
        DigitalMockupSeverity.Contact => new(1.0, 0.45, 0.05),
        DigitalMockupSeverity.Clearance => new(1.0, 0.85, 0.05),
        DigitalMockupSeverity.Information => new(0.15, 0.55, 1.0),
        _ => new(0.65, 0.65, 0.65)
    };

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>Removes every parent-bound presentation created by this session.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (ViewerPresentation presentation in _presentations.Values.SelectMany(value => value)) presentation.Dispose();
        _presentations.Clear();
        GC.SuppressFinalize(this);
    }
}
