// Native Runtime/Error implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <stdexcept>
#include <string>

namespace OcctSharp::Native
{
thread_local std::string LastError;

void SetLastError(const char* message)
{
  LastError = message == nullptr ? "Unknown native error." : message;
}
}

using namespace OcctSharp::Native;

void OcctSharp_Internal_SetLastError(const char* message)
{
  SetLastError(message);
}

const char* OCCTSHARP_CALL occtsharp_get_last_error(void)
{
  return LastError.c_str();
}
