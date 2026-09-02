using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Owns an XDE document with parent-bound shape labels and copied metadata.</summary>
public sealed partial class XdeDocument : IDisposable
{
    private const int EntryCapacity = 1024;

    private XdeDocument(OcafDocumentHandle handle) => Handle = handle;

    internal OcafDocumentHandle Handle { get; }

    /// <summary>Gets whether an XDE/OCAF command is currently open.</summary>
    public bool HasOpenTransaction
    {
        get
        {
            ThrowIfDisposed();
            NativeError.ThrowIfFailed(NativeMethods.HasOpenOcafCommand(Handle, out int open), "xde_document_has_open_command");
            return open != 0;
        }
    }

    /// <summary>Gets a copied view of the current undo, redo, and dirty state.</summary>
    public DocumentHistoryState HistoryState
    {
        get { ThrowIfDisposed(); return DocumentStateApi.GetHistoryState(Handle); }
    }

    /// <summary>Gets or sets the bounded undo depth; -1 means unlimited and zero disables history.</summary>
    public int UndoLimit
    {
        get => HistoryState.UndoLimit;
        set { ThrowIfDisposed(); DocumentStateApi.SetUndoLimit(Handle, value); }
    }

    /// <summary>Gets whether the document differs from its current savepoint.</summary>
    public bool IsChanged => HistoryState.IsChanged;

    /// <summary>Gets copied undo-history entries without exposing native deltas.</summary>
    public IReadOnlyList<DocumentHistoryEntry> UndoHistory
    {
        get { ThrowIfDisposed(); return DocumentStateApi.GetHistory(Handle, false); }
    }

    /// <summary>Gets copied redo-history entries without exposing native deltas.</summary>
    public IReadOnlyList<DocumentHistoryEntry> RedoHistory
    {
        get { ThrowIfDisposed(); return DocumentStateApi.GetHistory(Handle, true); }
    }

