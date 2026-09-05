#pragma once

// Private native Visualization/Selection contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <AIS_SelectionScheme.hxx>

namespace OcctSharp::Native
{
AIS_SelectionScheme ToSelectionScheme(const int32_t selectionMode);
}
