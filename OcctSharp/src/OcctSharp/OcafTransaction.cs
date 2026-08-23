namespace OcctSharp;

/// <summary>Controls one OCAF document command; disposal aborts an uncommitted command.</summary>
public sealed class OcafTransaction : IDisposable
{
    private OcafDocument? _document;

    internal OcafTransaction(OcafDocument document) => _document = document;

    /// <summary>Commits the transaction and reports whether an undo delta was created.</summary>
    public bool Commit()
    {
        OcafDocument document = _document
            ?? throw new ObjectDisposedException(nameof(OcafTransaction));
        bool changed = document.CommitTransaction();
        _document = null;
        return changed;
    }

    /// <summary>Aborts the transaction and rolls back its changes.</summary>
    public void Abort()
    {
        OcafDocument document = _document
            ?? throw new ObjectDisposedException(nameof(OcafTransaction));
        document.AbortTransaction();
        _document = null;
    }

    /// <summary>Aborts the transaction if it has not been committed or aborted.</summary>
    public void Dispose()
    {
        OcafDocument? document = Interlocked.Exchange(ref _document, null);
        if (document is null)
        {
            return;
        }

        try
        {
            document.AbortTransaction();
        }
        catch (ObjectDisposedException)
        {
            // Disposing the parent document already aborts its native command.
        }
    }
}