    /// <summary>Creates an empty binary-persistable XDE document.</summary>
    public static XdeDocument Create()
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(NativeMethods.CreateXdeDocument(out nint handle), "xde_document_create");
        return new XdeDocument(new OcafDocumentHandle(handle));
    }

    /// <summary>Opens an OCCT BinXCAF document.</summary>
    public static XdeDocument Open(string filePath) => OpenCore(filePath, NativeMethods.OpenXdeDocument, "xde_document_open");

    /// <summary>Imports a STEP file with STEPCAF/XDE metadata enabled.</summary>
    public static XdeDocument ReadStep(string filePath) => OpenCore(filePath, NativeMethods.ReadStepXdeDocument, "xde_document_read_step");

    /// <summary>Imports a STEP file with explicit common STEPCAF metadata switches.</summary>
    public static XdeDocument ReadStep(string filePath, XdeStepReadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return OpenCore(
            filePath,
            (string path, out nint document) => NativeMethods.ReadStepXdeDocumentWithOptions(
                path,
                options.ReadNames ? 1 : 0,
                options.ReadColors ? 1 : 0,
                options.ReadLayers ? 1 : 0,
                options.ReadValidationProperties ? 1 : 0,
                options.ReadMaterials ? 1 : 0,
                options.ReadGdt ? 1 : 0,
                options.ReadSavedViews ? 1 : 0,
                out document),
            "xde_document_read_step_options");
    }

    /// <summary>Begins an XDE/OCAF transaction.</summary>
    public XdeTransaction BeginTransaction()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.BeginOcafCommand(Handle), "xde_document_begin_command");
        return new XdeTransaction(this, null);
    }

    /// <summary>Begins an explicitly named XDE/OCAF command.</summary>
    public XdeTransaction BeginTransaction(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.BeginOcafCommand(Handle), "xde_document_begin_command");
        return new XdeTransaction(this, name);
    }

    /// <summary>
    /// Imports every free STEPCAF/XDE shape root into this document and returns the
    /// newly cloned parent-bound labels. The caller can compose them into any assembly.
    /// </summary>
    public IReadOnlyList<XdeLabel> ImportStep(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The STEP input file does not exist.", fullPath);
        ThrowIfDisposed();

        HashSet<string> existingEntries = GetFreeShapes()
            .Select(static label => label.Entry)
            .ToHashSet(StringComparer.Ordinal);
        NativeError.ThrowIfFailed(
            NativeMethods.ImportStepIntoXdeDocument(Handle, fullPath, out int importedCount),
            "xde_document_import_step");
        XdeLabel[] imported = GetFreeShapes()
            .Where(label => !existingEntries.Contains(label.Entry))
            .ToArray();
        if (imported.Length != importedCount)
        {
            throw new OcctException(
                NativeStatus.UnknownException.ToString(),
                $"The XDE STEP importer reported {importedCount} roots but exposed {imported.Length} new labels.");
        }
        return imported;
    }

    /// <summary>Adds a top-level shape inside the current transaction.</summary>
    public XdeLabel AddShape(Shape shape, string name)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentNullException.ThrowIfNull(name);
        ThrowIfDisposed();
        using Utf8Buffer utf8 = Utf8Buffer.FromString(name);
        string entry = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.AddXdeShape(Handle, shape.Handle, utf8.Pointer, utf8.Length, buffer, capacity, out written),
            "xde_label_add_shape");
        return new XdeLabel(this, entry);
    }

    /// <summary>
    /// Adds a top-level part and applies its common name, color, layers, and material
    /// metadata as one friendly operation inside the current transaction.
    /// </summary>
    public XdeLabel AddPart(Shape shape, XdePartMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(metadata.Name);
        XdeLabel part = AddShape(shape, metadata.Name);
        if (metadata.Color is XdeColor color) part.Color = color;
        if (metadata.Layers is { Count: > 0 } layers)
        {
            for (int index = 0; index < layers.Count; ++index)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(layers[index]);
                if (index == 0) part.SetLayer(layers[index]);
                else part.AddLayer(layers[index]);
            }
        }
        if (metadata.Material is not null) part.Material = metadata.Material;
        return part;
    }

    /// <summary>Adds an initially empty assembly inside the current transaction.</summary>
    public XdeLabel AddAssembly(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        ThrowIfDisposed();
        using Utf8Buffer utf8 = Utf8Buffer.FromString(name);
        string entry = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.AddXdeAssembly(Handle, utf8.Pointer, utf8.Length, buffer, capacity, out written),
            "xde_label_add_assembly");
        return new XdeLabel(this, entry);
    }

    /// <summary>Adds a transformed occurrence of a part to an assembly.</summary>
    public XdeLabel AddComponent(XdeLabel assembly, XdeLabel part, TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(location);
        EnsureOwns(assembly);
        EnsureOwns(part);
        ThrowIfDisposed();
        string entry = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.AddXdeComponent(Handle, assembly.Entry, part.Entry, location.Handle, buffer, capacity, out written),
            "xde_label_add_component");
        return new XdeLabel(this, entry);
    }

    /// <summary>Returns the current free/top-level shape labels.</summary>
    public XdeLabel[] GetFreeShapes()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeFreeShapeCount(Handle, out int count), "xde_document_free_shape_count");
        XdeLabel[] labels = new XdeLabel[count];
        for (int index = 0; index < count; ++index)
        {
            string entry = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeFreeShapeEntry(Handle, index + 1, buffer, capacity, out written),
                "xde_document_free_shape_entry");
            labels[index] = new XdeLabel(this, entry);
        }
        return labels;
    }

    /// <summary>Resolves an XDE label by stable entry.</summary>
    public XdeLabel GetLabel(string entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        _ = GetName(entry);
        return new XdeLabel(this, entry);
    }

    /// <summary>Saves this document as BinXCAF.</summary>
    public string Save(string filePath) => WriteFile(filePath, NativeMethods.SaveOcafDocument, "xde_document_save");

    /// <summary>Saves this XDE document as BinXCAF or XmlXCAF.</summary>
    public string Save(string filePath, DocumentStorageFormat format)
    {
        ThrowIfDisposed();
        if (format is not (DocumentStorageFormat.BinXcaf or DocumentStorageFormat.XmlXcaf))
            throw new ArgumentOutOfRangeException(nameof(format), "An XDE document requires BinXCAF or XmlXCAF.");
        return DocumentStateApi.Save(Handle, filePath, format);
    }

    /// <summary>Copies the complete XDE label/attribute table, including owning named topology.</summary>
    public DocumentSnapshot CreateSnapshot()
    {
        ThrowIfDisposed();
        return DocumentStateApi.Snapshot(Handle);
    }

    /// <summary>Builds a copied graph including TDF references, tree nodes, and XDE occurrences.</summary>
    public DocumentDependencyGraph CreateDependencyGraph()
    {
        using DocumentSnapshot snapshot = CreateSnapshot();
        List<DocumentDependencyEdge> occurrences = [];
        foreach (DocumentLabelSnapshot label in snapshot.Labels)
        {
            if (!IsAssembly(label.Entry)) continue;
            XdeLabel[] components = GetComponents(label.Entry);
            for (int index = 0; index < components.Length; ++index)
            {
                XdeLabel component = components[index];
                occurrences.Add(new(label.Entry, component.Entry, DocumentDependencyEdgeKind.XdeOccurrence, index));
                occurrences.Add(new(component.Entry, GetReferredLabel(component.Entry).Entry,
                    DocumentDependencyEdgeKind.XdeOccurrence));
            }
        }
        return DocumentStateApi.BuildGraph(snapshot, occurrences);
    }

    /// <summary>Undoes one committed command and reports whether state changed.</summary>
    public bool Undo() { ThrowIfDisposed(); return DocumentStateApi.Undo(Handle); }

    /// <summary>Redoes one undone command and reports whether state changed.</summary>
    public bool Redo() { ThrowIfDisposed(); return DocumentStateApi.Redo(Handle); }

    /// <summary>Clears all undo entries.</summary>
    public void ClearUndoHistory() { ThrowIfDisposed(); DocumentStateApi.ClearUndos(Handle); }

    /// <summary>Clears all redo entries.</summary>
    public void ClearRedoHistory() { ThrowIfDisposed(); DocumentStateApi.ClearRedos(Handle); }

    /// <summary>Marks the current document time as the clean savepoint.</summary>
    public void MarkSaved() { ThrowIfDisposed(); DocumentStateApi.MarkSaved(Handle); }

    /// <summary>Writes this document through STEPCAF with metadata enabled.</summary>
    public string WriteStep(string filePath) => WriteFile(filePath, NativeMethods.WriteStepXdeDocument, "xde_document_write_step");

    /// <summary>Writes this document through STEPCAF with explicit common metadata and representation options.</summary>
    public string WriteStep(string filePath, XdeStepWriteOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(options.ModelType))
            throw new ArgumentOutOfRangeException(nameof(options), "The STEP model type is not defined.");
        if (!Enum.IsDefined(options.Schema))
            throw new ArgumentOutOfRangeException(nameof(options), "The STEP schema is not defined.");
        return WriteFile(
            filePath,
            (document, path) => NativeMethods.WriteStepXdeDocumentWithOptions(
                document,
                path,
                (int)options.ModelType,
                (int)options.Schema,
                options.WriteNames ? 1 : 0,
                options.WriteColors ? 1 : 0,
                options.WriteLayers ? 1 : 0,
                options.WriteValidationProperties ? 1 : 0,
                options.WriteMaterials ? 1 : 0,
                options.WriteGdt ? 1 : 0),
            "xde_document_write_step_options");
    }

    internal Shape GetShape(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeShape(Handle, entry, out nint shape), "xde_label_get_shape");
        return ShapeFactory.FromNativeHandle(shape, "xde_label_get_shape");
    }

    internal string? GetName(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetOcafLabelNameLength(Handle, entry, out int hasName, out int length), "xde_label_name_length");
        return hasName == 0 ? null : ReadSized(length, (nint buffer, int capacity, out int written) =>
            NativeMethods.GetOcafLabelName(Handle, entry, buffer, capacity, out written), "xde_label_name");
    }

    internal void SetName(string entry, string name)
    {
        ThrowIfDisposed();
        using Utf8Buffer utf8 = Utf8Buffer.FromString(name);
        NativeError.ThrowIfFailed(NativeMethods.SetOcafLabelName(Handle, entry, utf8.Pointer, utf8.Length), "xde_label_set_name");
    }

    internal bool IsAssembly(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.IsXdeAssembly(Handle, entry, out int value), "xde_label_is_assembly");
        return value != 0;
    }

    internal int GetComponentCount(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeComponentCount(Handle, entry, out int count), "xde_label_component_count");
        return count;
    }

    internal XdeLabel[] GetComponents(string entry)
    {
        int count = GetComponentCount(entry);
        XdeLabel[] components = new XdeLabel[count];
        for (int index = 0; index < count; ++index)
        {
            int nativeIndex = index + 1;
            string componentEntry = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeComponentEntry(Handle, entry, nativeIndex, buffer, capacity, out written),
                "xde_label_component_entry");
            components[index] = new XdeLabel(this, componentEntry);
        }
        return components;
    }

    internal XdeLabel GetReferredLabel(string entry)
    {
        string referred = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.GetXdeReferredEntry(Handle, entry, buffer, capacity, out written),
            "xde_label_referred_entry");
        return new XdeLabel(this, referred);
    }

    internal TopLocLocation GetLocation(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeLocation(Handle, entry, out nint location), "xde_label_get_location");
        return new TopLocLocation(new LocationHandle(location));
    }

    internal XdeValidationProperties GetValidationProperties(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdeValidationProperties(Handle, entry, out XdeValidationPropertiesRaw raw),
            "xde_label_validation_properties");
        return new XdeValidationProperties(
            raw.HasArea != 0 ? raw.Area : null,
            raw.HasVolume != 0 ? raw.Volume : null,
            raw.HasCentroid != 0
                ? new GpPoint(raw.Centroid.X, raw.Centroid.Y, raw.Centroid.Z)
                : null);
    }

    internal void SetValidationProperties(string entry, XdeValidationProperties properties)
    {
        ThrowIfDisposed();
        GpPoint centroid = properties.Centroid.GetValueOrDefault();
        XdeValidationPropertiesRaw raw = new(
            properties.Area.GetValueOrDefault(),
            properties.Volume.GetValueOrDefault(),
            new XyzRaw(centroid.X, centroid.Y, centroid.Z),
            properties.Area.HasValue ? 1 : 0,
            properties.Volume.HasValue ? 1 : 0,
            properties.Centroid.HasValue ? 1 : 0);
        NativeError.ThrowIfFailed(
            NativeMethods.SetXdeValidationProperties(Handle, entry, in raw),
            "xde_label_set_validation_properties");
    }

    internal XdeValidationProperties UpdateValidationPropertiesFromShape(string entry)
    {
        using Shape shape = GetShape(entry);
        using GPropProperties surface = GPropProperties.FromShape(shape, GPropMode.Surface);
        using GPropProperties volume = GPropProperties.FromShape(shape, GPropMode.Volume, onlyClosed: true);
        double areaValue = Math.Abs(surface.Mass);
        double volumeValue = Math.Abs(volume.Mass);
        GpPoint centroid = volumeValue > 1e-12 ? volume.CenterOfMass : surface.CenterOfMass;
        XdeValidationProperties properties = new(areaValue, volumeValue, centroid);
        SetValidationProperties(entry, properties);
        return properties;
    }

    internal IReadOnlyList<XdeOccurrence> GetOccurrences(string entry, bool recursive)
    {
        ThrowIfDisposed();
        XdeLabel root = new(this, entry);
        if (!root.IsAssembly) return [];

        List<XdeOccurrence> results = [];
        HashSet<string> activeAssemblies = new(StringComparer.Ordinal) { root.Entry };
        try
        {
            using TopLocLocation identity = TopLocLocation.Identity;
            Walk(root, identity, []);
            return results;
        }
        catch
        {
            foreach (XdeOccurrence occurrence in results) occurrence.Dispose();
            throw;
        }

        void Walk(XdeLabel assembly, TopLocLocation parentLocation, IReadOnlyList<string> parentPath)
        {
            foreach (XdeLabel occurrenceLabel in assembly.GetComponents())
            {
                using TopLocLocation localLocation = occurrenceLabel.Location;
                TopLocLocation worldLocation = parentLocation.Multiplied(localLocation);
                XdeLabel referredLabel = occurrenceLabel.ReferredShape;
                string[] path = [.. parentPath, occurrenceLabel.Entry];
                results.Add(new XdeOccurrence(occurrenceLabel, referredLabel, path, worldLocation));

                if (!recursive || !referredLabel.IsAssembly) continue;
                if (!activeAssemblies.Add(referredLabel.Entry))
                    throw new InvalidOperationException("The XDE assembly graph contains a component cycle.");
                try { Walk(referredLabel, worldLocation, path); }
                finally { activeAssemblies.Remove(referredLabel.Entry); }
            }
        }
    }

    internal void SetColor(string entry, XdeColor color)
    {
        color.Validate();
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.SetXdeColor(Handle, entry, new(color.Red, color.Green, color.Blue, color.Alpha)), "xde_label_set_color");
    }

    internal XdeColor? GetColor(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeColor(Handle, entry, out int hasColor, out XdeColorRaw color), "xde_label_get_color");
        return hasColor == 0 ? null : new XdeColor(color.Red, color.Green, color.Blue, color.Alpha);
    }

    internal unsafe IReadOnlyList<XdePresentationStyle> GetPresentationStyles(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdePresentationStyleCount(Handle, entry, out int count),
            "xde_label_presentation_style_count");
        if (count == 0) return [];

        nint[] nativeShapes = new nint[count];
        XdePresentationStyleRaw[] nativeStyles = new XdePresentationStyleRaw[count];
        int written;
        fixed (nint* shapePointer = nativeShapes)
        fixed (XdePresentationStyleRaw* stylePointer = nativeStyles)
        {
            NativeError.ThrowIfFailed(
                NativeMethods.SnapshotXdePresentationStyles(
                    Handle, entry, shapePointer, stylePointer, count, out written),
                "xde_label_presentation_style_snapshot");
        }

        if (written < 0 || written > count)
        {
            foreach (nint nativeShape in nativeShapes)
                if (nativeShape != 0) NativeMethods.ReleaseShape(nativeShape);
            throw new OcctException(
                NativeStatus.UnknownException.ToString(),
                "The native XDE presentation-style snapshot returned an invalid count.");
        }

        XdePresentationStyle[] result = new XdePresentationStyle[written];
        int created = 0;
        try
        {
            for (; created < written; ++created)
            {
                Shape shape = ShapeFactory.FromNativeHandle(
                    nativeShapes[created], "xde_label_presentation_style_snapshot");
                nativeShapes[created] = 0;
                XdePresentationStyleRaw style = nativeStyles[created];
                result[created] = new XdePresentationStyle(
                    shape,
                    style.IsVisible != 0,
                    CopyColor(style.HasSurfaceColor, style.SurfaceColor),
                    CopyColor(style.HasCurveColor, style.CurveColor),
                    CopyColor(style.HasMaterialColor, style.MaterialColor));
            }
            return result;
        }
        catch
        {
            for (int index = 0; index < created; ++index) result[index].Dispose();
            throw;
        }
        finally
        {
            foreach (nint nativeShape in nativeShapes)
                if (nativeShape != 0) NativeMethods.ReleaseShape(nativeShape);
        }

        static XdeColor? CopyColor(int hasColor, XdeColorRaw color) =>
            hasColor == 0 ? null : new XdeColor(color.Red, color.Green, color.Blue, color.Alpha);
    }

    internal void AddLayer(string entry, string layer, bool replace)
    {
        ThrowIfDisposed();
        using Utf8Buffer utf8 = Utf8Buffer.FromString(layer);
        NativeError.ThrowIfFailed(NativeMethods.SetXdeLayer(Handle, entry, utf8.Pointer, utf8.Length, replace ? 1 : 0), "xde_label_set_layer");
    }

    internal string[] GetLayers(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeLayerCount(Handle, entry, out int count), "xde_label_layer_count");
        string[] layers = new string[count];
        for (int index = 0; index < count; ++index)
        {
            NativeError.ThrowIfFailed(NativeMethods.GetXdeLayerNameLength(Handle, entry, index + 1, out int length), "xde_label_layer_length");
            int nativeIndex = index + 1;
            layers[index] = ReadSized(length, (nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeLayerName(Handle, entry, nativeIndex, buffer, capacity, out written), "xde_label_layer_name");
        }
        return layers;
    }

    internal void SetMaterial(string entry, XdeMaterial material)
    {
        ArgumentNullException.ThrowIfNull(material);
        if (!double.IsFinite(material.Density) || material.Density < 0) throw new ArgumentOutOfRangeException(nameof(material));
        ThrowIfDisposed();
        using Utf8Buffer name = Utf8Buffer.FromString(material.Name);
        using Utf8Buffer description = Utf8Buffer.FromString(material.Description);
        using Utf8Buffer densityName = Utf8Buffer.FromString(material.DensityName);
        using Utf8Buffer densityType = Utf8Buffer.FromString(material.DensityType);
        NativeError.ThrowIfFailed(NativeMethods.SetXdeMaterial(
            Handle, entry, name.Pointer, name.Length, description.Pointer, description.Length,
            material.Density, densityName.Pointer, densityName.Length, densityType.Pointer, densityType.Length), "xde_label_set_material");
    }

    internal XdeMaterial? GetMaterial(string entry)
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.GetXdeMaterialInfo(Handle, entry, out int hasMaterial, out double density), "xde_label_material_info");
        if (hasMaterial == 0) return null;
        string Field(int field)
        {
            NativeError.ThrowIfFailed(NativeMethods.GetXdeMaterialFieldLength(Handle, entry, field, out int length), "xde_label_material_field_length");
            return ReadSized(length, (nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeMaterialField(Handle, entry, field, buffer, capacity, out written), "xde_label_material_field");
        }
        return new XdeMaterial(Field(0), Field(1), density, Field(2), Field(3));
    }

    internal bool CommitTransaction(string? name)
    {
        ThrowIfDisposed();
        if (name is not null) return DocumentStateApi.CommitNamedCommand(Handle, name);
        NativeError.ThrowIfFailed(NativeMethods.CommitOcafCommand(Handle, out int changed), "xde_document_commit_command");
        return changed != 0;
    }

    internal DocumentLabelSnapshot SnapshotLabel(string entry) { ThrowIfDisposed(); return DocumentStateApi.SnapshotLabel(Handle, entry); }
    internal string? GetDocumentText(string entry, DocumentAttributeKind kind) { ThrowIfDisposed(); return DocumentStateApi.GetText(Handle, entry, kind); }
    internal void SetDocumentText(string entry, DocumentAttributeKind kind, string value) { ThrowIfDisposed(); DocumentStateApi.SetText(Handle, entry, kind, value); }
    internal int? GetInteger(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetInteger(Handle, entry); }
    internal void SetInteger(string entry, int value) { ThrowIfDisposed(); DocumentStateApi.SetInteger(Handle, entry, value); }
    internal double? GetReal(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetReal(Handle, entry); }
    internal void SetReal(string entry, double value) { ThrowIfDisposed(); DocumentStateApi.SetReal(Handle, entry, value); }
    internal DocumentIntegerArray? GetIntegerArray(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetIntegerArray(Handle, entry); }
    internal void SetIntegerArray(string entry, int lower, IReadOnlyList<int> values) { ThrowIfDisposed(); DocumentStateApi.SetIntegerArray(Handle, entry, lower, values); }
    internal DocumentRealArray? GetRealArray(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetRealArray(Handle, entry); }
    internal void SetRealArray(string entry, int lower, IReadOnlyList<double> values) { ThrowIfDisposed(); DocumentStateApi.SetRealArray(Handle, entry, lower, values); }
    internal string? GetReference(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetReference(Handle, entry); }
    internal void SetReference(string entry, string target) { ThrowIfDisposed(); DocumentStateApi.SetReference(Handle, entry, target); }
    internal IReadOnlyList<string> GetReferenceArray(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetReferenceArray(Handle, entry); }
    internal void SetReferenceArray(string entry, IReadOnlyList<string> targets) { ThrowIfDisposed(); DocumentStateApi.SetReferenceArray(Handle, entry, targets); }
    internal DocumentTreeSnapshot? GetTree(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetTree(Handle, entry); }
    internal void ReparentTree(string entry, string parent) { ThrowIfDisposed(); DocumentStateApi.ReparentTree(Handle, entry, parent); }
    internal void DetachTree(string entry) { ThrowIfDisposed(); DocumentStateApi.DetachTree(Handle, entry); }
    internal Shape? GetNamedShape(string entry) { ThrowIfDisposed(); return DocumentStateApi.GetNamedShape(Handle, entry); }
    internal void SetNamedShape(string entry, Shape shape) { ThrowIfDisposed(); DocumentStateApi.SetNamedShape(Handle, entry, shape); }
    internal void RemoveAttribute(string entry, DocumentAttributeKind kind) { ThrowIfDisposed(); DocumentStateApi.RemoveAttribute(Handle, entry, kind); }

    internal void AbortTransaction()
    {
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.AbortOcafCommand(Handle), "xde_document_abort_command");
    }

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Handle.IsClosed || Handle.IsInvalid, this);

    /// <summary>Closes this XDE document and invalidates its labels.</summary>
    public void Dispose() => Handle.Dispose();

    private delegate NativeStatus DocumentReader(string path, out nint document);
    private delegate NativeStatus FileWriter(OcafDocumentHandle document, string path);
    private delegate NativeStatus Utf8Reader(nint buffer, int capacity, out int written);

    private static XdeDocument OpenCore(string filePath, DocumentReader reader, string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The XDE input file does not exist.", fullPath);
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(reader(fullPath, out nint handle), operation);
        return new XdeDocument(new OcafDocumentHandle(handle));
    }

    private string WriteFile(string filePath, FileWriter writer, string operation)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        NativeError.ThrowIfFailed(writer(Handle, fullPath), operation);
        return fullPath;
    }

    private static string ReadFixed(Utf8Reader reader, string operation) => ReadBuffer(EntryCapacity, reader, operation);
    private static string ReadSized(int length, Utf8Reader reader, string operation) => ReadBuffer(checked(length + 1), reader, operation);

    private static string ReadBuffer(int capacity, Utf8Reader reader, string operation)
    {
        nint buffer = Marshal.AllocHGlobal(capacity);
        try
        {
            NativeError.ThrowIfFailed(reader(buffer, capacity, out int written), operation);
            return Marshal.PtrToStringUTF8(buffer, written) ?? string.Empty;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    private void EnsureOwns(XdeLabel label)
    {
        if (!ReferenceEquals(label.Document, this)) throw new ArgumentException("The XDE label belongs to another document.", nameof(label));
    }
}
