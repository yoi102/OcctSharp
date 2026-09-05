#pragma once

// Private native Runtime/Error contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <Standard_Failure.hxx>
#include <exception>
#include <stdexcept>
#include <string>

namespace OcctSharp::Native
{
class OperationFailure final : public std::runtime_error
{
public:
  OperationFailure(const OcctSharp_Status status, const char* message)
    : std::runtime_error(message), Status(status)
  {
  }

  OcctSharp_Status Status;
};

extern thread_local std::string LastError;

void SetLastError(const char* message);

template <typename TAction>
OcctSharp_Status Guard(TAction&& action)
{
  LastError.clear();

  try
  {
    action();
    return OCCTSHARP_STATUS_SUCCESS;
  }
  catch (const Standard_Failure& error)
  {
    SetLastError(error.GetMessageString());
    return OCCTSHARP_STATUS_OCCT_FAILURE;
  }
  catch (const OperationFailure& error)
  {
    SetLastError(error.what());
    return error.Status;
  }
  catch (const std::exception& error)
  {
    SetLastError(error.what());
    return OCCTSHARP_STATUS_STANDARD_EXCEPTION;
  }
  catch (...)
  {
    SetLastError("Unknown C++ exception.");
    return OCCTSHARP_STATUS_UNKNOWN_EXCEPTION;
  }
}
}
