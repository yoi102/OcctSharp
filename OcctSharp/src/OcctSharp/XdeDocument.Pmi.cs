using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

public sealed partial class XdeDocument
{
    /// <summary>Enumerates parent-bound semantic dimensions in document order.</summary>
    public XdeDimension[] GetDimensions() => GetPmiEntries(0).Select(entry => new XdeDimension(this, entry)).ToArray();
    /// <summary>Enumerates parent-bound geometric tolerances in document order.</summary>
    public XdeGeomTolerance[] GetGeometricTolerances() => GetPmiEntries(1).Select(entry => new XdeGeomTolerance(this, entry)).ToArray();
    /// <summary>Enumerates parent-bound datums and datum targets in document order.</summary>
    public XdeDatum[] GetDatums() => GetPmiEntries(2).Select(entry => new XdeDatum(this, entry)).ToArray();
    /// <summary>Enumerates parent-bound saved model views in document order.</summary>
    public XdeSavedView[] GetSavedViews() => GetPmiEntries(3).Select(entry => new XdeSavedView(this, entry)).ToArray();

    /// <summary>Creates one transaction-bound saved model view and its copied reference graph.</summary>
    public unsafe XdeSavedView CreateSavedView(
        XdeSavedViewDefinition definition,
        IEnumerable<XdeLabel> visibleShapes,
        IEnumerable<XdePmiItem>? pmi = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        string shapeEntries = JoinEntries(visibleShapes);
        string pmiEntries = JoinPmiEntries(pmi ?? []);
        SavedViewRaw raw = ToRaw(definition);
        PlaneEquationRaw[] planes = definition.ClippingPlanes
            .Select(value => new PlaneEquationRaw(value.A, value.B, value.C, value.D, 0)).ToArray();
        string entry;
        fixed (PlaneEquationRaw* planePointer = planes)
        {
            nint planeAddress = (nint)planePointer;
            entry = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.CreateXdeSavedView(
                    Handle, in raw, definition.Name, definition.ClippingExpression,
                    shapeEntries, pmiEntries, (PlaneEquationRaw*)planeAddress, planes.Length,
                    buffer, capacity, out written), "xde_saved_view_create");
        }
        return new XdeSavedView(this, entry);
    }

    /// <summary>Creates one transaction-bound semantic dimension.</summary>
    public unsafe XdeDimension CreateDimension(
        XdeDimensionDefinition definition,
        IEnumerable<XdeLabel>? firstReferences = null,
        IEnumerable<XdeLabel>? secondReferences = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ThrowIfDisposed();
        PmiDimensionRaw raw = ToRaw(definition);
        double[] values = [.. definition.Values];
        int[] modifiers = definition.Modifiers.Select(value => (int)value).ToArray();
        string entry;
        fixed (double* valuePointer = values)
        fixed (int* modifierPointer = modifiers)
        {
            nint valueAddress = (nint)valuePointer;
            nint modifierAddress = (nint)modifierPointer;
            entry = ReadFixed((nint buffer, int capacity, out int written) =>
            {
                NativeStatus status = NativeMethods.CreateXdeDimension(
                    Handle, in raw, (double*)valueAddress, values.Length, (int*)modifierAddress, modifiers.Length,
                    definition.SemanticName, definition.PresentationName,
                    definition.Description, definition.DescriptionName, buffer, capacity, out written);
                return status;
            }, "xde_pmi_dimension_create");
        }
        XdeDimension dimension = new(this, entry);
        SetDimensionAuxShapes(dimension, definition);
        if (firstReferences is not null || secondReferences is not null)
            SetDimensionReferences(dimension, firstReferences ?? [], secondReferences ?? []);
        return dimension;
    }

    /// <summary>Creates one transaction-bound semantic geometric tolerance.</summary>
    public unsafe XdeGeomTolerance CreateGeometricTolerance(
        XdeGeomToleranceDefinition definition,
        IEnumerable<XdeLabel>? shapeReferences = null,
        IEnumerable<XdeDatum>? datums = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ThrowIfDisposed();
        PmiToleranceRaw raw = ToRaw(definition);
        int[] modifiers = definition.Modifiers.Select(value => (int)value).ToArray();
        string entry;
        fixed (int* modifierPointer = modifiers)
        {
            nint modifierAddress = (nint)modifierPointer;
            entry = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.CreateXdeTolerance(
                    Handle, in raw, (int*)modifierAddress, modifiers.Length,
                    definition.SemanticName, definition.PresentationName,
                    buffer, capacity, out written), "xde_pmi_tolerance_create");
        }
        XdeGeomTolerance tolerance = new(this, entry);
        SetToleranceAuxShape(tolerance, definition);
        if (shapeReferences is not null || datums is not null)
            SetToleranceReferences(tolerance, shapeReferences ?? [], datums ?? []);
        return tolerance;
    }

    /// <summary>Creates one transaction-bound datum or datum target.</summary>
    public unsafe XdeDatum CreateDatum(
        XdeDatumDefinition definition,
        IEnumerable<XdeLabel>? shapeReferences = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ValidateDatumDefinition(definition);
        ThrowIfDisposed();
        PmiDatumRaw raw = ToRaw(definition);
        int[] modifiers = definition.Modifiers.Select(value => (int)value).ToArray();
        string entry;
        fixed (int* modifierPointer = modifiers)
        {
            nint modifierAddress = (nint)modifierPointer;
            entry = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.CreateXdeDatum(
                    Handle, in raw, (int*)modifierAddress, modifiers.Length,
                    definition.Name, definition.Description, definition.Identification,
                    definition.SemanticName, definition.PresentationName,
                    buffer, capacity, out written), "xde_pmi_datum_create");
        }
        XdeDatum datum = new(this, entry);
        SetDatumAuxShapes(datum, definition);
        if (shapeReferences is not null) SetDatumReferences(datum, shapeReferences);
        return datum;
    }

    internal unsafe void UpdateDimension(XdeDimension item, XdeDimensionDefinition definition)
    {
        EnsureOwns(item);
        ArgumentNullException.ThrowIfNull(definition);
        PmiDimensionRaw raw = ToRaw(definition);
        double[] values = [.. definition.Values];
        int[] modifiers = definition.Modifiers.Select(value => (int)value).ToArray();
        fixed (double* valuePointer = values)
        fixed (int* modifierPointer = modifiers)
            NativeError.ThrowIfFailed(NativeMethods.UpdateXdeDimension(
                Handle, item.Entry, in raw, valuePointer, values.Length, modifierPointer, modifiers.Length,
                definition.SemanticName, definition.PresentationName,
                definition.Description, definition.DescriptionName), "xde_pmi_dimension_update");
        SetDimensionAuxShapes(item, definition);
    }

    internal unsafe void UpdateTolerance(XdeGeomTolerance item, XdeGeomToleranceDefinition definition)
    {
        EnsureOwns(item);
        ArgumentNullException.ThrowIfNull(definition);
        PmiToleranceRaw raw = ToRaw(definition);
        int[] modifiers = definition.Modifiers.Select(value => (int)value).ToArray();
        fixed (int* modifierPointer = modifiers)
            NativeError.ThrowIfFailed(NativeMethods.UpdateXdeTolerance(
                Handle, item.Entry, in raw, modifierPointer, modifiers.Length,
                definition.SemanticName, definition.PresentationName), "xde_pmi_tolerance_update");
        SetToleranceAuxShape(item, definition);
    }

    internal unsafe void UpdateDatum(XdeDatum item, XdeDatumDefinition definition)
    {
        EnsureOwns(item);
        ArgumentNullException.ThrowIfNull(definition);
        ValidateDatumDefinition(definition);
        PmiDatumRaw raw = ToRaw(definition);
        int[] modifiers = definition.Modifiers.Select(value => (int)value).ToArray();
        fixed (int* modifierPointer = modifiers)
            NativeError.ThrowIfFailed(NativeMethods.UpdateXdeDatum(
                Handle, item.Entry, in raw, modifierPointer, modifiers.Length,
                definition.Name, definition.Description, definition.Identification,
                definition.SemanticName, definition.PresentationName), "xde_pmi_datum_update");
        SetDatumAuxShapes(item, definition);
    }

    internal XdeDimensionSnapshot GetDimensionSnapshot(XdeDimension item)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.GetXdeDimension(Handle, item.Entry, out PmiDimensionRaw raw, out int valueCount, out int modifierCount), "xde_pmi_dimension_get");
        double[] values = new double[valueCount];
        for (int index = 0; index < valueCount; ++index)
        {
            NativeError.ThrowIfFailed(NativeMethods.GetXdePmiNumericItem(Handle, 0, item.Entry, 0, index + 1, out double value, out _), "xde_pmi_dimension_value");
            values[index] = value;
        }
        XCAFDimTolObjectsDimensionModif[] modifiers = new XCAFDimTolObjectsDimensionModif[modifierCount];
        for (int index = 0; index < modifierCount; ++index)
        {
            NativeError.ThrowIfFailed(NativeMethods.GetXdePmiNumericItem(Handle, 0, item.Entry, 1, index + 1, out _, out int value), "xde_pmi_dimension_modifier");
            modifiers[index] = (XCAFDimTolObjectsDimensionModif)value;
        }
        XdeDimensionDefinition definition = new((XCAFDimTolObjectsDimensionType)raw.Type, values)
        {
            Qualifier = raw.HasQualifier != 0 ? (XCAFDimTolObjectsDimensionQualifier)raw.Qualifier : null,
            AngularQualifier = raw.HasAngularQualifier != 0 ? (XCAFDimTolObjectsAngularQualifier)raw.AngularQualifier : null,
            ClassOfTolerance = raw.HasClassOfTolerance != 0 ? new(raw.IsHole != 0, (XCAFDimTolObjectsDimensionFormVariance)raw.FormVariance, (XCAFDimTolObjectsDimensionGrade)raw.Grade) : null,
            LeftDecimalPlaces = raw.LeftDecimalPlaces,
            RightDecimalPlaces = raw.RightDecimalPlaces,
            Modifiers = modifiers,
            Direction = raw.HasDirection != 0 ? ToXyz(raw.Direction) : null,
            AnnotationPlane = raw.HasPlane != 0 ? ToAxis(raw.Plane) : null,
            FirstPoint = raw.HasFirstPoint != 0 ? ToPoint(raw.FirstPoint) : null,
            SecondPoint = raw.HasSecondPoint != 0 ? ToPoint(raw.SecondPoint) : null,
            TextPosition = raw.HasTextPoint != 0 ? ToPoint(raw.TextPoint) : null,
            SemanticName = ReadPmiText(0, item.Entry, 0),
            PresentationName = ReadPmiText(0, item.Entry, 1),
            Description = ReadPmiText(0, item.Entry, 2),
            DescriptionName = ReadPmiText(0, item.Entry, 3),
            Path = ReadAuxShape(0, item.Entry, 0),
            Presentation = ReadAuxShape(0, item.Entry, 1)
        };
        return new(item.Entry, definition, GetReferenceEntries(0, item.Entry), GetReferenceEntries(1, item.Entry));
    }

    internal XdeGeomToleranceSnapshot GetToleranceSnapshot(XdeGeomTolerance item)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.GetXdeTolerance(Handle, item.Entry, out PmiToleranceRaw raw, out int modifierCount), "xde_pmi_tolerance_get");
        XCAFDimTolObjectsGeomToleranceModif[] modifiers = ReadEnumItems<XCAFDimTolObjectsGeomToleranceModif>(1, item.Entry, 0, modifierCount);
        XdeGeomToleranceDefinition definition = new()
        {
            Type = (XCAFDimTolObjectsGeomToleranceType)raw.Type,
            TypeOfValue = (XCAFDimTolObjectsGeomToleranceTypeValue)raw.TypeOfValue,
            Value = raw.Value,
            MaterialRequirement = (XCAFDimTolObjectsGeomToleranceMatReqModif)raw.MaterialRequirement,
            ZoneModifier = (XCAFDimTolObjectsGeomToleranceZoneModif)raw.ZoneModifier,
            ZoneModifierValue = raw.ZoneModifierValue,
            MaximumValueModifier = raw.MaximumValueModifier,
            Modifiers = modifiers,
            Axis = raw.HasAxis != 0 ? ToAxis(raw.Axis) : null,
            AnnotationPlane = raw.HasPlane != 0 ? ToAxis(raw.Plane) : null,
            Point = raw.HasPoint != 0 ? ToPoint(raw.Point) : null,
            TextPosition = raw.HasTextPoint != 0 ? ToPoint(raw.TextPoint) : null,
            AffectedPlaneType = (XCAFDimTolObjectsToleranceZoneAffectedPlane)raw.AffectedPlaneType,
            AffectedPlane = raw.AffectedPlaneType != 0 ? ToPlane(raw.AffectedPlane) : null,
            SemanticName = ReadPmiText(1, item.Entry, 0),
            PresentationName = ReadPmiText(1, item.Entry, 1),
            Presentation = ReadAuxShape(1, item.Entry, 1)
        };
        return new(item.Entry, definition, GetReferenceEntries(2, item.Entry), GetReferenceEntries(3, item.Entry));
    }

    internal XdeDatumSnapshot GetDatumSnapshot(XdeDatum item)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.GetXdeDatum(Handle, item.Entry, out PmiDatumRaw raw, out int modifierCount), "xde_pmi_datum_get");
        XdeDatumDefinition definition = new()
        {
            Name = ReadPmiText(2, item.Entry, 0),
            Description = ReadPmiText(2, item.Entry, 1),
            Identification = ReadPmiText(2, item.Entry, 2),
            SemanticName = ReadPmiText(2, item.Entry, 3),
            PresentationName = ReadPmiText(2, item.Entry, 4),
            Modifiers = ReadEnumItems<XCAFDimTolObjectsDatumSingleModif>(2, item.Entry, 0, modifierCount),
            ModifierWithValue = raw.HasModifierWithValue != 0 ? (XCAFDimTolObjectsDatumModifWithValue)raw.ModifierWithValue : null,
            ModifierValue = raw.ModifierValue,
            Position = raw.Position,
            IsDatumTarget = raw.IsDatumTarget != 0,
            TargetType = (XCAFDimTolObjectsDatumTargetType)raw.TargetType,
            TargetAxis = raw.HasTargetAxis != 0 ? ToAxis(raw.TargetAxis) : null,
            TargetLength = raw.TargetLength,
            TargetWidth = raw.TargetWidth,
            TargetNumber = raw.TargetNumber,
            AnnotationPlane = raw.HasPlane != 0 ? ToAxis(raw.Plane) : null,
            Point = raw.HasPoint != 0 ? ToPoint(raw.Point) : null,
            TextPosition = raw.HasTextPoint != 0 ? ToPoint(raw.TextPoint) : null,
            Target = ReadAuxShape(2, item.Entry, 0),
            Presentation = ReadAuxShape(2, item.Entry, 1)
        };
        return new(item.Entry, definition, GetReferenceEntries(4, item.Entry), GetReferenceEntries(5, item.Entry));
    }

    internal void SetDimensionReferences(XdeDimension item, IEnumerable<XdeLabel> first, IEnumerable<XdeLabel> second)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.SetXdePmiReferences(Handle, 0, item.Entry, JoinEntries(first), JoinEntries(second)), "xde_pmi_dimension_references");
    }

    internal void SetToleranceReferences(XdeGeomTolerance item, IEnumerable<XdeLabel> shapes, IEnumerable<XdeDatum> datums)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.SetXdePmiReferences(Handle, 1, item.Entry, JoinEntries(shapes), string.Empty), "xde_pmi_tolerance_references");
        string datumEntries = JoinPmiEntries(datums);
        NativeError.ThrowIfFailed(NativeMethods.SetXdePmiReferences(Handle, 3, item.Entry, datumEntries, string.Empty), "xde_pmi_tolerance_datums");
    }

    internal void SetDatumReferences(XdeDatum item, IEnumerable<XdeLabel> shapes)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.SetXdePmiReferences(Handle, 2, item.Entry, JoinEntries(shapes), string.Empty), "xde_pmi_datum_references");
    }

    internal void RemovePmi(XdePmiItem item, int kind)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.RemoveXdePmi(Handle, kind, item.Entry), "xde_pmi_remove");
    }

    internal unsafe void UpdateSavedView(
        XdeSavedView item,
        XdeSavedViewDefinition definition,
        IEnumerable<XdeLabel> visibleShapes,
        IEnumerable<XdePmiItem> pmi)
    {
        EnsureOwns(item);
        ArgumentNullException.ThrowIfNull(definition);
        string shapeEntries = JoinEntries(visibleShapes);
        string pmiEntries = JoinPmiEntries(pmi);
        SavedViewRaw raw = ToRaw(definition);
        PlaneEquationRaw[] planes = definition.ClippingPlanes
            .Select(value => new PlaneEquationRaw(value.A, value.B, value.C, value.D, 0)).ToArray();
        fixed (PlaneEquationRaw* planePointer = planes)
            NativeError.ThrowIfFailed(NativeMethods.UpdateXdeSavedView(
                Handle, item.Entry, in raw, definition.Name, definition.ClippingExpression,
                shapeEntries, pmiEntries, planePointer, planes.Length), "xde_saved_view_update");
    }

    internal XdeSavedViewSnapshot GetSavedViewSnapshot(XdeSavedView item)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdeSavedView(Handle, item.Entry, out SavedViewRaw raw, out int planeCount),
            "xde_saved_view_get");
        ViewerPlaneEquation[] planes = new ViewerPlaneEquation[planeCount];
        for (int index = 0; index < planeCount; ++index)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.GetXdeSavedViewPlane(Handle, item.Entry, index + 1, out PlaneEquationRaw plane),
                "xde_saved_view_plane");
            planes[index] = new ViewerPlaneEquation(plane.A, plane.B, plane.C, plane.D);
        }
        XdeSavedViewDefinition definition = new()
        {
            Name = ReadPmiText(3, item.Entry, 0),
            ProjectionType = (XCAFViewProjectionType)raw.ProjectionType,
            ProjectionPoint = ToPoint(raw.ProjectionPoint),
            ViewDirection = ToXyz(raw.ViewDirection),
            UpDirection = ToXyz(raw.UpDirection),
            ZoomFactor = raw.ZoomFactor,
            WindowHorizontalSize = raw.WindowHorizontalSize,
            WindowVerticalSize = raw.WindowVerticalSize,
            ClippingExpression = ReadPmiText(3, item.Entry, 1),
            FrontClippingDistance = raw.HasFrontClipping != 0 ? raw.FrontClippingDistance : null,
            BackClippingDistance = raw.HasBackClipping != 0 ? raw.BackClippingDistance : null,
            ClipViewVolumeSides = raw.HasViewVolumeSidesClipping != 0,
            ClippingPlanes = planes
        };
        return new(item.Entry, definition, GetReferenceEntries(6, item.Entry), GetReferenceEntries(7, item.Entry));
    }

    internal void RemoveSavedView(XdeSavedView item)
    {
        EnsureOwns(item);
        NativeError.ThrowIfFailed(NativeMethods.RemoveXdeSavedView(Handle, item.Entry), "xde_saved_view_remove");
    }

    private string[] GetPmiEntries(int kind)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdePmiCount(Handle, kind, out int count), "xde_pmi_count");
        string[] entries = new string[count];
        for (int index = 0; index < count; ++index)
            entries[index] = ReadFixed((nint buffer, int capacity, out int written) => NativeMethods.GetXdePmiEntry(Handle, kind, index + 1, buffer, capacity, out written), "xde_pmi_entry");
        return entries;
    }

    private string[] GetReferenceEntries(int relation, string entry)
    {
        NativeError.ThrowIfFailed(NativeMethods.GetXdePmiReferenceCount(Handle, relation, entry, out int count), "xde_pmi_reference_count");
        string[] entries = new string[count];
        for (int index = 0; index < count; ++index)
            entries[index] = ReadFixed((nint buffer, int capacity, out int written) => NativeMethods.GetXdePmiReferenceEntry(Handle, relation, entry, index + 1, buffer, capacity, out written), "xde_pmi_reference_entry");
        return entries;
    }

    private string ReadPmiText(int kind, string entry, int field)
    {
        NativeError.ThrowIfFailed(NativeMethods.GetXdePmiTextLength(Handle, kind, entry, field, out int length), "xde_pmi_text_length");
        return ReadSized(length, (nint buffer, int capacity, out int written) => NativeMethods.GetXdePmiText(Handle, kind, entry, field, buffer, capacity, out written), "xde_pmi_text");
    }

    private T[] ReadEnumItems<T>(int kind, string entry, int field, int count) where T : struct, Enum
    {
        T[] result = new T[count];
        for (int index = 0; index < count; ++index)
        {
            NativeError.ThrowIfFailed(NativeMethods.GetXdePmiNumericItem(Handle, kind, entry, field, index + 1, out _, out int value), "xde_pmi_enum_item");
            result[index] = (T)Enum.ToObject(typeof(T), value);
        }
        return result;
    }

    private Shape? ReadAuxShape(int kind, string entry, int role)
    {
        NativeError.ThrowIfFailed(NativeMethods.GetXdePmiAuxShape(Handle, kind, entry, role, out int hasShape, out nint shape), "xde_pmi_aux_shape");
        return hasShape == 0 ? null : ShapeFactory.FromNativeHandle(shape, "xde_pmi_aux_shape");
    }

    private void SetDimensionAuxShapes(XdeDimension item, XdeDimensionDefinition definition)
    {
        SetAuxShape(0, item.Entry, 0, definition.Path, string.Empty);
        SetAuxShape(0, item.Entry, 1, definition.Presentation, definition.PresentationName);
    }

    private void SetToleranceAuxShape(XdeGeomTolerance item, XdeGeomToleranceDefinition definition) =>
        SetAuxShape(1, item.Entry, 1, definition.Presentation, definition.PresentationName);

    private void SetDatumAuxShapes(XdeDatum item, XdeDatumDefinition definition)
    {
        SetAuxShape(2, item.Entry, 0, definition.Target, string.Empty);
        SetAuxShape(2, item.Entry, 1, definition.Presentation, definition.PresentationName);
    }

    private static void ValidateDatumDefinition(XdeDatumDefinition definition)
    {
        if (definition.Target is not null
            && (!definition.IsDatumTarget
                || definition.TargetType != XCAFDimTolObjectsDatumTargetType.XCAFDimTolObjects_DatumTargetType_Area))
            throw new ArgumentException("Owning datum target topology is valid only for an Area datum target.", nameof(definition));
    }

    private void SetAuxShape(int kind, string entry, int role, Shape? shape, string name)
    {
        if (shape is null) NativeError.ThrowIfFailed(NativeMethods.ClearXdePmiAuxShape(Handle, kind, entry, role), "xde_pmi_clear_aux_shape");
        else NativeError.ThrowIfFailed(NativeMethods.SetXdePmiAuxShape(Handle, kind, entry, role, shape.Handle, name), "xde_pmi_set_aux_shape");
    }

    private string JoinEntries(IEnumerable<XdeLabel> labels)
    {
        ArgumentNullException.ThrowIfNull(labels);
        return string.Join('\n', labels.Select(label => { EnsureOwns(label); return label.Entry; }));
    }

    private string JoinPmiEntries<T>(IEnumerable<T> items) where T : XdePmiItem
    {
        ArgumentNullException.ThrowIfNull(items);
        return string.Join('\n', items.Select(item => { EnsureOwns(item); return item.Entry; }));
    }

    private void EnsureOwns(XdePmiItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!ReferenceEquals(item.Document, this)) throw new ArgumentException("The PMI item belongs to another document.", nameof(item));
        ThrowIfDisposed();
    }

    private static PmiDimensionRaw ToRaw(XdeDimensionDefinition value) => new(
        (int)value.Type, value.Qualifier.HasValue ? 1 : 0, (int)value.Qualifier.GetValueOrDefault(),
        value.AngularQualifier.HasValue ? 1 : 0, (int)value.AngularQualifier.GetValueOrDefault(),
        value.ClassOfTolerance is null ? 0 : 1, value.ClassOfTolerance?.IsHole == true ? 1 : 0,
        (int)(value.ClassOfTolerance?.FormVariance ?? default), (int)(value.ClassOfTolerance?.Grade ?? default),
        value.LeftDecimalPlaces, value.RightDecimalPlaces,
        value.Direction.HasValue ? 1 : 0, ToRaw(value.Direction.GetValueOrDefault()),
        value.AnnotationPlane.HasValue ? 1 : 0, ToRaw(value.AnnotationPlane.GetValueOrDefault()),
        value.FirstPoint.HasValue ? 1 : 0, ToRaw(value.FirstPoint.GetValueOrDefault()),
        value.SecondPoint.HasValue ? 1 : 0, ToRaw(value.SecondPoint.GetValueOrDefault()),
        value.TextPosition.HasValue ? 1 : 0, ToRaw(value.TextPosition.GetValueOrDefault()));

    private static PmiToleranceRaw ToRaw(XdeGeomToleranceDefinition value) => new(
        (int)value.Type, (int)value.TypeOfValue, value.Value, (int)value.MaterialRequirement,
        (int)value.ZoneModifier, value.ZoneModifierValue, value.MaximumValueModifier,
        value.Axis.HasValue ? 1 : 0, ToRaw(value.Axis.GetValueOrDefault()),
        value.AnnotationPlane.HasValue ? 1 : 0, ToRaw(value.AnnotationPlane.GetValueOrDefault()),
        value.Point.HasValue ? 1 : 0, ToRaw(value.Point.GetValueOrDefault()),
        value.TextPosition.HasValue ? 1 : 0, ToRaw(value.TextPosition.GetValueOrDefault()),
        (int)value.AffectedPlaneType, ToRaw(value.AffectedPlane.GetValueOrDefault()));

    private static PmiDatumRaw ToRaw(XdeDatumDefinition value) => new(
        value.Position, value.IsDatumTarget ? 1 : 0, (int)value.TargetType,
        value.TargetLength, value.TargetWidth, value.TargetNumber,
        value.TargetAxis.HasValue ? 1 : 0, ToRaw(value.TargetAxis.GetValueOrDefault()),
        value.AnnotationPlane.HasValue ? 1 : 0, ToRaw(value.AnnotationPlane.GetValueOrDefault()),
        value.Point.HasValue ? 1 : 0, ToRaw(value.Point.GetValueOrDefault()),
        value.TextPosition.HasValue ? 1 : 0, ToRaw(value.TextPosition.GetValueOrDefault()),
        value.ModifierWithValue.HasValue ? 1 : 0, (int)value.ModifierWithValue.GetValueOrDefault(), value.ModifierValue);

    private static SavedViewRaw ToRaw(XdeSavedViewDefinition value) => new(
        (int)value.ProjectionType, ToRaw(value.ProjectionPoint), ToRaw(value.ViewDirection), ToRaw(value.UpDirection),
        value.ZoomFactor, value.WindowHorizontalSize, value.WindowVerticalSize,
        value.FrontClippingDistance.HasValue ? 1 : 0, value.FrontClippingDistance.GetValueOrDefault(),
        value.BackClippingDistance.HasValue ? 1 : 0, value.BackClippingDistance.GetValueOrDefault(),
        value.ClipViewVolumeSides ? 1 : 0);

    private static XyzRaw ToRaw(GpPoint value) => new(value.X, value.Y, value.Z);
    private static XyzRaw ToRaw(GpXyz value) => new(value.X, value.Y, value.Z);
    private static Ax2Raw ToRaw(GpAx2Value value) => new(ToRaw(value.Origin), ToRaw(value.XDirection), ToRaw(value.YDirection), ToRaw(value.Direction));
    private static PlaneRaw ToRaw(GpPlane value) => new(ToRaw(value.Origin), ToRaw(value.Normal));
    private static GpPoint ToPoint(XyzRaw value) => new(value.X, value.Y, value.Z);
    private static GpXyz ToXyz(XyzRaw value) => new(value.X, value.Y, value.Z);
    private static GpAx2Value ToAxis(Ax2Raw value) => new(ToXyz(value.Origin), ToXyz(value.XDirection), ToXyz(value.YDirection), ToXyz(value.Direction));
    private static GpPlane ToPlane(PlaneRaw value) => new(ToXyz(value.Origin), ToXyz(value.Normal));
}
