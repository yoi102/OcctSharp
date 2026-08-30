using System.Text;
using OcctSharp.Interop;

namespace OcctSharp;

#pragma warning disable CS1591

/// <summary>Native-local advanced solid-feature, robust Boolean, copied-history, and recovery workflows.</summary>
public static class FeatureModeling
{
    private enum Operation
    {
        Fillet = 0, VariableFillet = 1, Chamfer = 2, TwoDistanceChamfer = 3,
        PlanarFillet = 4, PlanarChamfer = 5, Draft = 6, Boss = 7, Pocket = 8,
        Hole = 9, AddRevolve = 10, CutRevolve = 11, AddPipe = 12, CutPipe = 13,
        Split = 14, Defeature = 15, Cells = 16, Fuse = 17, Cut = 18,
        Common = 19, Section = 20, Preflight = 21
    }

    public static FeatureOperationResult Fillet(
        Shape source, IReadOnlyList<Shape> edges, double radius,
        FeatureModelingOptions? options = null) =>
        Execute(Operation.Fillet, Join(source, edges, 1), 0, 0, [radius], [], options);

    public static FeatureOperationResult VariableFillet(
        Shape source, IReadOnlyList<Shape> edges, double startRadius, double endRadius,
        FeatureModelingOptions? options = null) =>
        Execute(Operation.VariableFillet, Join(source, edges, 1), 0, 0, [startRadius, endRadius], [], options);

    public static FeatureOperationResult Chamfer(
        Shape source, IReadOnlyList<Shape> edges, double distance,
        FeatureModelingOptions? options = null) =>
        Execute(Operation.Chamfer, Join(source, edges, 1), 0, 0, [distance], [], options);

    public static FeatureOperationResult Chamfer(
        Shape source, IReadOnlyList<ChamferSelection> selections,
        double firstDistance, double secondDistance, FeatureModelingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(selections);
        List<Shape> shapes = [source];
        foreach (ChamferSelection selection in selections)
        {
            ArgumentNullException.ThrowIfNull(selection);
            shapes.Add(selection.Edge); shapes.Add(selection.SupportFace);
        }
        return Execute(Operation.TwoDistanceChamfer, shapes, 0, 0, [firstDistance, secondDistance], [], options);
    }

    public static FeatureOperationResult FilletPlanarFace(
        Shape face, IReadOnlyList<Shape> vertices, double radius,
        FeatureModelingOptions? options = null) =>
        Execute(Operation.PlanarFillet, Join(face, vertices, 1), 0, 0, [radius], [], options);

    public static FeatureOperationResult ChamferPlanarFace(
        Shape face, IReadOnlyList<PlanarChamferSelection> selections,
        double firstDistance, double secondDistance, FeatureModelingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(selections);
        List<Shape> shapes = [face];
        foreach (PlanarChamferSelection selection in selections)
        {
            ArgumentNullException.ThrowIfNull(selection);
            shapes.Add(selection.FirstEdge); shapes.Add(selection.SecondEdge);
        }
        return Execute(Operation.PlanarChamfer, shapes, 0, 0, [firstDistance, secondDistance], [], options);
    }

    public static FeatureOperationResult Draft(
        Shape source, IReadOnlyList<Shape> faces, GpXyz pullDirection,
        double angleRadians, GpPlane neutralPlane, FeatureModelingOptions? options = null) =>
        Execute(Operation.Draft, Join(source, faces, 1), 0, 0, [angleRadians],
            [pullDirection, neutralPlane.Origin, neutralPlane.Normal], options);

    public static FeatureOperationResult AddBoss(
        Shape source, Shape profile, GpXyz direction, FeatureModelingOptions? options = null) =>
        Execute(Operation.Boss, [source, profile], 0, 0, [], [direction], options);

    public static FeatureOperationResult CutPocket(
        Shape source, Shape profile, GpXyz direction, FeatureModelingOptions? options = null) =>
        Execute(Operation.Pocket, [source, profile], 0, 0, [], [direction], options);

    public static FeatureOperationResult CutHole(
        Shape source, GpXyz origin, GpXyz direction, double radius, double depth,
        bool throughAll = false, FeatureModelingOptions? options = null) =>
        Execute(Operation.Hole, [source], 0, throughAll ? 1 : 0, [radius, depth], [origin, direction], options);

    public static FeatureOperationResult AddRevolvedFeature(
        Shape source, Shape profile, GpXyz axisOrigin, GpXyz axisDirection,
        double angleRadians, FeatureModelingOptions? options = null) =>
        Execute(Operation.AddRevolve, [source, profile], 0, 0, [angleRadians], [axisOrigin, axisDirection], options);

