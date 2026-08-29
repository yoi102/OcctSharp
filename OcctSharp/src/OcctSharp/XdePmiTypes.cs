namespace OcctSharp;

#pragma warning disable CS1591

public sealed record PmiClassOfTolerance(
    bool IsHole,
    XCAFDimTolObjectsDimensionFormVariance FormVariance,
    XCAFDimTolObjectsDimensionGrade Grade);

public sealed record XdeDimensionDefinition
{
    public XdeDimensionDefinition(XCAFDimTolObjectsDimensionType type, IEnumerable<double> values)
    {
        Type = type;
        Values = Array.AsReadOnly([.. values]);
        if (Values.Count == 0 || Values.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("A dimension requires at least one finite value.", nameof(values));
    }

    public XCAFDimTolObjectsDimensionType Type { get; init; }
    public IReadOnlyList<double> Values { get; init; }
    public XCAFDimTolObjectsDimensionQualifier? Qualifier { get; init; }
    public XCAFDimTolObjectsAngularQualifier? AngularQualifier { get; init; }
    public PmiClassOfTolerance? ClassOfTolerance { get; init; }
    public int LeftDecimalPlaces { get; init; } = 3;
    public int RightDecimalPlaces { get; init; } = 3;
    public IReadOnlyList<XCAFDimTolObjectsDimensionModif> Modifiers { get; init; } = [];
    public GpXyz? Direction { get; init; }
    public GpAx2Value? AnnotationPlane { get; init; }
    public GpPoint? FirstPoint { get; init; }
    public GpPoint? SecondPoint { get; init; }
    public GpPoint? TextPosition { get; init; }
    public string SemanticName { get; init; } = string.Empty;
    public string PresentationName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DescriptionName { get; init; } = string.Empty;
    public Shape? Path { get; init; }
    public Shape? Presentation { get; init; }
}

public sealed record XdeGeomToleranceDefinition
{
    public XCAFDimTolObjectsGeomToleranceType Type { get; init; }
    public XCAFDimTolObjectsGeomToleranceTypeValue TypeOfValue { get; init; }
    public double Value { get; init; }
    public XCAFDimTolObjectsGeomToleranceMatReqModif MaterialRequirement { get; init; }
    public XCAFDimTolObjectsGeomToleranceZoneModif ZoneModifier { get; init; }
    public double ZoneModifierValue { get; init; }
    public IReadOnlyList<XCAFDimTolObjectsGeomToleranceModif> Modifiers { get; init; } = [];
    public double MaximumValueModifier { get; init; }
    public GpAx2Value? Axis { get; init; }
    public GpAx2Value? AnnotationPlane { get; init; }
    public GpPoint? Point { get; init; }
    public GpPoint? TextPosition { get; init; }
    public XCAFDimTolObjectsToleranceZoneAffectedPlane AffectedPlaneType { get; init; }
    public GpPlane? AffectedPlane { get; init; }
    public string SemanticName { get; init; } = string.Empty;
    public string PresentationName { get; init; } = string.Empty;
    public Shape? Presentation { get; init; }
}

public sealed record XdeDatumDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Identification { get; init; } = string.Empty;
    public string SemanticName { get; init; } = string.Empty;
    public string PresentationName { get; init; } = string.Empty;
    public IReadOnlyList<XCAFDimTolObjectsDatumSingleModif> Modifiers { get; init; } = [];
    public XCAFDimTolObjectsDatumModifWithValue? ModifierWithValue { get; init; }
    public double ModifierValue { get; init; }
    public int Position { get; init; }
    public bool IsDatumTarget { get; init; }
    public XCAFDimTolObjectsDatumTargetType TargetType { get; init; }
    public GpAx2Value? TargetAxis { get; init; }
    public double TargetLength { get; init; }
    public double TargetWidth { get; init; }
    public int TargetNumber { get; init; }
    public GpAx2Value? AnnotationPlane { get; init; }
    public GpPoint? Point { get; init; }
    public GpPoint? TextPosition { get; init; }
    public Shape? Target { get; init; }
    public Shape? Presentation { get; init; }
}

