namespace OcctSharp;

public sealed partial class XdeDocument
{
    private DocumentProjections.DocumentProjectionContext? projectionContext;
    private XdeAttributeProjections.XdeAttributeProjectionContext? attributeProjectionContext;

    /// <summary>Gets copied generic label maps and diagnostics over this document's existing transactions.</summary>
    public DocumentProjections.DocumentProjectionContext Projections
    {
        get
        {
            ThrowIfDisposed();
            if (projectionContext is null || projectionContext.IsDisposed)
                projectionContext = DocumentProjections.DocumentProjectionContext.BindLegacy(Handle);
            return projectionContext;
        }
    }

    /// <summary>Gets copied XCAF layer/note tables and parent-bound mutations using the existing document command.</summary>
    public XdeAttributeProjections.XdeAttributeProjectionContext AttributeProjections
    {
        get
        {
            var document = Projections;
            if (attributeProjectionContext is null || attributeProjectionContext.IsDisposed
                || !ReferenceEquals(attributeProjectionContext.Document, document))
                attributeProjectionContext = XdeAttributeProjections.XdeAttributeProjectionContext.Bind(document);
            return attributeProjectionContext;
        }
    }
}
