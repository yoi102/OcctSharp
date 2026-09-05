// Native Runtime/Shape implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <TopoDS_Shape.hxx>
#include <mutex>
#include <utility>

namespace OcctSharp::Native
{
void RegisterShape(OcctSharp_ShapeHandle* shape)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveShapes.insert(shape);
}

bool IsLiveShape(const OcctSharp_ShapeHandle* shape)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveShapes.contains(shape);
}

bool UnregisterShape(const OcctSharp_ShapeHandle* shape)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveShapes.erase(shape) != 0;
}

OcctSharp_ShapeHandle* AllocateShape(TopoDS_Shape shape)
{
  OcctSharp_ShapeHandle* handle = new OcctSharp_ShapeHandle(std::move(shape));
  try
  {
    RegisterShape(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void ValidateShape(const OcctSharp_ShapeHandle* shape)
{
  if (shape == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The shape handle is null.");
  }

  if (!IsLiveShape(shape))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The shape handle is invalid or already released.");
  }
}

void ValidateUsableShape(const OcctSharp_ShapeHandle* shape)
{
  ValidateShape(shape);
  if (shape->Value.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology shape is null.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OcctSharp_Internal_TryGetShape(
  const OcctSharp_ShapeHandle* handle,
  const TopoDS_Shape** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The internal topology output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  if (handle == nullptr)
  {
    SetLastError("The shape handle is null.");
    return OCCTSHARP_STATUS_NULL_HANDLE;
  }

  if (!IsLiveShape(handle))
  {
    SetLastError("The shape handle is invalid or already released.");
    return OCCTSHARP_STATUS_INVALID_HANDLE;
  }

  *out_shape = &handle->Value;
  return OCCTSHARP_STATUS_SUCCESS;
}

OcctSharp_ShapeHandle* OcctSharp_Internal_AllocateShape(TopoDS_Shape shape)
{
  return AllocateShape(std::move(shape));
}

void OCCTSHARP_CALL occtsharp_shape_release(OcctSharp_ShapeHandle* shape)
{
  if (shape != nullptr && UnregisterShape(shape))
  {
    delete shape;
  }
}
