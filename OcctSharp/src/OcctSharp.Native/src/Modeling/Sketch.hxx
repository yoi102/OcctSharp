#pragma once

// Private native Modeling/Sketch contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <Geom2d_Curve.hxx>
#include <Standard_Handle.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt2d.hxx>

namespace OcctSharp::Native
{
gp_Pnt2d ToSketchPoint(const OcctSharp_SketchPoint2d& value, const char* message);

OcctSharp_SketchPoint2d FromSketchPoint(const gp_Pnt2d& value);

opencascade::handle<Geom2d_Curve> BuildSketchCurve(const OcctSharp_SketchCurve& definition);

double SketchBasisParameter(const OcctSharp_SketchCurve& definition, const double parameter);

double SketchResultParameter(const OcctSharp_SketchCurve& definition, double parameter);

gp_Pln BuildSketchPlane(const OcctSharp_SketchPlane& definition);
}
