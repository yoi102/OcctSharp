#pragma once

// Private native Visualization/Navigation contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <V3d_TypeOfOrientation.hxx>

namespace OcctSharp::Native
{
Aspect_TypeOfTriedronPosition ToTrihedronPosition(const int32_t position);

V3d_TypeOfOrientation ToViewerProjection(const int32_t projection);
}
