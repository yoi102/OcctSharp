#pragma once

// Private native Modeling/Properties contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <GProp_GProps.hxx>
#include <utility>

struct OcctSharp_GPropsHandle { explicit OcctSharp_GPropsHandle(GProp_GProps value) : Value(std::move(value)) {} GProp_GProps Value; };

namespace OcctSharp::Native
{
void ValidateGProps(const OcctSharp_GPropsHandle* handle);
}
