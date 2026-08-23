using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

internal static partial class NativeMethods
{
    private const string LibraryName = "OcctSharp.Native";

    static NativeMethods()
    {
        NativeLibraryResolver.EnsureRegistered();
    }

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_abi_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint GetAbiVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_bridge_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetBridgeVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_occt_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetOcctVersion();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_get_last_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint GetLastError();

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_box")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateBox(
        double sizeX,
        double sizeY,
        double sizeZ,
        out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_get_face_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetFaceCount(ShapeHandle shape, out int faceCount);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_read_step",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReadStep(string filePath, out nint shape);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_write_step",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteStep(ShapeHandle shape, string filePath);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_write_stl",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteStl(
        ShapeHandle shape,
        string filePath,
        double linearDeflection,
        double angularDeflection,
        int binary);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_shape_write_iges",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus WriteIges(ShapeHandle shape, string filePath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_transform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TransformShape(
        ShapeHandle shape,
        double translationX,
        double translationY,
        double translationZ,
        double rotationAxisX,
        double rotationAxisY,
        double rotationAxisZ,
        double rotationAngleRadians,
        out nint transformedShape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTransformIdentity(out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_translation_rotation")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTransform(
        double translationX,
        double translationY,
        double translationZ,
        double rotationAxisX,
        double rotationAxisY,
        double rotationAxisZ,
        double rotationAngleRadians,
        out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneTransform(TransformHandle source, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_inverted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus InvertTransform(TransformHandle source, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_multiplied")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MultiplyTransforms(
        TransformHandle left,
        TransformHandle right,
        out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTransformValue(
        TransformHandle transform,
        int row,
        int column,
        out double value);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseTransform(nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_transform_trsf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TransformShapeWithTransform(
        ShapeHandle shape,
        TransformHandle transform,
        out nint transformedShape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_create_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateLocationIdentity(out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_create_from_trsf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateLocation(TransformHandle transform, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneLocation(LocationHandle source, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_inverted")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus InvertLocation(LocationHandle source, out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_multiplied")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MultiplyLocations(
        LocationHandle left,
        LocationHandle right,
        out nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_is_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsLocationIdentity(LocationHandle location, out int isIdentity);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_to_trsf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocationToTransform(LocationHandle location, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_location_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseLocation(nint location);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_located")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus LocateShape(
        ShapeHandle shape,
        LocationHandle location,
        int moved,
        out nint locatedShape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateVector(double x, double y, double z, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneVector(VectorHandle source, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_components")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVectorComponents(VectorHandle vector, out double x, out double y, out double z);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_magnitude")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVectorMagnitude(VectorHandle vector, out double magnitude);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_dot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetVectorDot(VectorHandle left, VectorHandle right, out double dot);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_crossed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CrossVectors(VectorHandle left, VectorHandle right, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_vec_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseVector(nint vector);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateDirection(double x, double y, double z, out nint direction);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneDirection(DirectionHandle source, out nint direction);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_components")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetDirectionComponents(DirectionHandle direction, out double x, out double y, out double z);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_dot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetDirectionDot(DirectionHandle left, DirectionHandle right, out double dot);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_reversed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReverseDirection(DirectionHandle source, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_dir_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseDirection(nint direction);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateAxis(double ox, double oy, double oz, double dx, double dy, double dz, out nint axis);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneAxis(AxisHandle source, out nint axis);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_components")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetAxisComponents(AxisHandle axis, out double ox, out double oy, out double oz, out double dx, out double dy, out double dz);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_reversed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ReverseAxis(AxisHandle source, out nint result);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ax1_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseAxis(nint axis);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateMatrix(nint values, out nint matrix);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_identity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateIdentityMatrix(out nint matrix);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneMatrix(MatrixHandle source, out nint matrix);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetMatrixValue(MatrixHandle matrix, int row, int column, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_determinant")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetMatrixDeterminant(MatrixHandle matrix, out double determinant);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_mat_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseMatrix(nint matrix);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_translation_vec")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTranslationTransform(VectorHandle vector, out nint transform);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_trsf_create_rotation_axis")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRotationTransform(AxisHandle axis, double angleRadians, out nint transform);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateAscii(nint utf8, int length, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneAscii(AsciiStringHandle source, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetAsciiLength(AsciiStringHandle value, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendAscii(AsciiStringHandle value, nint utf8, int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_to_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CopyAsciiUtf8(AsciiStringHandle value, nint buffer, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_to_extended")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ConvertAsciiToExtended(AsciiStringHandle value, out nint extended);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_ascii_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseAscii(nint value);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_create_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateExtended(nint utf8, int length, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneExtended(ExtendedStringHandle source, out nint value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetExtendedLength(ExtendedStringHandle value, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_utf8_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetExtendedUtf8Length(ExtendedStringHandle value, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_append_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendExtendedUtf8(ExtendedStringHandle value, nint utf8, int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_to_utf8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CopyExtendedUtf8(ExtendedStringHandle value, nint buffer, int capacity, out int written);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetExtendedValue(ExtendedStringHandle value, int index, out ushort character);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_to_ascii")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus ConvertExtendedToAscii(ExtendedStringHandle value, out nint ascii);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_extended_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseExtended(nint value);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRealSequence(nint values, int count, out nint sequence);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneRealSequence(RealSequenceHandle source, out nint sequence);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealSequenceLength(RealSequenceHandle sequence, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealSequenceValue(RealSequenceHandle sequence, int index, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendRealSequence(RealSequenceHandle sequence, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_set_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetRealSequenceValue(RealSequenceHandle sequence, int index, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveRealSequence(RealSequenceHandle sequence, int index);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_sequence_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseRealSequence(nint sequence);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRealArray(nint values, int count, out nint array);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneRealArray(RealArrayHandle source, out nint array);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealArrayLength(RealArrayHandle array, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_lower")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealArrayLower(RealArrayHandle array, out int lower);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealArrayValue(RealArrayHandle array, int index, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_set_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetRealArrayValue(RealArrayHandle array, int index, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_array_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseRealArray(nint array);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateRealVector(nint values, int count, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneRealVector(RealVectorHandle source, out nint vector);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_length")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealVectorLength(RealVectorHandle vector, out int length);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetRealVectorValue(RealVectorHandle vector, int index, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AppendRealVector(RealVectorHandle vector, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_set_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus SetRealVectorValue(RealVectorHandle vector, int index, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_real_vector_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseRealVector(nint vector);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateIntRealMap(nint keys, nint values, int count, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneIntRealMap(IntRealMapHandle source, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_extent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetIntRealMapExtent(IntRealMapHandle map, out int extent);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_is_bound")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsIntRealMapBound(IntRealMapHandle map, int key, out int isBound);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_find")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FindIntRealMap(IntRealMapHandle map, int key, out double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_bind")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus BindIntRealMap(IntRealMapHandle map, int key, double value);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_unbind")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus UnbindIntRealMap(IntRealMapHandle map, int key, out int removed);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_real_map_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseIntRealMap(nint map);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateIntIndexedMap(nint keys, int count, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneIntIndexedMap(IntIndexedMapHandle source, out nint map);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_extent")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetIntIndexedMapExtent(IntIndexedMapHandle map, out int extent);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddIntIndexedMap(IntIndexedMapHandle map, int key, out int index, out int added);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_key")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetIntIndexedMapKey(IntIndexedMapHandle map, int index, out int key);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_find_index")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus FindIntIndexedMapIndex(IntIndexedMapHandle map, int key, out int index);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_remove_last")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus RemoveLastIntIndexedMap(IntIndexedMapHandle map, out int removedKey);
    [LibraryImport(LibraryName, EntryPoint = "occtsharp_int_indexed_map_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseIntIndexedMap(nint map);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_create_compound")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateCompound(out nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_compound_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus AddToCompound(ShapeHandle compound, ShapeHandle child);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_step_merge_xde",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus MergeStepXde(
        nint inputs,
        int inputCount,
        string outputPath);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_shape_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseShape(nint shape);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateTransient(out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_create_null")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateNullTransient(out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_create_derived")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CreateDerivedTransient(out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_clone")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus CloneTransient(
        SharedTransientHandle source,
        out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_try_cast_derived")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus TryCastDerivedTransient(
        SharedTransientHandle source,
        out nint handle);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_is_null")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsTransientNull(
        SharedTransientHandle handle,
        out int isNull);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_get_ref_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTransientRefCount(
        SharedTransientHandle handle,
        out int referenceCount);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_get_type_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus GetTransientTypeName(
        SharedTransientHandle handle,
        out nint typeName);

    [LibraryImport(
        LibraryName,
        EntryPoint = "occtsharp_transient_is_kind",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial NativeStatus IsTransientKind(
        SharedTransientHandle handle,
        string typeName,
        out int isKind);

    [LibraryImport(LibraryName, EntryPoint = "occtsharp_transient_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void ReleaseTransient(nint handle);
}
