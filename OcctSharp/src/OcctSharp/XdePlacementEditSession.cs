namespace OcctSharp;

/// <summary>Previews and transactionally commits one rigid XDE occurrence placement.</summary>
public sealed class XdePlacementEditSession : IDisposable
{
    private readonly XdeDocument _document;
    private readonly ViewerPresentation _presentation;
    private readonly string _transactionName;
    private readonly GpTrsf _originalOccurrenceTransform;
    private readonly GpTrsf _originalPresentationTransform;
    private GpTrsf? _pendingPlacement;
    private bool _completed;

    internal XdePlacementEditSession(
        XdeDocument document, XdeLabel occurrence, ViewerPresentation presentation, string transactionName)
    {
        _document = document;
        Occurrence = occurrence;
        _presentation = presentation;
        _transactionName = transactionName;
        using TopLocLocation location = occurrence.Location;
        _originalOccurrenceTransform = location.ToTransform();
        _originalPresentationTransform = presentation.GetTransform();
    }

    /// <summary>Gets the current occurrence label, updated to the replacement after commit.</summary>
    public XdeLabel Occurrence { get; private set; }
    /// <summary>Gets whether an uncommitted placement has been previewed.</summary>
    public bool HasPreview => _pendingPlacement is not null;
    /// <summary>Gets whether the session was committed or cancelled.</summary>
    public bool IsCompleted => _completed;

    /// <summary>Previews an absolute rigid local occurrence placement without mutating XDE.</summary>
    public void Preview(GpTrsf localPlacement)
    {
        ArgumentNullException.ThrowIfNull(localPlacement);
        ThrowIfCompleted();
        ValidateRigid(localPlacement);
        using GpTrsf inverse = _originalOccurrenceTransform.Inverted();
        using GpTrsf delta = inverse.Multiplied(localPlacement);
        using GpTrsf preview = _originalPresentationTransform.Multiplied(delta);
        _presentation.SetTransform(preview);
        _pendingPlacement?.Dispose();
        _pendingPlacement = localPlacement.Clone();
    }

    /// <summary>Commits the current preview in a named transaction and returns the replacement occurrence label.</summary>
    public XdeLabel Commit()
    {
        ThrowIfCompleted();
        GpTrsf placement = _pendingPlacement
            ?? throw new InvalidOperationException("A placement must be previewed before it can be committed.");
        if (_document.HasOpenTransaction)
            throw new InvalidOperationException("Placement commit requires ownership of a new named XDE transaction.");
        using TopLocLocation location = TopLocLocation.FromTransform(placement);
        using XdeTransaction transaction = _document.BeginTransaction(_transactionName);
        XdeLabel replacement = _document.RelocateOccurrence(Occurrence, location);
        if (!transaction.Commit())
            throw new InvalidOperationException("The placement transaction did not create an undo delta.");
        Occurrence = replacement;
        _presentation.UpdateSourceIdentity(replacement.Entry);
        _completed = true;
        DisposeTransforms();
        return replacement;
    }

    /// <summary>Restores the original viewer presentation transform without mutating XDE.</summary>
    public void Cancel()
    {
        if (_completed) return;
        try
        {
            _presentation.SetTransform(_originalPresentationTransform);
        }
        finally
        {
            _completed = true;
            DisposeTransforms();
        }
    }

    /// <summary>Cancels an unfinished preview and releases copied transforms.</summary>
    public void Dispose()
    {
        if (!_completed) Cancel();
    }

    private static void ValidateRigid(GpTrsf transform)
    {
        const double tolerance = 1e-8;
        double[,] rotation = new double[3, 3];
        for (int row = 0; row < 3; ++row)
        for (int column = 0; column < 3; ++column)
        {
            double value = transform.Value(row + 1, column + 1);
            if (!double.IsFinite(value)) throw new ArgumentException("Placement values must be finite.", nameof(transform));
            rotation[row, column] = value;
        }
        for (int row = 1; row <= 3; ++row)
            if (!double.IsFinite(transform.Value(row, 4)))
                throw new ArgumentException("Placement values must be finite.", nameof(transform));
        for (int column = 0; column < 3; ++column)
        {
            double norm = 0;
            for (int row = 0; row < 3; ++row) norm += rotation[row, column] * rotation[row, column];
            if (Math.Abs(norm - 1) > tolerance)
                throw new ArgumentException("XDE occurrence placement must be rigid; scale is not supported.", nameof(transform));
        }
        for (int first = 0; first < 3; ++first)
        for (int second = first + 1; second < 3; ++second)
        {
            double dot = 0;
            for (int row = 0; row < 3; ++row) dot += rotation[row, first] * rotation[row, second];
            if (Math.Abs(dot) > tolerance)
                throw new ArgumentException("XDE occurrence placement axes must remain orthogonal.", nameof(transform));
        }
        double determinant =
            rotation[0, 0] * (rotation[1, 1] * rotation[2, 2] - rotation[1, 2] * rotation[2, 1])
          - rotation[0, 1] * (rotation[1, 0] * rotation[2, 2] - rotation[1, 2] * rotation[2, 0])
          + rotation[0, 2] * (rotation[1, 0] * rotation[2, 1] - rotation[1, 1] * rotation[2, 0]);
        if (Math.Abs(determinant - 1) > tolerance)
            throw new ArgumentException("XDE occurrence placement cannot mirror geometry.", nameof(transform));
    }

    private void ThrowIfCompleted() => ObjectDisposedException.ThrowIf(_completed, this);

    private void DisposeTransforms()
    {
        _pendingPlacement?.Dispose();
        _pendingPlacement = null;
        _originalOccurrenceTransform.Dispose();
        _originalPresentationTransform.Dispose();
    }
}

public sealed partial class XdeDocument
{
    /// <summary>Begins a reversible viewer preview for one rigid occurrence placement.</summary>
    public XdePlacementEditSession BeginPlacementEdit(
        XdeLabel occurrence, ViewerPresentation presentation,
        string transactionName = "Move assembly occurrence")
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionName);
        EnsureOwns(occurrence);
        ThrowIfDisposed();
        if (FindParentAssembly(occurrence) is null)
            throw new ArgumentException("Placement editing requires an XDE component occurrence.", nameof(occurrence));
        if (presentation.SourceIdentity is ViewerSourceIdentity identity
            && !string.Equals(identity.OccurrenceEntry, occurrence.Entry, StringComparison.Ordinal))
            throw new ArgumentException("The presentation belongs to another XDE occurrence.", nameof(presentation));
        return new XdePlacementEditSession(this, occurrence, presentation, transactionName);
    }
}
