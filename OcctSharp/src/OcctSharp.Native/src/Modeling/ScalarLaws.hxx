#pragma once
#include "OcctSharp.Native.Authoring.h"
#include <Law_Function.hxx>
#include <Standard_Handle.hxx>
#include <vector>

namespace OcctSharp::Native {
struct ScalarLawData {
  opencascade::handle<Law_Function> Function;
  std::vector<opencascade::handle<Law_Function>> Spans;
  std::vector<double> Ends;
  double LowerBound = 0;
};
ScalarLawData BuildScalarLaw(const OcctSharp_LawInput& input);
OcctSharp_LawSample SampleScalarLaw(const ScalarLawData& law, double parameter);
}
