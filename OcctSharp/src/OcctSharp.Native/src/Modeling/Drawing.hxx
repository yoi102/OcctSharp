#pragma once

// Private native Modeling/Drawing contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <HLRAlgo_Projector.hxx>
#include <TopoDS_Shape.hxx>
#include <vector>

namespace OcctSharp::Native
{
struct DrawingPolylineData
{
  std::vector<OcctSharp_DrawingPolyline> Polylines;
  std::vector<OcctSharp_Xyz> Points;
};

TopoDS_Shape NonNullDrawingLayer(TopoDS_Shape shape);

HLRAlgo_Projector MakeDrawingProjector(const OcctSharp_DrawingProjection& projection);

DrawingPolylineData BuildDrawingPolylines(const TopoDS_Shape& shape, const int32_t samples_per_curve);
}
