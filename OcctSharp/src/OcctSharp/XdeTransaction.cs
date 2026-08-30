namespace OcctSharp;

/// <summary>Controls one XDE/OCAF command and aborts automatically when uncommitted.</summary>
public sealed class XdeTransaction : IDisposable
{
    private XdeDocument? _document;
    private readonly string? _name;

    internal XdeTransaction(XdeDocument document, string? name)
    {
        _document = document;
        _name = name;
    }

    /// <summary>Commits the transaction and reports whether an undo delta was created.</summary>
    public bool Commit()
    {
        XdeDocument document = _document ?? throw new ObjectDisposedException(nameof(XdeTransaction));
        bool changed = document.CommitTransaction(_name);
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
        try { if (document.HasOpenTransaction) document.AbortTransaction(); }
        catch (ObjectDisposedException) { }
    }
}
