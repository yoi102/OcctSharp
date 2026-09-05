#pragma once
#include "OcctSharp.Native.Regions.h"
#include "Modeling/Features.hxx"
#include <NCollection_IndexedMap.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <vector>

struct OcctSharp_RegionData {
  OcctSharp_RegionInfo Info{};
  std::vector<OcctSharp_RegionItem> Items;
  std::vector<TopoDS_Shape> Shapes;
};

namespace OcctSharp::Native::Regions {
using ShapeMap = NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher>;
enum ItemKind { Cell, Membership, Boundary, BoundaryUse, Assignment, Output, RuleEffect,
  InputMeasure, History, Fault, SourceFace, UnusedFace, FreeBoundary, InternalTopology, HelperCheck,
  ShellCandidate, VolumeShell };
int Dimension(const TopoDS_Shape& shape);
double Measure(const TopoDS_Shape& shape, int dimension);
void Add(OcctSharp_RegionData& data, int kind, int a = -1, int b = -1, int c = -1,
  int d = -1, int flags = 0, double measure = 0, const TopoDS_Shape& shape = {});
void InspectBoundaries(const std::vector<TopoDS_Shape>& cells, OcctSharp_RegionData& data);
void Publish(std::unique_ptr<OcctSharp_FeatureResultHandle> owner, OcctSharp_FeatureResultHandle** output);
bool CheckInputs(const std::vector<TopoDS_Shape>& inputs, OcctSharp_FeatureResultHandle& owner);
}
