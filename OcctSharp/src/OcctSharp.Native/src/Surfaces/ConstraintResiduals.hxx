#pragma once
#include "OcctSharp.Native.Authoring.h"
#include <Geom_Surface.hxx>
#include <Standard_Handle.hxx>
#include <gp_Pnt.hxx>

namespace OcctSharp::Native::Authoring {
// Independent residuals on the final approximated surface, not just solver state.
OcctSharp_ConstraintResidual SurfaceResidual(const opencascade::handle<Geom_Surface>& result,
  const gp_Pnt& target, const opencascade::handle<Geom_Surface>& support, double supportU, double supportV,
  int order, double tolerance);
}
