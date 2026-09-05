// Native Geometry/Transforms implementation. Public contracts and ownership are unchanged.
#include "Geometry/Transforms.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepBuilderAPI_Transform.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS_Shape.hxx>
#include <cmath>
#include <gp_Ax1.hxx>
#include <gp_Dir.hxx>
#include <gp_Mat.hxx>
#include <gp_Pnt.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <mutex>
#include <utility>

namespace OcctSharp::Native
{
void RegisterTransform(OcctSharp_TrsfHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveTransforms.insert(handle);
}

bool IsLiveTransform(const OcctSharp_TrsfHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransforms.contains(handle);
}

bool UnregisterTransform(const OcctSharp_TrsfHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveTransforms.erase(handle) != 0;
}

OcctSharp_TrsfHandle* AllocateTransform(gp_Trsf value)
{
  OcctSharp_TrsfHandle* handle = new OcctSharp_TrsfHandle(std::move(value));
  try
  {
    RegisterTransform(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void RegisterLocation(OcctSharp_LocationHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  LiveLocations.insert(handle);
}

bool IsLiveLocation(const OcctSharp_LocationHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveLocations.contains(handle);
}

bool UnregisterLocation(const OcctSharp_LocationHandle* handle)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return LiveLocations.erase(handle) != 0;
}

OcctSharp_LocationHandle* AllocateLocation(TopLoc_Location value)
{
  OcctSharp_LocationHandle* handle = new OcctSharp_LocationHandle(std::move(value));
  try
  {
    RegisterLocation(handle);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}

void ValidateVector(const OcctSharp_VecHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The vector handle is null.");
  if (!IsLiveValue(handle, LiveVectors)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The vector handle is invalid or already released.");
}

void ValidateDirection(const OcctSharp_DirHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The direction handle is null.");
  if (!IsLiveValue(handle, LiveDirections)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The direction handle is invalid or already released.");
}

void ValidateAxis(const OcctSharp_Ax1Handle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The axis handle is null.");
  if (!IsLiveValue(handle, LiveAxes)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The axis handle is invalid or already released.");
}

void ValidateMatrix(const OcctSharp_MatHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The matrix handle is null.");
  if (!IsLiveValue(handle, LiveMatrices)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The matrix handle is invalid or already released.");
}

void ValidateTransformHandle(const OcctSharp_TrsfHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The transform handle is null.");
  }

  if (!IsLiveTransform(handle))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The transform handle is invalid or already released.");
  }
}

void ValidateLocationHandle(const OcctSharp_LocationHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The location handle is null.");
  }

  if (!IsLiveLocation(handle))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The location handle is invalid or already released.");
  }
}

gp_Trsf CreateTranslationRotation(
  const double translationX,
  const double translationY,
  const double translationZ,
  const double axisX,
  const double axisY,
  const double axisZ,
  const double angle)
{
  ValidateFinite(translationX, "Translation X must be finite.");
  ValidateFinite(translationY, "Translation Y must be finite.");
  ValidateFinite(translationZ, "Translation Z must be finite.");
  ValidateFinite(axisX, "Rotation axis X must be finite.");
  ValidateFinite(axisY, "Rotation axis Y must be finite.");
  ValidateFinite(axisZ, "Rotation axis Z must be finite.");
  ValidateFinite(angle, "Rotation angle must be finite.");
  const double axisMagnitudeSquared = axisX * axisX + axisY * axisY + axisZ * axisZ;
  if (!std::isfinite(axisMagnitudeSquared) || axisMagnitudeSquared <= 0.0)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Rotation axis must be finite and non-zero.");
  }

  gp_Trsf transform;
  if (angle != 0.0)
  {
    transform.SetRotation(
      gp_Ax1(gp_Pnt(0.0, 0.0, 0.0), gp_Dir(axisX, axisY, axisZ)), angle);
  }
  transform.SetTranslationPart(gp_Vec(translationX, translationY, translationZ));
  return transform;
}

void ValidateTransform(const OcctSharp_StepAssemblyInput& input)
{
  const double axisMagnitudeSquared = input.rotation_axis_x * input.rotation_axis_x
    + input.rotation_axis_y * input.rotation_axis_y
    + input.rotation_axis_z * input.rotation_axis_z;
  if (!std::isfinite(input.translation_x)
      || !std::isfinite(input.translation_y)
      || !std::isfinite(input.translation_z)
      || !std::isfinite(input.rotation_angle_radians)
      || !std::isfinite(axisMagnitudeSquared)
      || axisMagnitudeSquared <= 0.0)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "STEP assembly transforms must be finite and use a non-zero rotation axis.");
  }
}

gp_Trsf CreateTransform(const OcctSharp_StepAssemblyInput& input)
{
  ValidateTransform(input);
  gp_Trsf transform;
  if (input.rotation_angle_radians != 0.0)
  {
    transform.SetRotation(
      gp_Ax1(
        gp_Pnt(0.0, 0.0, 0.0),
        gp_Dir(input.rotation_axis_x, input.rotation_axis_y, input.rotation_axis_z)),
      input.rotation_angle_radians);
  }
  transform.SetTranslationPart(
    gp_Vec(input.translation_x, input.translation_y, input.translation_z));
  return transform;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform(
  const OcctSharp_ShapeHandle* shape,
  const double translation_x,
  const double translation_y,
  const double translation_z,
  const double rotation_axis_x,
  const double rotation_axis_y,
  const double rotation_axis_z,
  const double rotation_angle_radians,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  const double axisMagnitudeSquared = rotation_axis_x * rotation_axis_x
    + rotation_axis_y * rotation_axis_y
    + rotation_axis_z * rotation_axis_z;
  if (!std::isfinite(translation_x) || !std::isfinite(translation_y) || !std::isfinite(translation_z)
      || !std::isfinite(rotation_angle_radians) || !std::isfinite(axisMagnitudeSquared)
      || axisMagnitudeSquared <= 0.0)
  {
    SetLastError("Transform values must be finite and the rotation axis must be non-zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateShape(shape);
    gp_Trsf transform = CreateTranslationRotation(
      translation_x,
      translation_y,
      translation_z,
      rotation_axis_x,
      rotation_axis_y,
      rotation_axis_z,
      rotation_angle_radians);
    TopoDS_Shape transformed = BRepBuilderAPI_Transform(shape->Value, transform, false, false).Shape();
    *out_shape = AllocateShape(std::move(transformed));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_identity(
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    *out_transform = AllocateTransform(gp_Trsf());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_rotation(
  const double translation_x,
  const double translation_y,
  const double translation_z,
  const double rotation_axis_x,
  const double rotation_axis_y,
  const double rotation_axis_z,
  const double rotation_angle_radians,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    *out_transform = AllocateTransform(CreateTranslationRotation(
      translation_x,
      translation_y,
      translation_z,
      rotation_axis_x,
      rotation_axis_y,
      rotation_axis_z,
      rotation_angle_radians));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_clone(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(source);
    *out_transform = AllocateTransform(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_inverted(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(source);
    *out_transform = AllocateTransform(source->Value.Inverted());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_multiplied(
  const OcctSharp_TrsfHandle* left,
  const OcctSharp_TrsfHandle* right,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(left);
    ValidateTransformHandle(right);
    *out_transform = AllocateTransform(left->Value.Multiplied(right->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_value(
  const OcctSharp_TrsfHandle* transform,
  const int32_t row,
  const int32_t column,
  double* out_value)
{
  if (out_value == nullptr)
  {
    SetLastError("The output transform value pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_value = 0.0;
  return Guard([&]
  {
    ValidateTransformHandle(transform);
    if (row < 1 || row > 3 || column < 1 || column > 4)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Transform matrix indices must be row 1..3 and column 1..4.");
    }
    *out_value = transform->Value.Value(row, column);
  });
}

void OCCTSHARP_CALL occtsharp_trsf_release(OcctSharp_TrsfHandle* transform)
{
  if (transform != nullptr && UnregisterTransform(transform))
  {
    delete transform;
  }
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform_trsf(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateShape(shape);
    ValidateTransformHandle(transform);
    TopoDS_Shape transformed = BRepBuilderAPI_Transform(shape->Value, transform->Value, false, false).Shape();
    *out_shape = AllocateShape(std::move(transformed));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_identity(
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    *out_location = AllocateLocation(TopLoc_Location());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_from_trsf(
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateTransformHandle(transform);
    *out_location = AllocateLocation(TopLoc_Location(transform->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_clone(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(source);
    *out_location = AllocateLocation(source->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_inverted(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(source);
    *out_location = AllocateLocation(source->Value.Inverted());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_multiplied(
  const OcctSharp_LocationHandle* left,
  const OcctSharp_LocationHandle* right,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_location = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(left);
    ValidateLocationHandle(right);
    *out_location = AllocateLocation(left->Value.Multiplied(right->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_is_identity(
  const OcctSharp_LocationHandle* location,
  int32_t* out_is_identity)
{
  if (out_is_identity == nullptr)
  {
    SetLastError("The output identity pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_is_identity = 0;
  return Guard([&]
  {
    ValidateLocationHandle(location);
    *out_is_identity = location->Value.IsIdentity() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_location_to_trsf(
  const OcctSharp_LocationHandle* location,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr)
  {
    SetLastError("The output transform pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateLocationHandle(location);
    *out_transform = AllocateTransform(location->Value.Transformation());
  });
}

void OCCTSHARP_CALL occtsharp_location_release(OcctSharp_LocationHandle* location)
{
  if (location != nullptr && UnregisterLocation(location))
  {
    delete location;
  }
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_located(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_LocationHandle* location,
  const int32_t moved,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateShape(shape);
    ValidateLocationHandle(location);
    TopoDS_Shape result = moved == 0
      ? shape->Value.Located(location->Value, false)
      : shape->Value.Moved(location->Value, false);
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_vec(
  const OcctSharp_VecHandle* vector,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr) { SetLastError("The output transform pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateVector(vector);
    *out_transform = AllocateTransform(gp_Trsf());
    (*out_transform)->Value.SetTranslation(vector->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_rotation_axis(
  const OcctSharp_Ax1Handle* axis,
  const double angle_radians,
  OcctSharp_TrsfHandle** out_transform)
{
  if (out_transform == nullptr) { SetLastError("The output transform pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_transform = nullptr;
  return Guard([&]
  {
    ValidateAxis(axis);
    ValidateFinite(angle_radians, "Rotation angle must be finite.");
    gp_Trsf value;
    value.SetRotation(axis->Value, angle_radians);
    *out_transform = AllocateTransform(std::move(value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_create(
  const double x, const double y, const double z, OcctSharp_VecHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr;
  return Guard([&]
  {
    ValidateFinite(x, "Vector X must be finite."); ValidateFinite(y, "Vector Y must be finite."); ValidateFinite(z, "Vector Z must be finite.");
    *out_vector = AllocateValue(new OcctSharp_VecHandle(gp_Vec(x, y, z)), LiveVectors);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_clone(const OcctSharp_VecHandle* source, OcctSharp_VecHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr; return Guard([&] { ValidateVector(source); *out_vector = AllocateValue(new OcctSharp_VecHandle(source->Value), LiveVectors); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_components(const OcctSharp_VecHandle* vector, double* x, double* y, double* z)
{
  if (x == nullptr || y == nullptr || z == nullptr) { SetLastError("The vector component output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateVector(vector); *x = vector->Value.X(); *y = vector->Value.Y(); *z = vector->Value.Z(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_magnitude(const OcctSharp_VecHandle* vector, double* magnitude)
{
  if (magnitude == nullptr) { SetLastError("The vector magnitude output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateVector(vector); *magnitude = vector->Value.Magnitude(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_dot(const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, double* dot)
{
  if (dot == nullptr) { SetLastError("The vector dot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateVector(left); ValidateVector(right); *dot = left->Value.Dot(right->Value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_crossed(const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, OcctSharp_VecHandle** result)
{
  if (result == nullptr) { SetLastError("The output vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = nullptr; return Guard([&] { ValidateVector(left); ValidateVector(right); *result = AllocateValue(new OcctSharp_VecHandle(left->Value.Crossed(right->Value)), LiveVectors); });
}

void OCCTSHARP_CALL occtsharp_vec_release(OcctSharp_VecHandle* vector)
{ if (vector != nullptr && UnregisterValue(vector, LiveVectors)) delete vector; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_create(const double x, const double y, const double z, OcctSharp_DirHandle** out_direction)
{
  if (out_direction == nullptr) { SetLastError("The output direction pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_direction = nullptr; return Guard([&] { ValidateFinite(x, "Direction X must be finite."); ValidateFinite(y, "Direction Y must be finite."); ValidateFinite(z, "Direction Z must be finite."); *out_direction = AllocateValue(new OcctSharp_DirHandle(gp_Dir(x, y, z)), LiveDirections); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_clone(const OcctSharp_DirHandle* source, OcctSharp_DirHandle** out_direction)
{
  if (out_direction == nullptr) { SetLastError("The output direction pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_direction = nullptr; return Guard([&] { ValidateDirection(source); *out_direction = AllocateValue(new OcctSharp_DirHandle(source->Value), LiveDirections); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_components(const OcctSharp_DirHandle* direction, double* x, double* y, double* z)
{
  if (x == nullptr || y == nullptr || z == nullptr) { SetLastError("The direction component output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateDirection(direction); *x = direction->Value.X(); *y = direction->Value.Y(); *z = direction->Value.Z(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_dot(const OcctSharp_DirHandle* left, const OcctSharp_DirHandle* right, double* dot)
{
  if (dot == nullptr) { SetLastError("The direction dot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateDirection(left); ValidateDirection(right); *dot = left->Value.Dot(right->Value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_reversed(const OcctSharp_DirHandle* source, OcctSharp_DirHandle** result)
{
  if (result == nullptr) { SetLastError("The output direction pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = nullptr; return Guard([&] { ValidateDirection(source); *result = AllocateValue(new OcctSharp_DirHandle(source->Value.Reversed()), LiveDirections); });
}

void OCCTSHARP_CALL occtsharp_dir_release(OcctSharp_DirHandle* direction)
{ if (direction != nullptr && UnregisterValue(direction, LiveDirections)) delete direction; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_create(
  const double origin_x, const double origin_y, const double origin_z,
  const double direction_x, const double direction_y, const double direction_z,
  OcctSharp_Ax1Handle** out_axis)
{
  if (out_axis == nullptr) { SetLastError("The output axis pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_axis = nullptr; return Guard([&] { ValidateFinite(origin_x, "Axis origin X must be finite."); ValidateFinite(origin_y, "Axis origin Y must be finite."); ValidateFinite(origin_z, "Axis origin Z must be finite."); ValidateFinite(direction_x, "Axis direction X must be finite."); ValidateFinite(direction_y, "Axis direction Y must be finite."); ValidateFinite(direction_z, "Axis direction Z must be finite."); *out_axis = AllocateValue(new OcctSharp_Ax1Handle(gp_Ax1(gp_Pnt(origin_x, origin_y, origin_z), gp_Dir(direction_x, direction_y, direction_z))), LiveAxes); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_clone(const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** out_axis)
{
  if (out_axis == nullptr) { SetLastError("The output axis pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_axis = nullptr; return Guard([&] { ValidateAxis(source); *out_axis = AllocateValue(new OcctSharp_Ax1Handle(source->Value), LiveAxes); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_components(const OcctSharp_Ax1Handle* axis, double* ox, double* oy, double* oz, double* dx, double* dy, double* dz)
{
  if (ox == nullptr || oy == nullptr || oz == nullptr || dx == nullptr || dy == nullptr || dz == nullptr) { SetLastError("The axis component output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateAxis(axis); *ox = axis->Value.Location().X(); *oy = axis->Value.Location().Y(); *oz = axis->Value.Location().Z(); *dx = axis->Value.Direction().X(); *dy = axis->Value.Direction().Y(); *dz = axis->Value.Direction().Z(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_reversed(const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** result)
{
  if (result == nullptr) { SetLastError("The output axis pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = nullptr; return Guard([&] { ValidateAxis(source); *result = AllocateValue(new OcctSharp_Ax1Handle(source->Value.Reversed()), LiveAxes); });
}

void OCCTSHARP_CALL occtsharp_ax1_release(OcctSharp_Ax1Handle* axis)
{ if (axis != nullptr && UnregisterValue(axis, LiveAxes)) delete axis; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_create(const double* values, OcctSharp_MatHandle** out_matrix)
{
  if (values == nullptr || out_matrix == nullptr) { SetLastError("The matrix input or output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_matrix = nullptr; return Guard([&] { for (int i = 0; i < 9; ++i) ValidateFinite(values[i], "Matrix values must be finite."); *out_matrix = AllocateValue(new OcctSharp_MatHandle(gp_Mat(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8])), LiveMatrices); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_identity(OcctSharp_MatHandle** out_matrix)
{
  if (out_matrix == nullptr) { SetLastError("The output matrix pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_matrix = nullptr; return Guard([&] { *out_matrix = AllocateValue(new OcctSharp_MatHandle(gp_Mat(1,0,0,0,1,0,0,0,1)), LiveMatrices); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_clone(const OcctSharp_MatHandle* source, OcctSharp_MatHandle** out_matrix)
{
  if (out_matrix == nullptr) { SetLastError("The output matrix pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_matrix = nullptr; return Guard([&] { ValidateMatrix(source); *out_matrix = AllocateValue(new OcctSharp_MatHandle(source->Value), LiveMatrices); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_value(const OcctSharp_MatHandle* matrix, const int32_t row, const int32_t column, double* value)
{
  if (value == nullptr) { SetLastError("The matrix value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0; return Guard([&] { ValidateMatrix(matrix); if (row < 1 || row > 3 || column < 1 || column > 3) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Matrix indices must be row 1..3 and column 1..3."); *value = matrix->Value.Value(row, column); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_determinant(const OcctSharp_MatHandle* matrix, double* determinant)
{
  if (determinant == nullptr) { SetLastError("The matrix determinant output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateMatrix(matrix); *determinant = matrix->Value.Determinant(); });
}

void OCCTSHARP_CALL occtsharp_mat_release(OcctSharp_MatHandle* matrix)
{ if (matrix != nullptr && UnregisterValue(matrix, LiveMatrices)) delete matrix; }
