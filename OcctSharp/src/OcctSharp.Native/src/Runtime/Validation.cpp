// Native Runtime/Validation implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <cmath>

namespace OcctSharp::Native
{
void ValidateUtf8Input(const char* utf8, const int32_t length)
{
  if (length < 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "UTF-8 length cannot be negative.");
  if (length > 0 && utf8 == nullptr) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "UTF-8 input is null for a non-empty string.");
}

void ValidateOutputBuffer(char* buffer, const int32_t capacity, const int32_t required)
{
  if (capacity < required) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UTF-8 output buffer is too small.");
  if (required > 0 && buffer == nullptr) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UTF-8 output buffer is null.");
}

void ValidatePath(const char* filePath)
{
  if (filePath == nullptr || filePath[0] == '\0')
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The file path is null or empty.");
  }
}

void ValidateOutputCapacity(const int32_t capacity, const int32_t required, const void* buffer, const char* message)
{
  if (capacity < required || (required > 0 && buffer == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}

void ValidateFinite(double value, const char* name)
{
  if (!std::isfinite(value))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, name);
  }
}

void ValidateArray(const void* values, const int32_t count, const char* message)
{
  if (count < 0 || (count > 0 && values == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
}
