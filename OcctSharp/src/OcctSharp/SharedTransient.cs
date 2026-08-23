using OcctSharp.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace OcctSharp;

/// <summary>
/// Experimental managed wrapper for an OCCT <c>Handle(Standard_Transient)</c>.
/// </summary>
public sealed class SharedTransient : IDisposable
{
    private readonly SharedTransientHandle handle;

    private SharedTransient(SharedTransientHandle handle)
    {
        this.handle = handle;
    }

    /// <summary>Creates a non-null reference-counted OCCT transient object.</summary>
    public static SharedTransient Create()
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateTransient(out nint nativeHandle),
            "transient_create");
        return FromNativeHandle(nativeHandle, "transient_create");
    }

    /// <summary>Creates a null OCCT transient handle.</summary>
    public static SharedTransient CreateNull()
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateNullTransient(out nint nativeHandle),
            "transient_create_null");
        return FromNativeHandle(nativeHandle, "transient_create_null");
    }

    /// <summary>Creates the native derived probe used to validate OCCT runtime type identity.</summary>
    public static SharedTransient CreateDerived()
    {
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CreateDerivedTransient(out nint nativeHandle),
            "transient_create_derived");
        return FromNativeHandle(nativeHandle, "transient_create_derived");
    }

    /// <summary>Gets whether the wrapped OCCT handle is null.</summary>
    public bool IsNull
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.IsTransientNull(handle, out int isNull),
                "transient_is_null");
            return isNull != 0;
        }
    }

    /// <summary>Gets the OCCT intrusive reference count, or zero for a null handle.</summary>
    public int ReferenceCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetTransientRefCount(handle, out int referenceCount),
                "transient_get_ref_count");
            return referenceCount;
        }
    }

    /// <summary>Gets the OCCT runtime type name, or an empty string for a null handle.</summary>
    public string TypeName
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetTransientTypeName(handle, out nint typeName),
                "transient_get_type_name");
            return Marshal.PtrToStringUTF8(typeName) ?? string.Empty;
        }
    }

    /// <summary>Checks OCCT runtime type identity including its base classes.</summary>
    public bool IsKind(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.IsTransientKind(handle, typeName, out int isKind),
            "transient_is_kind");
        return isKind != 0;
    }

    /// <summary>
    /// Attempts a checked cast to the experimental derived transient wrapper.
    /// The native bridge verifies OCCT RTTI before retaining the shared object.
    /// </summary>
    public bool TryCastDerived([NotNullWhen(true)] out SharedTransientDerived? result)
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        OcctRuntime.EnsureCompatible();

        NativeStatus status = NativeMethods.TryCastDerivedTransient(
            handle,
            out nint nativeHandle);
        if (status is NativeStatus.TypeMismatch)
        {
            result = null;
            return false;
        }

        NativeError.ThrowIfFailed(status, "transient_try_cast_derived");
        result = SharedTransientDerived.FromNativeHandle(nativeHandle);
        return true;
    }

    /// <summary>Performs a checked cast to the experimental derived wrapper.</summary>
    public SharedTransientDerived CastDerived()
    {
        if (TryCastDerived(out SharedTransientDerived? result))
        {
            return result;
        }

        throw new InvalidCastException(
            $"The transient runtime type '{TypeName}' is not OcctSharp_TransientDerived.");
    }

    /// <summary>Creates another managed wrapper retaining the same OCCT object.</summary>
    public SharedTransient Clone()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.CloneTransient(handle, out nint nativeHandle),
            "transient_clone");
        return FromNativeHandle(nativeHandle, "transient_clone");
    }

    /// <summary>Releases this wrapper's OCCT reference.</summary>
    public void Dispose() => handle.Dispose();

    private static SharedTransient FromNativeHandle(nint nativeHandle, string operation)
    {
        if (nativeHandle == 0)
        {
            throw new OcctException(
                NativeStatus.UnknownException.ToString(),
                $"The native bridge reported success for '{operation}' but returned a null transient handle.");
        }

        return new SharedTransient(new SharedTransientHandle(nativeHandle));
    }
}

/// <summary>
/// Experimental typed wrapper for the native OcctSharp_TransientDerived probe.
/// </summary>
public sealed class SharedTransientDerived : IDisposable
{
    private readonly SharedTransientHandle handle;

    internal SharedTransientDerived(SharedTransientHandle handle)
    {
        this.handle = handle;
    }

    /// <summary>Gets the OCCT runtime type name.</summary>
    public string TypeName
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetTransientTypeName(handle, out nint typeName),
                "transient_derived_get_type_name");
            return Marshal.PtrToStringUTF8(typeName) ?? string.Empty;
        }
    }

    /// <summary>Gets the retained OCCT intrusive reference count.</summary>
    public int ReferenceCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            NativeError.ThrowIfFailed(
                NativeMethods.GetTransientRefCount(handle, out int referenceCount),
                "transient_derived_get_ref_count");
            return referenceCount;
        }
    }

    /// <summary>Checks OCCT runtime type identity including base classes.</summary>
    public bool IsKind(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        NativeError.ThrowIfFailed(
            NativeMethods.IsTransientKind(handle, typeName, out int isKind),
            "transient_derived_is_kind");
        return isKind != 0;
    }

    /// <summary>Creates another typed wrapper retaining the same OCCT object.</summary>
    public SharedTransientDerived Clone()
    {
        ObjectDisposedException.ThrowIf(handle.IsClosed, this);
        OcctRuntime.EnsureCompatible();
        NativeError.ThrowIfFailed(
            NativeMethods.TryCastDerivedTransient(handle, out nint nativeHandle),
            "transient_derived_clone");
        return FromNativeHandle(nativeHandle);
    }

    /// <summary>Releases this wrapper's retained OCCT reference.</summary>
    public void Dispose() => handle.Dispose();

    internal static SharedTransientDerived FromNativeHandle(nint nativeHandle)
    {
        if (nativeHandle == 0)
        {
            throw new OcctException(
                NativeStatus.UnknownException.ToString(),
                "The native bridge returned a null derived transient handle.");
        }

        return new SharedTransientDerived(new SharedTransientHandle(nativeHandle));
    }
}
