#pragma once

// Private native Foundation/Transient contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <Standard_Handle.hxx>
#include <Standard_Transient.hxx>
#include <Standard_Type.hxx>
#include <utility>

class OcctSharp_TransientDerived final : public Standard_Transient
{
  DEFINE_STANDARD_RTTI_INLINE(OcctSharp_TransientDerived, Standard_Transient)
};

struct OcctSharp_TransientHandle
{
  explicit OcctSharp_TransientHandle(opencascade::handle<Standard_Transient> value)
    : Value(std::move(value))
  {
  }

  opencascade::handle<Standard_Transient> Value;
};

namespace OcctSharp::Native
{
void RegisterTransient(OcctSharp_TransientHandle* handle);

bool IsLiveTransient(const OcctSharp_TransientHandle* handle);

bool UnregisterTransient(const OcctSharp_TransientHandle* handle);

OcctSharp_TransientHandle* AllocateTransient(opencascade::handle<Standard_Transient> value);

void ValidateTransient(const OcctSharp_TransientHandle* handle);
}
