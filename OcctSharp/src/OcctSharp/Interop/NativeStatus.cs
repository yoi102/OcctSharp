namespace OcctSharp.Interop;

internal enum NativeStatus
{
    Success = 0,
    InvalidArgument = 1,
    NullHandle = 2,
    OcctFailure = 3,
    StandardException = 4,
    UnknownException = 5,
    FileIoError = 6,
    TransferFailed = 7,
    InvalidHandle = 8,
    TypeMismatch = 9,
}
