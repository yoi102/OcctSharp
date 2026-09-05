#pragma once
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.Parametric.h"
#include "Runtime/Error.hxx"
#include <TFunction_GraphNode.hxx>
#include <TFunction_Scope.hxx>
#include <cstddef>

static_assert(sizeof(OcctSharp_ParameterInfo) == 24);
static_assert(alignof(OcctSharp_ParameterInfo) == 8);
static_assert(offsetof(OcctSharp_ParameterInfo, real_value) == 16);

namespace OcctSharp::Native::Parametric
{
inline void Require(bool condition, const char* message)
{
  if (!condition) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
occ::handle<TFunction_Scope> Scope(const TDF_Label& label);
occ::handle<TFunction_GraphNode> Node(const TDF_Label& label);
}
