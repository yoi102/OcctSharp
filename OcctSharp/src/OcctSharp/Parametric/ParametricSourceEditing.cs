using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp
{
    public sealed partial class ParametricDocument
    {
        /// <summary>Edits source geometry with exact kernel transform correspondence; dependants require recomputation.</summary>
        public unsafe void TransformSource(Guid feature, ShapeTransform transform)
        {
            var features = ReadFeatures(); var value = Get(features, feature);
            if (value.Definition.Kind != ParametricFeatureKind.SourceShape) throw new ArgumentException("Only source topology may be edited directly.");
            using var source = RequiredShape(value.SourceEntry);
            double[] values = [transform.TranslationX, transform.TranslationY, transform.TranslationZ,
                transform.RotationAxisX, transform.RotationAxisY, transform.RotationAxisZ, transform.RotationAngleRadians];
            AuthoringInfoRaw info; nint raw;
            fixed (double* p = values) NativeError.ThrowIfFailed(ParametricTransformNative.Transform(source.Handle, p, out info, out raw), "parametric_transform");
            using var result = AuthoringBridge.Read(Guid.NewGuid(), raw, info);
            Shape shape = result.RequireShape();
            using var command = BeginCommand("Transform parametric source");
            storage.Record(value.SourceEntry, ParametricEvolutionKind.Modified, [source], [shape]);
            var before = result.History.Where(x => x.Kind == AuthoringHistoryKind.InputSnapshot).ToDictionary(x => x.Source!.Value.TopologyIndex);
            foreach (var item in result.History.Where(x => x.Kind == AuthoringHistoryKind.Modified))
                storage.Record(AddChild(value.HistoryEntry), ParametricEvolutionKind.Modified,
                    [before[item.Source!.Value.TopologyIndex].Shape!], [item.Shape!]);
            UpdateCore(value.Definition, features);
            command.Commit();
        }
    }
}

namespace OcctSharp.Interop
{
    internal static unsafe partial class ParametricTransformNative
    {
        static ParametricTransformNative() => NativeLibraryResolver.EnsureRegistered(typeof(ParametricTransformNative).Assembly);
        [LibraryImport("OcctSharp.Native", EntryPoint = "occtsharp_parametric_transform")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial NativeStatus Transform(ShapeHandle source, double* values, out AuthoringInfoRaw info, out nint output);
    }
}
