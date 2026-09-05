#pragma once

// Private native Geometry/Transforms contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <TopLoc_Location.hxx>
#include <gp_Ax1.hxx>
#include <gp_Dir.hxx>
#include <gp_Mat.hxx>
#include <gp_Trsf.hxx>
#include <gp_Vec.hxx>
#include <utility>

struct OcctSharp_TrsfHandle
{
  explicit OcctSharp_TrsfHandle(gp_Trsf value)
    : Value(std::move(value))
  {
  }

  gp_Trsf Value;
};

struct OcctSharp_LocationHandle
{
  explicit OcctSharp_LocationHandle(TopLoc_Location value)
    : Value(std::move(value))
  {
  }

  TopLoc_Location Value;
};

struct OcctSharp_VecHandle { explicit OcctSharp_VecHandle(gp_Vec value) : Value(std::move(value)) {} gp_Vec Value; };

struct OcctSharp_DirHandle { explicit OcctSharp_DirHandle(gp_Dir value) : Value(std::move(value)) {} gp_Dir Value; };

struct OcctSharp_Ax1Handle { explicit OcctSharp_Ax1Handle(gp_Ax1 value) : Value(std::move(value)) {} gp_Ax1 Value; };

struct OcctSharp_MatHandle { explicit OcctSharp_MatHandle(gp_Mat value) : Value(std::move(value)) {} gp_Mat Value; };

namespace OcctSharp::Native
{
void RegisterTransform(OcctSharp_TrsfHandle* handle);

bool IsLiveTransform(const OcctSharp_TrsfHandle* handle);

bool UnregisterTransform(const OcctSharp_TrsfHandle* handle);

OcctSharp_TrsfHandle* AllocateTransform(gp_Trsf value);

void RegisterLocation(OcctSharp_LocationHandle* handle);

bool IsLiveLocation(const OcctSharp_LocationHandle* handle);

bool UnregisterLocation(const OcctSharp_LocationHandle* handle);

OcctSharp_LocationHandle* AllocateLocation(TopLoc_Location value);

void ValidateVector(const OcctSharp_VecHandle* handle);

void ValidateDirection(const OcctSharp_DirHandle* handle);

void ValidateAxis(const OcctSharp_Ax1Handle* handle);

void ValidateMatrix(const OcctSharp_MatHandle* handle);

void ValidateTransformHandle(const OcctSharp_TrsfHandle* handle);

void ValidateLocationHandle(const OcctSharp_LocationHandle* handle);

gp_Trsf CreateTranslationRotation(
  const double translationX,
  const double translationY,
  const double translationZ,
  const double axisX,
  const double axisY,
  const double axisZ,
  const double angle);

void ValidateTransform(const OcctSharp_StepAssemblyInput& input);

gp_Trsf CreateTransform(const OcctSharp_StepAssemblyInput& input);
}
