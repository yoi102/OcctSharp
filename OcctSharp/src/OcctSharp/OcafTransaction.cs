namespace OcctSharp;

/// <summary>Controls one OCAF document command; disposal aborts an uncommitted command.</summary>
public sealed class OcafTransaction : IDisposable
{
    private OcafDocument? _document;
    private readonly string? _name;

    internal OcafTransaction(OcafDocument document, string? name)
    {
        _document = document;
        _name = name;
    }

    /// <summary>Commits the transaction and reports whether an undo delta was created.</summary>
    public bool Commit()
    {
        OcafDocument document = _document
            ?? throw new ObjectDisposedException(nameof(OcafTransaction));
        bool changed = document.CommitTransaction(_name);
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
            if (document.HasOpenTransaction) document.AbortTransaction();
        }
        catch (ObjectDisposedException)
        {
            // Disposing the parent document already aborts its native command.
        }
    }
}
