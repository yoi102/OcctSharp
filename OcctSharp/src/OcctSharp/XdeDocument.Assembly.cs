using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Assembly-authoring, BOM, reference, and occurrence workflows.</summary>
public sealed partial class XdeDocument
{
    /// <summary>Replaces a reusable definition's owning topology inside the current transaction.</summary>
    public void UpdateDefinitionShape(XdeLabel definition, Shape shape)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(shape);
        EnsureOwns(definition);
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(
            NativeMethods.SetXdeDefinitionShape(Handle, definition.Entry, shape.Handle),
            "xde_label_set_shape");
    }

    /// <summary>Changes one occurrence placement inside the current transaction.</summary>
    public XdeLabel RelocateOccurrence(XdeLabel occurrence, TopLocLocation location)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(location);
        EnsureOwns(occurrence);
        ThrowIfDisposed();
        string entry = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.SetXdeOccurrenceLocation(Handle, occurrence.Entry, location.Handle, buffer, capacity, out written),
            "xde_label_set_location");
        return new XdeLabel(this, entry);
    }

    /// <summary>Relinks one occurrence to a new definition and returns its replacement occurrence.</summary>
    public XdeLabel ReplaceOccurrence(XdeLabel occurrence, XdeLabel newDefinition)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(newDefinition);
        EnsureOwns(occurrence);
        EnsureOwns(newDefinition);
        XdeLabel parent = FindParentAssembly(occurrence)
            ?? throw new ArgumentException("The XDE label is not a component occurrence in this document.", nameof(occurrence));
        using TopLocLocation location = occurrence.Location;
        XdeLabel replacement = AddComponent(parent, newDefinition, location);
        CopyOccurrenceMetadata(occurrence, replacement);
        RemoveOccurrence(occurrence);
        return replacement;
    }

    /// <summary>Removes one occurrence while retaining its referred definition.</summary>
    public void RemoveOccurrence(XdeLabel occurrence)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        EnsureOwns(occurrence);
        ThrowIfDisposed();
        NativeError.ThrowIfFailed(NativeMethods.RemoveXdeComponent(Handle, occurrence.Entry), "xde_label_remove_component");
    }

    /// <summary>Removes a reusable definition under an explicit usage policy.</summary>
    public void RemoveDefinition(XdeLabel definition, AssemblyDefinitionRemovalPolicy policy = AssemblyDefinitionRemovalPolicy.RejectIfUsed)
    {
        ArgumentNullException.ThrowIfNull(definition);
        EnsureOwns(definition);
        if (!Enum.IsDefined(policy)) throw new ArgumentOutOfRangeException(nameof(policy));
        XdeLabel[] users = GetUserLabels(definition, recursive: false);
        if (users.Length > 0 && policy == AssemblyDefinitionRemovalPolicy.RejectIfUsed)
            throw new InvalidOperationException("The XDE definition is still used by one or more component occurrences.");
        if (policy == AssemblyDefinitionRemovalPolicy.RemoveOccurrences)
            foreach (XdeLabel occurrence in users) RemoveOccurrence(occurrence);
        NativeError.ThrowIfFailed(
            NativeMethods.RemoveXdeShape(Handle, definition.Entry, 1, out int removed),
            "xde_label_remove_shape");
        if (removed == 0) throw new InvalidOperationException("The XDE definition is not a removable free/top-level shape.");
    }

    /// <summary>Clones a complete part or assembly subtree and its metadata into a new free definition.</summary>
    public XdeLabel CloneSubtree(XdeLabel source, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        EnsureOwns(source);
        if (FindParentAssembly(source) is not null)
            throw new ArgumentException("CloneSubtree expects a reusable definition, not a component occurrence.", nameof(source));
        IReadOnlyList<string> externalReferences = GetExternalReferences(source);
        string entry = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.CloneXdeSubtree(Handle, source.Entry, buffer, capacity, out written),
            "xde_label_clone_subtree");
        XdeLabel clone = new(this, entry);
        if (externalReferences.Count > 0) SetExternalReferences(clone, externalReferences);
        if (name is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            clone.Name = name;
        }
        return clone;
    }

    /// <summary>Moves one occurrence to another assembly while preserving location and direct metadata.</summary>
    public XdeLabel ReparentOccurrence(XdeLabel occurrence, XdeLabel newAssembly)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(newAssembly);
        EnsureOwns(occurrence);
        EnsureOwns(newAssembly);
        if (!newAssembly.IsAssembly) throw new ArgumentException("The new parent is not an assembly.", nameof(newAssembly));
        _ = FindParentAssembly(occurrence)
            ?? throw new ArgumentException("The XDE label is not a component occurrence in this document.", nameof(occurrence));
        XdeLabel definition = occurrence.ReferredShape;
        if (ReferenceEquals(definition.Document, this) && WouldCreateCycle(newAssembly, definition))
            throw new InvalidOperationException("Reparenting the occurrence would create an assembly cycle.");
        using TopLocLocation location = occurrence.Location;
        XdeLabel replacement = AddComponent(newAssembly, definition, location);
        CopyOccurrenceMetadata(occurrence, replacement);
        RemoveOccurrence(occurrence);
        return replacement;
    }

    /// <summary>Resolves a path of occurrence entries to owning world-space topology.</summary>
    public AssemblyOccurrenceResolution ResolveOccurrencePath(XdeLabel rootAssembly, IReadOnlyList<string> path)
    {
        ArgumentNullException.ThrowIfNull(rootAssembly);
        ArgumentNullException.ThrowIfNull(path);
        EnsureOwns(rootAssembly);
        if (!rootAssembly.IsAssembly) throw new ArgumentException("The root label is not an assembly.", nameof(rootAssembly));
        if (path.Count == 0) throw new ArgumentException("An occurrence path cannot be empty.", nameof(path));

        TopLocLocation world = TopLocLocation.Identity;
        try
        {
            XdeLabel current = rootAssembly;
            XdeLabel? occurrence = null;
            XdeLabel? definition = null;
            for (int index = 0; index < path.Count; ++index)
            {
                string requested = path[index];
                ArgumentException.ThrowIfNullOrWhiteSpace(requested);
                occurrence = current.GetComponents().SingleOrDefault(label =>
                    string.Equals(label.Entry, requested, StringComparison.Ordinal));
                if (occurrence is null)
                    throw new KeyNotFoundException($"Occurrence '{requested}' is not a child of assembly '{current.Entry}'.");
                using TopLocLocation local = occurrence.Location;
                TopLocLocation next = world.Multiplied(local);
                world.Dispose();
                world = next;
                definition = occurrence.ReferredShape;
                if (index + 1 < path.Count)
                {
                    if (!definition.IsAssembly)
                        throw new KeyNotFoundException($"Occurrence '{requested}' does not refer to an assembly.");
                    current = definition;
                }
            }
            using Shape definitionShape = definition!.Shape;
            Shape located = definitionShape.Located(world);
            return new AssemblyOccurrenceResolution(occurrence!, definition, path.ToArray(), world, located);
        }
        catch
        {
            world.Dispose();
            throw;
        }
    }

    /// <summary>Returns copied direct or path-qualified reverse usage for a definition.</summary>
    public IReadOnlyList<AssemblyWhereUsedItem> GetWhereUsed(XdeLabel definition, bool recursive = true)
    {
        ArgumentNullException.ThrowIfNull(definition);
        EnsureOwns(definition);
        HashSet<string> nativeUsers = GetUserLabels(definition, recursive)
            .Select(static label => label.Entry).ToHashSet(StringComparer.Ordinal);
        AssemblyStructureSnapshot snapshot = CreateAssemblyStructureSnapshot();
        return snapshot.Nodes
            .Where(node => node.Kind == AssemblyStructureNodeKind.Occurrence
                && string.Equals(node.DefinitionEntry, definition.Entry, StringComparison.Ordinal)
                && (nativeUsers.Count == 0 || nativeUsers.Contains(node.Entry)))
            .Select(node => new AssemblyWhereUsedItem(
                definition.Entry,
                node.Entry,
                node.ParentId is { } parent && parent.StartsWith("def:", StringComparison.Ordinal) ? parent[4..] : null,
                node.Path.ToArray()))
            .OrderBy(static item => string.Join('/', item.Path), StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>Copies the complete or root-scoped product-structure graph and diagnostics.</summary>
    public AssemblyStructureSnapshot CreateAssemblyStructureSnapshot(XdeLabel? root = null)
    {
        if (root is not null) EnsureOwns(root);
        ThrowIfDisposed();
        XdeLabel[] roots = root is null ? GetFreeShapes() : [root];
        Dictionary<string, AssemblyStructureNode> nodes = new(StringComparer.Ordinal);
        List<AssemblyStructureLink> links = [];
        List<AssemblyDiagnostic> diagnostics = [];
        HashSet<string> reachedDefinitions = new(StringComparer.Ordinal);

        foreach (XdeLabel free in roots.OrderBy(static label => label.Entry, StringComparer.Ordinal))
            WalkDefinition(free, [], new HashSet<string>(StringComparer.Ordinal));

        if (root is not null)
        {
            foreach (XdeLabel definition in EnumerateDefinitions())
            {
                if (!reachedDefinitions.Contains(definition.Entry))
                    diagnostics.Add(new(AssemblyDiagnosticCode.OrphanDefinition, definition.Entry, [],
                        "The definition is not reachable from the selected assembly root."));
            }
        }

        return new AssemblyStructureSnapshot(
            nodes.Values.OrderBy(static node => node.Id, StringComparer.Ordinal).ToArray(),
            links.Distinct().OrderBy(static link => link.SourceId, StringComparer.Ordinal)
                .ThenBy(static link => link.TargetId, StringComparer.Ordinal).ToArray(),
            diagnostics.OrderBy(static diagnostic => diagnostic.Code)
                .ThenBy(static diagnostic => diagnostic.Entry, StringComparer.Ordinal).ToArray());

        void WalkDefinition(XdeLabel definition, IReadOnlyList<string> parentPath, HashSet<string> activeDefinitions)
        {
            reachedDefinitions.Add(definition.Entry);
            string definitionId = DefinitionId(definition.Entry);
            AssemblyStructureNodeKind kind = definition.IsAssembly
                ? AssemblyStructureNodeKind.AssemblyDefinition
                : AssemblyStructureNodeKind.PartDefinition;
            nodes.TryAdd(definitionId, new(definitionId, definition.Entry, definition.Entry, null,
                definition.Name, kind, [], 0));
            try { using Shape _ = definition.Shape; }
            catch (Exception exception)
            {
                diagnostics.Add(new(AssemblyDiagnosticCode.MissingShape, definition.Entry, parentPath.ToArray(), exception.Message));
            }
            if (!definition.IsAssembly) return;
            if (!activeDefinitions.Add(definition.Entry))
            {
                diagnostics.Add(new(AssemblyDiagnosticCode.Cycle, definition.Entry, parentPath.ToArray(),
                    "The assembly definition is already active in this occurrence path."));
                return;
            }
            HashSet<string> directEntries = new(StringComparer.Ordinal);
            foreach (XdeLabel occurrence in definition.GetComponents().OrderBy(static item => item.Entry, StringComparer.Ordinal))
            {
                string[] path = [.. parentPath, occurrence.Entry];
                string occurrenceId = OccurrenceId(path);
                if (!directEntries.Add(occurrence.Entry))
                    diagnostics.Add(new(AssemblyDiagnosticCode.DuplicateOccurrence, occurrence.Entry, path,
                        "The assembly exposes the same direct occurrence entry more than once."));
                try
                {
                    XdeLabel referred = occurrence.ReferredShape;
                    nodes[occurrenceId] = new(occurrenceId, occurrence.Entry, referred.Entry, definitionId,
                        DirectOccurrenceName(occurrence) ?? referred.Name,
                        AssemblyStructureNodeKind.Occurrence, path, path.Length);
                    string referredId = DefinitionId(referred.Entry);
                    links.Add(new(definitionId, occurrenceId, AssemblyStructureLinkKind.ContainsOccurrence));
                    links.Add(new(occurrenceId, referredId, AssemblyStructureLinkKind.RefersToDefinition));
                    WalkDefinition(referred, path, activeDefinitions);
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new(AssemblyDiagnosticCode.DanglingReference, occurrence.Entry, path, exception.Message));
                }
            }
            activeDefinitions.Remove(definition.Entry);
        }
    }

    /// <summary>Creates a hierarchy-preserving or grouped BOM for one assembly root.</summary>
    public AssemblyBomReport CreateBom(XdeLabel rootAssembly, bool flattened = false)
    {
        ArgumentNullException.ThrowIfNull(rootAssembly);
        EnsureOwns(rootAssembly);
        if (!rootAssembly.IsAssembly) throw new ArgumentException("The BOM root is not an assembly.", nameof(rootAssembly));
        AssemblyStructureSnapshot snapshot = CreateAssemblyStructureSnapshot(rootAssembly);
        AssemblyStructureNode[] occurrences = snapshot.Nodes
            .Where(static node => node.Kind == AssemblyStructureNodeKind.Occurrence && node.DefinitionEntry is not null)
            .OrderBy(static node => string.Join('/', node.Path), StringComparer.Ordinal).ToArray();
        if (!flattened)
        {
            return new(false, occurrences.Select(node =>
            {
                XdeLabel definition = GetLabel(node.DefinitionEntry!);
                return new AssemblyBomItem(definition.Entry, node.Name ?? definition.Name, 1,
                    node.Path.ToArray(), node.Depth, definition.IsAssembly);
            }).ToArray());
        }
        AssemblyBomItem[] items = occurrences
            .GroupBy(static node => node.DefinitionEntry!, StringComparer.Ordinal)
            .Select(group =>
            {
                XdeLabel definition = GetLabel(group.Key);
                return new AssemblyBomItem(group.Key, definition.Name, group.Count(), [], 0, definition.IsAssembly);
            })
            .OrderBy(static item => item.DefinitionEntry, StringComparer.Ordinal).ToArray();
        return new(true, items);
    }

    /// <summary>Returns deterministic structure diagnostics for one root or the whole document.</summary>
    public IReadOnlyList<AssemblyDiagnostic> ValidateAssemblyStructure(XdeLabel? root = null) =>
        CreateAssemblyStructureSnapshot(root).Diagnostics;

    /// <summary>Replaces copied external-reference path/URI metadata inside the current transaction.</summary>
    public void SetExternalReferences(XdeLabel label, IReadOnlyList<string> references)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(references);
        EnsureOwns(label);
        WithUtf8Pointers(references, pointers => NativeError.ThrowIfFailed(
            NativeMethods.SetXdeExternalReferences(Handle, label.Entry, pointers, references.Count),
            "xde_label_set_external_references"));
    }

    /// <summary>Gets copied external-reference path/URI metadata.</summary>
    public IReadOnlyList<string> GetExternalReferences(XdeLabel label)
    {
        ArgumentNullException.ThrowIfNull(label);
        EnsureOwns(label);
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdeExternalReferenceCount(Handle, label.Entry, out int count),
            "xde_label_external_reference_count");
        string[] values = new string[count];
        for (int index = 0; index < count; ++index)
        {
            int nativeIndex = index + 1;
            NativeError.ThrowIfFailed(
                NativeMethods.GetXdeExternalReferenceLength(Handle, label.Entry, nativeIndex, out int length),
                "xde_label_external_reference_utf8_length");
            values[index] = ReadSized(length, (nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeExternalReference(Handle, label.Entry, nativeIndex, buffer, capacity, out written),
                "xde_label_external_reference_to_utf8");
        }
        return values;
    }

    /// <summary>Creates or replaces an XCAF assembly-item reference on a label.</summary>
    public AssemblyItemReference SetAssemblyItemReference(
        XdeLabel holder,
        IReadOnlyList<string> occurrencePath,
        int? subshapeIndex = null)
    {
        ArgumentNullException.ThrowIfNull(holder);
        ArgumentNullException.ThrowIfNull(occurrencePath);
        EnsureOwns(holder);
        if (occurrencePath.Count == 0) throw new ArgumentException("The assembly-item path cannot be empty.", nameof(occurrencePath));
        foreach (string entry in occurrencePath) ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        if (subshapeIndex is <= 0) throw new ArgumentOutOfRangeException(nameof(subshapeIndex), "A subshape index must be positive.");
        string path = string.Join('/', occurrencePath);
        NativeError.ThrowIfFailed(
            NativeMethods.SetXdeAssemblyItemReference(Handle, holder.Entry, path, subshapeIndex ?? 0),
            "xde_label_set_assembly_item_reference");
        return GetAssemblyItemReference(holder)!;
    }

    /// <summary>Gets a copied XCAF assembly-item reference, or null when absent.</summary>
    public AssemblyItemReference? GetAssemblyItemReference(XdeLabel holder)
    {
        ArgumentNullException.ThrowIfNull(holder);
        EnsureOwns(holder);
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdeAssemblyItemReferenceInfo(Handle, holder.Entry,
                out int hasReference, out int orphan, out int subshapeIndex, out int pathLength),
            "xde_label_assembly_item_reference_info");
        if (hasReference == 0) return null;
        string path = ReadSized(pathLength, (nint buffer, int capacity, out int written) =>
            NativeMethods.GetXdeAssemblyItemReferencePath(Handle, holder.Entry, buffer, capacity, out written),
            "xde_label_assembly_item_reference_path");
        return new(path, subshapeIndex > 0 ? subshapeIndex : null, orphan != 0);
    }

    /// <summary>Creates a specific-higher-usage occurrence chain.</summary>
    public AssemblyShuo CreateShuo(IReadOnlyList<XdeLabel> occurrencePath)
    {
        ArgumentNullException.ThrowIfNull(occurrencePath);
        if (occurrencePath.Count < 2) throw new ArgumentException("A SHUO chain requires at least two occurrences.", nameof(occurrencePath));
        foreach (XdeLabel label in occurrencePath) EnsureOwns(label);
        string[] entries = occurrencePath.Select(static label => label.Entry).ToArray();
        string resultEntry = string.Empty;
        WithUtf8Pointers(entries, pointers => resultEntry = ReadFixed((nint buffer, int capacity, out int written) =>
            NativeMethods.CreateXdeShuo(Handle, pointers, entries.Length, buffer, capacity, out written),
            "xde_shuo_create"));
        return ReadShuo(resultEntry, entries);
    }

    /// <summary>Gets a copied SHUO graph view.</summary>
    public AssemblyShuo GetShuo(string entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        return ReadShuo(entry, []);
    }

    /// <summary>Resolves direct occurrence metadata with definition fallback.</summary>
    public AssemblyEffectiveMetadata GetEffectiveMetadata(XdeLabel occurrence)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        EnsureOwns(occurrence);
        XdeLabel definition = occurrence.ReferredShape;
        IReadOnlyList<string> occurrenceLayers = occurrence.Layers;
        return new(
            DirectOccurrenceName(occurrence) ?? definition.Name,
            occurrence.Color ?? definition.Color,
            occurrenceLayers.Count > 0 ? occurrenceLayers.ToArray() : definition.Layers.ToArray(),
            occurrence.Material ?? definition.Material,
            occurrence.VisualMaterial ?? definition.VisualMaterial);
    }

    /// <summary>Applies direct occurrence metadata overrides inside the current transaction.</summary>
    public void SetOccurrenceMetadata(XdeLabel occurrence, AssemblyEffectiveMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(metadata);
        EnsureOwns(occurrence);
        _ = occurrence.ReferredShape;
        if (metadata.Name is not null) occurrence.Name = metadata.Name;
        if (metadata.Color is XdeColor color) occurrence.Color = color;
        for (int index = 0; index < metadata.Layers.Count; ++index)
        {
            string layer = metadata.Layers[index];
            ArgumentException.ThrowIfNullOrWhiteSpace(layer);
            if (index == 0) occurrence.SetLayer(layer); else occurrence.AddLayer(layer);
        }
        if (metadata.Material is not null) occurrence.Material = metadata.Material;
        if (metadata.VisualMaterial is not null) occurrence.VisualMaterial = metadata.VisualMaterial;
    }

    /// <summary>Computes world-space bounds, mass, centroid, and definition groups.</summary>
    public AssemblyPropertyRollup GetAssemblyPropertyRollup(XdeLabel rootAssembly)
    {
        ArgumentNullException.ThrowIfNull(rootAssembly);
        EnsureOwns(rootAssembly);
        if (!rootAssembly.IsAssembly) throw new ArgumentException("The rollup root is not an assembly.", nameof(rootAssembly));
        IReadOnlyList<XdeOccurrence> occurrences = rootAssembly.GetOccurrences(recursive: true);
        if (occurrences.Count == 0) throw new InvalidOperationException("The assembly contains no occurrences to aggregate.");
        List<PhysicalItem> items = [];
        try
        {
            foreach (XdeOccurrence occurrence in occurrences)
            {
                // Assembly occurrence topology already contains its descendants. Counting it
                // together with leaf occurrences would double-count the same physical parts.
                if (occurrence.IsAssembly) continue;
                using Shape shape = occurrence.GetLocatedShape();
                BoundingBox3d bounds = shape.GetBoundingBox();
                using GPropProperties properties = GPropProperties.FromShape(shape, GPropMode.Volume, onlyClosed: true);
                double density = (occurrence.OccurrenceLabel.Material ?? occurrence.ReferredLabel.Material)?.Density ?? 1.0;
                if (!double.IsFinite(density) || density <= 0) density = 1.0;
                double mass = Math.Abs(properties.Mass) * density;
                items.Add(new(occurrence.ReferredLabel.Entry, occurrence.ReferredLabel.Name,
                    mass, properties.CenterOfMass, bounds));
            }
        }
        finally
        {
            foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose();
        }
        PhysicalAggregate total = AggregatePhysical(items);
        AssemblyPropertyGroup[] groups = items.GroupBy(static item => item.DefinitionEntry, StringComparer.Ordinal)
            .Select(group =>
            {
                PhysicalItem[] values = group.ToArray();
                PhysicalAggregate aggregate = AggregatePhysical(values);
                return new AssemblyPropertyGroup(group.Key, values[0].Name, values.Length,
                    aggregate.Mass, aggregate.Center, aggregate.Bounds);
            })
            .OrderBy(static group => group.DefinitionEntry, StringComparer.Ordinal).ToArray();
        return new(items.Count, total.Mass, total.Center, total.Bounds, groups);
    }

    /// <summary>Displays every path-qualified occurrence and returns viewer-owned presentations.</summary>
    public IReadOnlyList<AssemblyViewerPresentation> DisplayAssembly(XdeLabel rootAssembly, OcctViewer viewer)
    {
        ArgumentNullException.ThrowIfNull(rootAssembly);
        ArgumentNullException.ThrowIfNull(viewer);
        EnsureOwns(rootAssembly);
        IReadOnlyList<XdeOccurrence> occurrences = rootAssembly.GetOccurrences(recursive: true);
        List<AssemblyViewerPresentation> presentations = [];
        try
        {
            foreach (XdeOccurrence occurrence in occurrences)
                presentations.Add(new(occurrence.Path.ToArray(), viewer.Display(occurrence)));
            return presentations;
        }
        catch
        {
            foreach (AssemblyViewerPresentation presentation in presentations) presentation.Dispose();
            throw;
        }
        finally
        {
            foreach (XdeOccurrence occurrence in occurrences) occurrence.Dispose();
        }
    }

    private XdeLabel[] GetUserLabels(XdeLabel definition, bool recursive)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdeUserCount(Handle, definition.Entry, recursive ? 1 : 0, out int count),
            "xde_label_user_count");
        XdeLabel[] users = new XdeLabel[count];
        for (int index = 0; index < count; ++index)
        {
            int nativeIndex = index + 1;
            string entry = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeUserEntry(Handle, definition.Entry, recursive ? 1 : 0,
                    nativeIndex, buffer, capacity, out written), "xde_label_user_entry");
            users[index] = new XdeLabel(this, entry);
        }
        return users;
    }

    private XdeLabel[] EnumerateDefinitions()
    {
        Dictionary<string, XdeLabel> definitions = new(StringComparer.Ordinal);
        Queue<XdeLabel> pending = new(GetFreeShapes());
        while (pending.TryDequeue(out XdeLabel? definition))
        {
            if (!definitions.TryAdd(definition.Entry, definition) || !definition.IsAssembly) continue;
            foreach (XdeLabel component in definition.GetComponents())
            {
                try { pending.Enqueue(component.ReferredShape); }
                catch { }
            }
        }
        return definitions.Values.OrderBy(static label => label.Entry, StringComparer.Ordinal).ToArray();
    }

    private XdeLabel? FindParentAssembly(XdeLabel occurrence)
    {
        foreach (XdeLabel definition in EnumerateDefinitions())
            if (definition.IsAssembly && definition.GetComponents().Any(component =>
                    string.Equals(component.Entry, occurrence.Entry, StringComparison.Ordinal)))
                return definition;
        return null;
    }

    private static bool WouldCreateCycle(XdeLabel assembly, XdeLabel definition)
    {
        if (!definition.IsAssembly) return false;
        if (string.Equals(assembly.Entry, definition.Entry, StringComparison.Ordinal)) return true;
        Queue<XdeLabel> pending = new([definition]);
        HashSet<string> visited = new(StringComparer.Ordinal);
        while (pending.TryDequeue(out XdeLabel? item))
        {
            if (!visited.Add(item.Entry)) continue;
            foreach (XdeLabel occurrence in item.GetComponents())
            {
                XdeLabel child = occurrence.ReferredShape;
                if (string.Equals(child.Entry, assembly.Entry, StringComparison.Ordinal)) return true;
                if (child.IsAssembly) pending.Enqueue(child);
            }
        }
        return false;
    }

    private static string DefinitionId(string entry) => $"def:{entry}";
    private static string OccurrenceId(IReadOnlyList<string> path) => $"occ:{string.Join('/', path)}";

    private static void CopyOccurrenceMetadata(XdeLabel source, XdeLabel destination)
    {
        if (DirectOccurrenceName(source) is { } name) destination.Name = name;
        if (source.Color is XdeColor color) destination.Color = color;
        IReadOnlyList<string> layers = source.Layers;
        for (int index = 0; index < layers.Count; ++index)
        {
            if (index == 0) destination.SetLayer(layers[index]); else destination.AddLayer(layers[index]);
        }
        if (source.Material is { } material) destination.Material = material;
        if (source.VisualMaterial is { } visual) destination.VisualMaterial = visual;
    }

    private AssemblyShuo ReadShuo(string entry, IReadOnlyList<string> occurrencePath)
    {
        string[] upper = ReadShuoLinks(entry, upper: true);
        string[] next = ReadShuoLinks(entry, upper: false);
        return new(entry, occurrencePath.ToArray(), upper, next);
    }

    private static string? DirectOccurrenceName(XdeLabel occurrence)
    {
        string? name = occurrence.Name;
        return name is not null
            && name.StartsWith("=>[", StringComparison.Ordinal)
            && name.EndsWith(']')
                ? null
                : name;
    }

    private string[] ReadShuoLinks(string entry, bool upper)
    {
        NativeError.ThrowIfFailed(
            NativeMethods.GetXdeShuoLinkCount(Handle, entry, upper ? 1 : 0, out int count),
            "xde_shuo_link_count");
        string[] entries = new string[count];
        for (int index = 0; index < count; ++index)
        {
            int nativeIndex = index + 1;
            entries[index] = ReadFixed((nint buffer, int capacity, out int written) =>
                NativeMethods.GetXdeShuoLinkEntry(Handle, entry, upper ? 1 : 0,
                    nativeIndex, buffer, capacity, out written), "xde_shuo_link_entry");
        }
        return entries;
    }

    private static void WithUtf8Pointers(IReadOnlyList<string> values, Action<nint> action)
    {
        nint[] pointers = new nint[values.Count];
        try
        {
            for (int index = 0; index < values.Count; ++index)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(values[index]);
                pointers[index] = Marshal.StringToCoTaskMemUTF8(values[index]);
            }
            unsafe
            {
                fixed (nint* pointer = pointers) action((nint)pointer);
            }
        }
        finally
        {
            foreach (nint pointer in pointers)
                if (pointer != 0) Marshal.FreeCoTaskMem(pointer);
        }
    }

    private static PhysicalAggregate AggregatePhysical(IReadOnlyList<PhysicalItem> items)
    {
        double mass = items.Sum(static item => item.Mass);
        double weight = mass > 1e-12 ? mass : items.Count;
        double x = 0, y = 0, z = 0;
        foreach (PhysicalItem item in items)
        {
            double itemWeight = mass > 1e-12 ? item.Mass : 1.0;
            x += item.Center.X * itemWeight;
            y += item.Center.Y * itemWeight;
            z += item.Center.Z * itemWeight;
        }
        GpPoint minimum = new(
            items.Min(static item => item.Bounds.Minimum.X),
            items.Min(static item => item.Bounds.Minimum.Y),
            items.Min(static item => item.Bounds.Minimum.Z));
        GpPoint maximum = new(
            items.Max(static item => item.Bounds.Maximum.X),
            items.Max(static item => item.Bounds.Maximum.Y),
            items.Max(static item => item.Bounds.Maximum.Z));
        return new(mass, new GpPoint(x / weight, y / weight, z / weight), new BoundingBox3d(minimum, maximum));
    }

    private sealed record PhysicalItem(
        string DefinitionEntry,
        string? Name,
        double Mass,
        GpPoint Center,
        BoundingBox3d Bounds);

    private readonly record struct PhysicalAggregate(double Mass, GpPoint Center, BoundingBox3d Bounds);
}
