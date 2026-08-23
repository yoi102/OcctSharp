namespace OcctSharp;

/// <summary>Controls one XDE/OCAF command and aborts automatically when uncommitted.</summary>
public sealed class XdeTransaction : IDisposable
{
    private XdeDocument? _document;

    internal XdeTransaction(XdeDocument document) => _document = document;

    /// <summary>Commits the transaction and reports whether an undo delta was created.</summary>
    public bool Commit()
    {
        XdeDocument document = _document ?? throw new ObjectDisposedException(nameof(XdeTransaction));
        bool changed = document.CommitTransaction();
        _document = null;
        return changed;
    }

    /// <summary>Aborts the transaction.</summary>
    public void Abort()
    {
        XdeDocument document = _document ?? throw new ObjectDisposedException(nameof(XdeTransaction));
        document.AbortTransaction();
        _document = null;
    }

    /// <summary>Aborts this transaction if it remains open.</summary>
    public void Dispose()
    {
        XdeDocument? document = Interlocked.Exchange(ref _document, null);
        if (document is null) return;
        try { document.AbortTransaction(); }
        catch (ObjectDisposedException) { }
    }
}