public sealed record XdeSavedViewDefinition
{
    public string Name { get; init; } = string.Empty;
    public XCAFViewProjectionType ProjectionType { get; init; }
    public GpPoint ProjectionPoint { get; init; }
    public GpXyz ViewDirection { get; init; } = new(0, 0, -1);
    public GpXyz UpDirection { get; init; } = new(0, 1, 0);
    public double ZoomFactor { get; init; } = 1;
    public double WindowHorizontalSize { get; init; } = 1;
    public double WindowVerticalSize { get; init; } = 1;
    public string ClippingExpression { get; init; } = string.Empty;
    public double? FrontClippingDistance { get; init; }
    public double? BackClippingDistance { get; init; }
    public bool ClipViewVolumeSides { get; init; }
    public IReadOnlyList<ViewerPlaneEquation> ClippingPlanes { get; init; } = [];
}

public abstract class XdePmiItem
{
    private protected XdePmiItem(XdeDocument document, string entry) { Document = document; Entry = entry; }
    internal XdeDocument Document { get; }
    public string Entry { get; }
    internal void EnsureAlive() => Document.ThrowIfDisposed();
}

public sealed class XdeDimension : XdePmiItem
{
    internal XdeDimension(XdeDocument document, string entry) : base(document, entry) { }
    public XdeDimensionSnapshot GetSnapshot() => Document.GetDimensionSnapshot(this);
    public void Update(XdeDimensionDefinition definition) => Document.UpdateDimension(this, definition);
    public void SetReferences(IEnumerable<XdeLabel> first, IEnumerable<XdeLabel>? second = null) => Document.SetDimensionReferences(this, first, second ?? []);
    public void Remove() => Document.RemovePmi(this, 0);
}

public sealed class XdeGeomTolerance : XdePmiItem
{
    internal XdeGeomTolerance(XdeDocument document, string entry) : base(document, entry) { }
    public XdeGeomToleranceSnapshot GetSnapshot() => Document.GetToleranceSnapshot(this);
    public void Update(XdeGeomToleranceDefinition definition) => Document.UpdateTolerance(this, definition);
    public void SetReferences(IEnumerable<XdeLabel> shapes, IEnumerable<XdeDatum>? datums = null) => Document.SetToleranceReferences(this, shapes, datums ?? []);
    public void Remove() => Document.RemovePmi(this, 1);
}

public sealed class XdeDatum : XdePmiItem
{
    internal XdeDatum(XdeDocument document, string entry) : base(document, entry) { }
    public XdeDatumSnapshot GetSnapshot() => Document.GetDatumSnapshot(this);
    public void Update(XdeDatumDefinition definition) => Document.UpdateDatum(this, definition);
    public void SetReferences(IEnumerable<XdeLabel> shapes) => Document.SetDatumReferences(this, shapes);
    public void Remove() => Document.RemovePmi(this, 2);
}

public sealed class XdeSavedView : XdePmiItem
{
    internal XdeSavedView(XdeDocument document, string entry) : base(document, entry) { }
    public XdeSavedViewSnapshot GetSnapshot() => Document.GetSavedViewSnapshot(this);
    public void Update(XdeSavedViewDefinition definition, IEnumerable<XdeLabel> visibleShapes, IEnumerable<XdePmiItem> pmi) =>
        Document.UpdateSavedView(this, definition, visibleShapes, pmi);
    public void ApplyTo(OcctViewer viewer)
    {
        ArgumentNullException.ThrowIfNull(viewer);
        viewer.ApplySavedView(GetSnapshot());
    }
    public void Remove() => Document.RemoveSavedView(this);
}

public sealed record XdeDimensionSnapshot(
    string Entry,
    XdeDimensionDefinition Definition,
    IReadOnlyList<string> FirstShapeEntries,
    IReadOnlyList<string> SecondShapeEntries) : IDisposable
{
    public void Dispose() { Definition.Path?.Dispose(); Definition.Presentation?.Dispose(); }
}

public sealed record XdeGeomToleranceSnapshot(
    string Entry,
    XdeGeomToleranceDefinition Definition,
    IReadOnlyList<string> ShapeEntries,
    IReadOnlyList<string> DatumEntries) : IDisposable
{
    public void Dispose() => Definition.Presentation?.Dispose();
}

public sealed record XdeDatumSnapshot(
    string Entry,
    XdeDatumDefinition Definition,
    IReadOnlyList<string> ShapeEntries,
    IReadOnlyList<string> ToleranceEntries) : IDisposable
{
    public void Dispose() { Definition.Target?.Dispose(); Definition.Presentation?.Dispose(); }
}

public sealed record XdeSavedViewSnapshot(
    string Entry,
    XdeSavedViewDefinition Definition,
    IReadOnlyList<string> VisibleShapeEntries,
    IReadOnlyList<string> PmiEntries);

#pragma warning restore CS1591
