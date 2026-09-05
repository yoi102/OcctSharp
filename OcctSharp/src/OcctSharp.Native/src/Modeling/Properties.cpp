// Native Modeling/Properties implementation. Public contracts and ownership are unchanged.
#include "Modeling/Properties.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <GProp_PrincipalProps.hxx>
#include <cmath>
#include <gp_Pnt.hxx>
#include <utility>

namespace OcctSharp::Native
{
void ValidateGProps(const OcctSharp_GPropsHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The GProp_GProps handle is null.");
  if (!IsLiveValue(handle, LiveGProps)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The GProp_GProps handle is invalid or already released.");
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_create(OcctSharp_GPropsHandle** out_properties)
{
  if (out_properties == nullptr) { SetLastError("The output GProp_GProps pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_properties = nullptr;
  return Guard([&] { *out_properties = AllocateValue(new OcctSharp_GPropsHandle(GProp_GProps()), LiveGProps); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_from_shape(
  const OcctSharp_ShapeHandle* shape, const int32_t mode, const int32_t only_closed,
  OcctSharp_GPropsHandle** out_properties)
{
  if (out_properties == nullptr) { SetLastError("The output GProp_GProps pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_properties = nullptr;
  if (mode < 0 || mode > 2) { SetLastError("GProp mode must be 0 (linear), 1 (surface), or 2 (volume)."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    GProp_GProps value;
    if (mode == 0)
      BRepGProp::LinearProperties(shape->Value, value);
    else if (mode == 1)
      BRepGProp::SurfaceProperties(shape->Value, value);
    else
      BRepGProp::VolumeProperties(shape->Value, value, only_closed != 0);
    *out_properties = AllocateValue(new OcctSharp_GPropsHandle(std::move(value)), LiveGProps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_clone(
  const OcctSharp_GPropsHandle* source, OcctSharp_GPropsHandle** out_properties)
{
  if (out_properties == nullptr) { SetLastError("The output GProp_GProps pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_properties = nullptr;
  return Guard([&]
  {
    ValidateGProps(source);
    *out_properties = AllocateValue(new OcctSharp_GPropsHandle(source->Value), LiveGProps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_add(
  OcctSharp_GPropsHandle* target, const OcctSharp_GPropsHandle* item, const double density)
{
  return Guard([&]
  {
    ValidateGProps(target);
    ValidateGProps(item);
    if (!std::isfinite(density) || density <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "GProp density must be finite and greater than zero.");
    target->Value.Add(item->Value, density);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_mass(
  const OcctSharp_GPropsHandle* properties, double* mass)
{
  if (mass == nullptr) { SetLastError("The GProp mass output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *mass = 0.0;
  return Guard([&] { ValidateGProps(properties); *mass = properties->Value.Mass(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_center(
  const OcctSharp_GPropsHandle* properties, OcctSharp_Xyz* center)
{
  if (center == nullptr) { SetLastError("The GProp center output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *center = {};
  return Guard([&]
  {
    ValidateGProps(properties);
    const gp_Pnt value = properties->Value.CentreOfMass();
    *center = { value.X(), value.Y(), value.Z() };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_inertia_value(
  const OcctSharp_GPropsHandle* properties, const int32_t row, const int32_t column, double* value)
{
  if (value == nullptr) { SetLastError("The GProp inertia output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0.0;
  if (row < 1 || row > 3 || column < 1 || column > 3) { SetLastError("GProp inertia indices are 1-based and must be 1..3."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateGProps(properties); *value = properties->Value.MatrixOfInertia().Value(row, column); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_principal_moments(
  const OcctSharp_GPropsHandle* properties, double* first, double* second, double* third)
{
  if (first == nullptr || second == nullptr || third == nullptr) { SetLastError("The principal-moment output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *first = *second = *third = 0.0;
  return Guard([&]
  {
    ValidateGProps(properties);
    properties->Value.PrincipalProperties().Moments(*first, *second, *third);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gprops_symmetry(
  const OcctSharp_GPropsHandle* properties, int32_t* axis, int32_t* point)
{
  if (axis == nullptr || point == nullptr) { SetLastError("The symmetry output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *axis = *point = 0;
  return Guard([&]
  {
    ValidateGProps(properties);
    const GProp_PrincipalProps principal = properties->Value.PrincipalProperties();
    *axis = principal.HasSymmetryAxis() ? 1 : 0;
    *point = principal.HasSymmetryPoint() ? 1 : 0;
  });
}

void OCCTSHARP_CALL occtsharp_gprops_release(OcctSharp_GPropsHandle* properties)
{
  if (properties != nullptr && UnregisterValue(properties, LiveGProps)) delete properties;
}
