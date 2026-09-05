#include "Modeling/ScalarLaws.hxx"
#include "Runtime/Error.hxx"
#include <Law_Constant.hxx>
#include <Law_Linear.hxx>
#include <Law_Interpol.hxx>
#include <Law_BSpline.hxx>
#include <Law_BSpFunc.hxx>
#include <Law_S.hxx>
#include <Law_Composite.hxx>
#include <NCollection_Array1.hxx>
#include <gp_Pnt2d.hxx>
#include <algorithm>
#include <cmath>
#include <limits>

namespace OcctSharp::Native {
namespace {
void Require(bool ok, const char* message) {
  if (!ok) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
void Range(int offset, int count, int capacity) {
  Require(offset >= 0 && count >= 0 && offset <= capacity && count <= capacity - offset,
    "A scalar-law buffer range is invalid.");
}
opencascade::handle<Law_Function> BuildSpan(const OcctSharp_LawInput& input,
  const OcctSharp_LawSpan& span, double& lower) {
  Require(span.kind >= 0 && span.kind <= 4 && (span.tangents == 0 || span.tangents == 1), "Unknown law kind or tangent flag.");
  for (double value : {span.first, span.last, span.value_first, span.value_last,
      span.derivative_first, span.derivative_last}) Require(std::isfinite(value), "Law coefficients must be finite.");
  Require(span.first < span.last && std::isfinite(span.last - span.first), "Law span domain is invalid.");
  Range(span.value_offset, span.value_count, input.value_count);
  Range(span.parameter_offset, span.parameter_count, input.value_count);
  if (span.kind == 0) {
    opencascade::handle<Law_Constant> law = new Law_Constant();
    law->Set(span.value_first, span.first, span.last); lower = span.value_first; return law;
  }
  if (span.kind == 1) {
    opencascade::handle<Law_Linear> law = new Law_Linear();
    law->Set(span.first, span.value_first, span.last, span.value_last);
    lower = std::min(span.value_first, span.value_last); return law;
  }
  opencascade::handle<Law_BSpFunc> law;
  if (span.kind == 2) {
    Require(span.value_count >= 2 && span.value_count == span.parameter_count, "Interpolation requires matching parameters and values.");
    NCollection_Array1<gp_Pnt2d> points(1, span.value_count);
    for (int i = 0; i < span.value_count; ++i) {
      const double parameter = input.values[span.parameter_offset + i];
      if (i) Require(parameter > input.values[span.parameter_offset + i - 1], "Law parameters must strictly increase.");
      points.SetValue(i + 1, gp_Pnt2d(parameter, input.values[span.value_offset + i]));
    }
    Require(points.First().X() == span.first && points.Last().X() == span.last, "Interpolation endpoints must equal the definition domain.");
    opencascade::handle<Law_Interpol> interpolation = new Law_Interpol();
    if (span.tangents) interpolation->Set(points, span.derivative_first, span.derivative_last, false);
    else interpolation->Set(points, false);
    law = interpolation;
  } else if (span.kind == 3) {
    Require(span.degree >= 1 && span.degree <= 25 && span.value_count >= span.degree + 1 && span.parameter_count >= 2,
      "Invalid scalar B-spline dimensions or degree.");
    Range(span.multiplicity_offset, span.parameter_count, input.multiplicity_count);
    NCollection_Array1<double> poles(1, span.value_count), knots(1, span.parameter_count);
    NCollection_Array1<int> multiplicities(1, span.parameter_count);
    int64_t sum = 0;
    for (int i = 0; i < span.value_count; ++i) poles.SetValue(i + 1, input.values[span.value_offset + i]);
    for (int i = 0; i < span.parameter_count; ++i) {
      const double knot = input.values[span.parameter_offset + i];
      if (i) Require(knot > knots.Value(i), "Scalar B-spline knots must strictly increase.");
      knots.SetValue(i + 1, knot);
      const int multiplicity = input.multiplicities[span.multiplicity_offset + i];
      Require(multiplicity >= 1 && multiplicity <= span.degree + ((i == 0 || i == span.parameter_count - 1) ? 1 : 0), "Invalid knot multiplicity.");
      multiplicities.SetValue(i + 1, multiplicity); sum += multiplicity;
    }
    Require(sum == static_cast<int64_t>(span.value_count) + span.degree + 1, "Scalar B-spline multiplicity sum is inconsistent.");
    opencascade::handle<Law_BSpline> curve = new Law_BSpline(poles, knots, multiplicities, span.degree, false);
    Require(span.first == curve->FirstParameter() && span.last == curve->LastParameter(), "Scalar B-spline definition domain differs from its active knots.");
    law = new Law_BSpFunc(curve, span.first, span.last);
  } else {
    opencascade::handle<Law_S> smooth = new Law_S();
    smooth->Set(span.first, span.value_first, span.derivative_first, span.last, span.value_last, span.derivative_last);
    law = smooth;
  }
  lower = std::numeric_limits<double>::infinity();
  const auto curve = law->Curve();
  Require(!curve.IsNull(), "Law construction produced no B-spline.");
  for (int i = 1; i <= curve->NbPoles(); ++i) {
    const double value = curve->Pole(i);
    Require(std::isfinite(value), "Law interpolation produced a non-finite control pole.");
    lower = std::min(lower, value);
  }
  return law;
}
}

ScalarLawData BuildScalarLaw(const OcctSharp_LawInput& input) {
  Require(input.reserved == 0 && input.spans != nullptr && input.span_count >= 1 && input.span_count <= 256,
    "Scalar law requires one to 256 spans.");
  Require(input.value_count >= 0 && input.value_count <= 65536 && input.multiplicity_count >= 0 && input.multiplicity_count <= 65536,
    "Scalar law exceeds the bounded buffer limit.");
  Require((input.value_count == 0 || input.values != nullptr) && (input.multiplicity_count == 0 || input.multiplicities != nullptr), "Missing scalar-law buffers.");
  Require(std::isfinite(input.first) && std::isfinite(input.last) && input.first < input.last,
    "The active law domain must be finite and increasing.");
  for (int i = 0; i < input.value_count; ++i) Require(std::isfinite(input.values[i]), "Law data must be finite.");
  Require(input.first >= input.spans[0].active_first && input.last <= input.spans[input.span_count - 1].active_last,
    "The active domain lies outside the law definition.");
  ScalarLawData result; result.LowerBound = std::numeric_limits<double>::infinity();
  opencascade::handle<Law_Composite> composite = new Law_Composite(input.first, input.last, 1e-12);
  for (int i = 0; i < input.span_count; ++i) {
    const auto& span = input.spans[i];
    Require(std::isfinite(span.active_first) && std::isfinite(span.active_last) && span.active_first < span.active_last
      && span.active_first >= span.first && span.active_last <= span.last, "Invalid active elementary law domain.");
    if (i) Require(span.active_first == input.spans[i - 1].active_last, "Composite law domains must cover consecutive non-overlapping spans.");
    double lower = 0;
    auto law = BuildSpan(input, span, lower);
    if (span.active_last <= input.first || span.active_first >= input.last) continue;
    const double first = std::max(span.active_first, input.first), last = std::min(span.active_last, input.last);
    law = law->Trim(first, last, 1e-12);
    result.LowerBound = std::min(result.LowerBound, lower);
    result.Spans.push_back(law); result.Ends.push_back(last); composite->ChangeLaws().Append(law);
  }
  Require(!result.Spans.empty(), "No active scalar-law spans remain.");
  result.Function = result.Spans.size() == 1 ? result.Spans.front() : opencascade::handle<Law_Function>(composite);
  return result;
}

OcctSharp_LawSample SampleScalarLaw(const ScalarLawData& law, double parameter) {
  size_t index = 0;
  while (index + 1 < law.Spans.size() && parameter >= law.Ends[index]) ++index;
  OcctSharp_LawSample sample{parameter, law.Spans[index]->Value(parameter), 0, 0, 0, 0};
  try { law.Spans[index]->D2(parameter, sample.value, sample.first_derivative, sample.second_derivative); sample.defined = 3; }
  catch (const Standard_Failure&) {
    try { law.Spans[index]->D1(parameter, sample.value, sample.first_derivative); sample.defined = 1; }
    catch (const Standard_Failure&) { sample.defined = 0; }
  }
  // A one-sided SDK derivative at a multiple knot is not a two-sided derivative.
  for (int order = 1; order <= 2; ++order) {
    const auto continuity = order == 1 ? GeomAbs_C1 : GeomAbs_C2;
    const int intervals = law.Spans[index]->NbIntervals(continuity);
    Require(intervals >= 1 && intervals <= 65536, "Law continuity interval count exceeds the limit.");
    NCollection_Array1<double> breaks(1, intervals + 1); law.Spans[index]->Intervals(breaks, continuity);
    for (int i = 2; i <= intervals; ++i) if (parameter == breaks(i)) sample.defined &= order == 1 ? 0 : 1;
  }
  if (index > 0 && parameter == law.Ends[index - 1]) {
    double value = 0, first = 0, second = 0;
    try {
      law.Spans[index - 1]->D2(parameter, value, first, second);
      if (std::abs(value - sample.value) > 1e-10 || std::abs(first - sample.first_derivative) > 1e-10) sample.defined = 0;
      else if (std::abs(second - sample.second_derivative) > 1e-10) sample.defined &= 1;
    } catch (const Standard_Failure&) { sample.defined = 0; }
  }
  Require(std::isfinite(sample.value), "Law evaluation is not finite.");
  if (!std::isfinite(sample.first_derivative)) sample.defined = 0;
  if (!std::isfinite(sample.second_derivative)) sample.defined &= 1;
  if (!(sample.defined & 1)) sample.first_derivative = 0;
  if (!(sample.defined & 2)) sample.second_derivative = 0;
  return sample;
}
}

using namespace OcctSharp::Native;
OcctSharp_Status OCCTSHARP_CALL occtsharp_law_evaluate(const OcctSharp_LawInput* input,
  const double* parameters, int32_t count, OcctSharp_LawSample* samples, int32_t capacity, double* lower) {
  return Guard([&] {
    Require(input != nullptr && parameters != nullptr && samples != nullptr && lower != nullptr,
      "A scalar-law argument is null.");
    Require(count >= 1 && count <= 65536 && capacity >= count, "Invalid law sample count or capacity.");
    const auto law = BuildScalarLaw(*input);
    std::vector<OcctSharp_LawSample> copied; copied.reserve(count);
    for (int i = 0; i < count; ++i) {
      Require(std::isfinite(parameters[i]) && parameters[i] >= input->first && parameters[i] <= input->last,
        "A parameter is outside the active scalar-law domain.");
      copied.push_back(SampleScalarLaw(law, parameters[i]));
    }
    std::copy(copied.begin(), copied.end(), samples); *lower = law.LowerBound;
  });
}