    public static FeatureOperationResult CutRevolvedFeature(
        Shape source, Shape profile, GpXyz axisOrigin, GpXyz axisDirection,
        double angleRadians, FeatureModelingOptions? options = null) =>
        Execute(Operation.CutRevolve, [source, profile], 0, 0, [angleRadians], [axisOrigin, axisDirection], options);

    public static FeatureOperationResult AddPipeFeature(
        Shape source, Shape spine, Shape profile, FeatureModelingOptions? options = null) =>
        Execute(Operation.AddPipe, [source, spine, profile], 0, 0, [], [], options);

    public static FeatureOperationResult CutPipeFeature(
        Shape source, Shape spine, Shape profile, FeatureModelingOptions? options = null) =>
        Execute(Operation.CutPipe, [source, spine, profile], 0, 0, [], [], options);

    public static FeatureOperationResult Split(
        IReadOnlyList<Shape> arguments, IReadOnlyList<Shape> tools,
        FeatureModelingOptions? options = null)
    {
        Shape[] shapes = ConcatRequired(arguments, tools, "Split requires arguments and tools.");
        return Execute(Operation.Split, shapes, arguments.Count, 0, [], [], options);
    }

    public static FeatureOperationResult Defeature(
        Shape source, IReadOnlyList<Shape> faces, FeatureModelingOptions? options = null) =>
        Execute(Operation.Defeature, Join(source, faces, 1), 0, 0, [], [], options);

