using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Independent XCAF general, surface and curve color channels.</summary>
public enum XdeColorChannel
{
    /// <summary>General shape color.</summary>
    General=0,
    /// <summary>Shaded surface color.</summary>
    Surface=1,
    /// <summary>Curve and wire color.</summary>
    Curve=2
}

/// <summary>Copied direct color assignments and native visibility; no inherited topology identity.</summary>
public sealed record XdePresentationColors(XdeColor? General,XdeColor? Surface,XdeColor? Curve,bool IsVisible);

public sealed partial class XdeDocument
{
    /// <summary>Copies direct colors, layers, physical/visual materials and visibility to a fresh label.
    /// Does not copy geometry, name, validation properties, PMI or arbitrary attributes.</summary>
    /// <remarks>The caller establishes the intended label relationship; this operation proves no topology history.
    /// The destination requires an open transaction and must have no presentation assignments.
    /// Abort the transaction on failure. Source and destination may belong to different documents.</remarks>
    public void CopyPresentationMetadata(XdeLabel source,XdeLabel destination)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(destination);
        ThrowIfDisposed();source.Document.ThrowIfDisposed();EnsureOwns(destination);
        NativeError.ThrowIfFailed(NativeMethods.CopyXdePresentationMetadata(Handle,destination.Entry,source.Document.Handle,source.Entry),
            "xde_copy_presentation_metadata");
    }
}

public partial class XdeLabel
{
    /// <summary>Gets or sets direct native visibility. Setting requires an open transaction.</summary>
    public bool IsVisible
    {
        get=>GetPresentationColors().IsVisible;
        set
        {
            Document.ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.SetXdeVisibility(Document.Handle,Entry,value?1:0),"xde_label_set_visibility");
        }
    }

    /// <summary>Gets copied direct assignments without collapsing the three native color channels.</summary>
    public XdePresentationColors GetPresentationColors()
    {
        Document.ThrowIfDisposed();
        XdeColor? Read(XdeColorChannel channel,out bool visible)
        {
            NativeError.ThrowIfFailed(NativeMethods.GetXdeColorChannel(Document.Handle,Entry,(int)channel,out int assigned,out var value,out int shown),
                "xde_label_get_color_channel");
            visible=shown!=0;return assigned==0?null:new(value.Red,value.Green,value.Blue,value.Alpha);
        }
        var general=Read(XdeColorChannel.General,out bool visible);var surface=Read(XdeColorChannel.Surface,out _);var curve=Read(XdeColorChannel.Curve,out _);
        return new(general,surface,curve,visible);
    }

    /// <summary>Sets or removes one direct native color channel; other channels remain independent.</summary>
    public void SetPresentationColor(XdeColorChannel channel,XdeColor? color)
    {
        if(!Enum.IsDefined(channel))throw new ArgumentOutOfRangeException(nameof(channel));color?.Validate();Document.ThrowIfDisposed();
        var value=color.GetValueOrDefault();
        NativeError.ThrowIfFailed(NativeMethods.SetXdeColorChannel(Document.Handle,Entry,(int)channel,color.HasValue?1:0,
            new(value.Red,value.Green,value.Blue,value.Alpha)),"xde_label_set_color_channel");
    }
}
