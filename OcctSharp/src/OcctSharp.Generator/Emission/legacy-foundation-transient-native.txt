// Native Foundation/Transient implementation. Public contracts and ownership are unchanged.
#include "Foundation/Transient.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <Standard_Handle.hxx>
#include <Standard_Transient.hxx>
#include <mutex>
#include <utility>

namespace OcctSharp::Native
{
void RegisterTransient(OcctSharp_TransientHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveTransients.insert(handle);
}

bool IsLiveTransient(const OcctSharp_TransientHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransients.contains(handle);
}

bool UnregisterTransient(const OcctSharp_TransientHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransients.erase(handle) != 0;
}

OcctSharp_TransientHandle* AllocateTransient(opencascade::handle<Standard_Transient> value)
{
  OcctSharp_TransientHandle* handle = new OcctSharp_TransientHandle(std::move(value));
  try
  {
    RegisterTransient(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void ValidateTransient(const OcctSharp_TransientHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The transient handle is null.");
  }

  if (!IsLiveTransient(handle))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The transient handle is invalid or already released.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create(
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    opencascade::handle<Standard_Transient> value = new Standard_Transient();
    *out_handle = AllocateTransient(std::move(value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_null(
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    *out_handle = AllocateTransient(opencascade::handle<Standard_Transient>());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_derived(
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    opencascade::handle<Standard_Transient> value = new OcctSharp_TransientDerived();
    *out_handle = AllocateTransient(std::move(value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_clone(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    ValidateTransient(source);
    *out_handle = AllocateTransient(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_try_cast_derived(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle)
{
  if (out_handle == nullptr)
  {
    SetLastError("The output transient pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_handle = nullptr;
  return Guard([&]
  {
    ValidateTransient(source);
    if (source->Value.IsNull()
        || !source->Value->IsKind("OcctSharp_TransientDerived"))
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_TYPE_MISMATCH,
        "The transient handle is not an OcctSharp_TransientDerived instance.");
    }

    // Copying the native handle retains the same OCCT object. The dynamic type
    // was checked above; no C++ object pointer or layout crosses the ABI.
    *out_handle = AllocateTransient(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_null(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_is_null)
{
  if (out_is_null == nullptr)
  {
    SetLastError("The output null-state pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateTransient(handle);
    *out_is_null = handle->Value.IsNull() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_ref_count(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_ref_count)
{
  if (out_ref_count == nullptr)
  {
    SetLastError("The output reference-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateTransient(handle);
    *out_ref_count = handle->Value.IsNull() ? 0 : handle->Value->GetRefCount();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_type_name(
  const OcctSharp_TransientHandle* handle,
  const char** out_type_name)
{
  if (out_type_name == nullptr)
  {
    SetLastError("The output transient type-name pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_type_name = nullptr;
  return Guard([&]
  {
    ValidateTransient(handle);
    if (handle->Value.IsNull())
    {
      *out_type_name = "";
      return;
    }

    *out_type_name = handle->Value->DynamicType()->Name();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_kind(
  const OcctSharp_TransientHandle* handle,
  const char* type_name,
  int32_t* out_is_kind)
{
  if (type_name == nullptr || type_name[0] == '\0')
  {
    SetLastError("The transient type name is null or empty.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  if (out_is_kind == nullptr)
  {
    SetLastError("The output transient kind-state pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateTransient(handle);
    *out_is_kind = !handle->Value.IsNull() && handle->Value->IsKind(type_name) ? 1 : 0;
  });
}

void OCCTSHARP_CALL occtsharp_transient_release(OcctSharp_TransientHandle* handle)
{
  if (handle != nullptr && UnregisterTransient(handle))
  {
    delete handle;
  }
}