    public static FeatureOperationResult SelectBooleanCells(
        IReadOnlyList<Shape> arguments, IReadOnlyList<Shape>? take = null,
        IReadOnlyList<Shape>? avoid = null, int material = 0,
        FeatureModelingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count < 2) throw new ArgumentException("Cell selection requires at least two arguments.", nameof(arguments));
        take ??= []; avoid ??= [];
        Shape[] shapes = [.. arguments, .. take, .. avoid];
        return Execute(Operation.Cells, shapes, arguments.Count, take.Count, [material], [], options);
    }

    public static FeatureOperationResult Boolean(
        FeatureBooleanOperation operation, IReadOnlyList<Shape> arguments,
        IReadOnlyList<Shape> tools, FeatureModelingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(arguments); ArgumentNullException.ThrowIfNull(tools);
        if (arguments.Count == 0) throw new ArgumentException("At least one Boolean argument is required.", nameof(arguments));
        if (operation != FeatureBooleanOperation.Fuse && tools.Count == 0)
            throw new ArgumentException("Cut, common, and section require at least one tool.", nameof(tools));
        Shape[] shapes = [.. arguments, .. tools];
        Operation nativeOperation = operation switch
        {
            FeatureBooleanOperation.Fuse => Operation.Fuse,
            FeatureBooleanOperation.Cut => Operation.Cut,
            FeatureBooleanOperation.Common => Operation.Common,
            FeatureBooleanOperation.Section => Operation.Section,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        return Execute(nativeOperation, shapes, arguments.Count, 0, [], [], options);
    }

    public static FeatureOperationResult Preflight(
        Shape first, Shape? second = null,
        FeatureBooleanOperation operation = FeatureBooleanOperation.Fuse)
    {
        int nativeOperation = operation switch
        {
            FeatureBooleanOperation.Common => 0,
            FeatureBooleanOperation.Fuse => 1,
            FeatureBooleanOperation.Cut => 2,
            FeatureBooleanOperation.Section => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        return Execute(Operation.Preflight, second is null ? [first] : [first, second], nativeOperation, 0, [], [], null);
    }

    private static IReadOnlyList<Shape> Join(Shape first, IReadOnlyList<Shape> remainder, int minimum)
    {
        ArgumentNullException.ThrowIfNull(first); ArgumentNullException.ThrowIfNull(remainder);
        if (remainder.Count < minimum) throw new ArgumentException($"At least {minimum} selection(s) are required.", nameof(remainder));
        return [first, .. remainder];
    }

    private static Shape[] ConcatRequired(IReadOnlyList<Shape> first, IReadOnlyList<Shape> second, string message)
    {
        ArgumentNullException.ThrowIfNull(first); ArgumentNullException.ThrowIfNull(second);
        if (first.Count == 0 || second.Count == 0) throw new ArgumentException(message);
        return [.. first, .. second];
    }

    private static FeatureOptionsRaw ValidateOptions(FeatureModelingOptions? options)
    {
        options ??= new FeatureModelingOptions();
        if (!double.IsFinite(options.FuzzyTolerance) || options.FuzzyTolerance < 0.0)
            throw new ArgumentOutOfRangeException(nameof(options), "Fuzzy tolerance must be finite and non-negative.");
        if (!Enum.IsDefined(options.Glue)) throw new ArgumentOutOfRangeException(nameof(options), "Glue mode is invalid.");
        return new FeatureOptionsRaw
        {
            FuzzyTolerance = options.FuzzyTolerance,
            RunParallel = options.RunParallel ? 1 : 0,
            NonDestructive = options.NonDestructive ? 1 : 0,
            GlueMode = (int)options.Glue,
            RepairInputs = options.RepairInputs ? 1 : 0,
            UnifyResult = options.UnifyResult ? 1 : 0
        };
    }

    private static unsafe FeatureOperationResult Execute(
        Operation operation, IReadOnlyList<Shape> shapes, int primaryCount, int secondaryCount,
        IReadOnlyList<double> parameters, IReadOnlyList<GpXyz> vectors,
        FeatureModelingOptions? options)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        if (shapes.Count == 0) throw new ArgumentException("At least one shape is required.", nameof(shapes));
        nint[] pointers = new nint[shapes.Count]; bool[] references = new bool[shapes.Count];
        double[] parameterArray = [.. parameters];
        XyzRaw[] vectorArray = vectors.Select(value => new XyzRaw(value.X, value.Y, value.Z)).ToArray();
        int acquired = 0;
        try
        {
            for (; acquired < shapes.Count; ++acquired)
            {
                Shape shape = shapes[acquired] ?? throw new ArgumentException("A shape collection contains null.", nameof(shapes));
                ObjectDisposedException.ThrowIf(shape.Handle.IsClosed, shape);
                shape.Handle.DangerousAddRef(ref references[acquired]);
                pointers[acquired] = shape.Handle.DangerousGetHandle();
            }
            fixed (nint* shapePointer = pointers)
            fixed (double* parameterPointer = parameterArray)
            fixed (XyzRaw* vectorPointer = vectorArray)
            {
                NativeError.ThrowIfFailed(NativeMethods.ExecuteFeature(
                    (int)operation, shapePointer, pointers.Length, primaryCount, secondaryCount,
                    parameterArray.Length == 0 ? null : parameterPointer, parameterArray.Length,
                    vectorArray.Length == 0 ? null : vectorPointer, vectorArray.Length,
                    ValidateOptions(options), out nint nativeResult), "feature_execute");
                using FeatureResultHandle result = new(nativeResult);
                return Materialize(result);
            }
        }
        finally
        {
            for (int index = acquired - 1; index >= 0; --index)
                if (references[index]) shapes[index].Handle.DangerousRelease();
        }
    }

    private static unsafe FeatureOperationResult Materialize(FeatureResultHandle result)
    {
        NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultInfo(result, out FeatureResultInfoRaw info), "feature_result_info");
        NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, null, 0, out int messageLength), "feature_result_message_count");
        byte[] messageBytes = new byte[messageLength];
        int written;
        fixed (byte* messagePointer = messageBytes)
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultMessage(result, messagePointer, messageBytes.Length, out written), "feature_result_message");
        string message = Encoding.UTF8.GetString(messageBytes, 0, Math.Max(0, written - 1));

        Shape? shape = null; List<FeatureHistoryItem> history = []; List<int> deleted = [];
        try
        {
            NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultShape(result, out nint nativeShape), "feature_result_shape");
            if (nativeShape != 0) shape = ShapeFactory.FromNativeHandle(nativeShape, "feature_result_shape");
            int historyCount = checked(info.ModifiedCount + info.GeneratedCount);
            for (int index = 0; index < historyCount; ++index)
            {
                NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultHistory(result, index, out FeatureHistoryInfoRaw item, out nint nativeHistory), "feature_result_history");
                history.Add(new FeatureHistoryItem(item.SourceIndex, (FeatureHistoryKind)item.Kind,
                    ShapeFactory.FromNativeHandle(nativeHistory, "feature_result_history")));
            }
            for (int index = 0; index < info.DeletedCount; ++index)
            {
                NativeError.ThrowIfFailed(NativeMethods.GetFeatureResultDeleted(result, index, out int sourceIndex), "feature_result_deleted");
                deleted.Add(sourceIndex);
            }
            return new FeatureOperationResult(shape,
                new FeatureOperationDiagnostics(info.Succeeded != 0, info.Recovered != 0,
                    info.ErrorCount, info.WarningCount, info.FaultyShapeCount,
                    info.ResultIsValid != 0, message), history, deleted);
        }
        catch
        {
            shape?.Dispose(); foreach (FeatureHistoryItem item in history) item.Dispose(); throw;
        }
    }
}

#pragma warning restore CS1591
