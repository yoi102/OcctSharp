#pragma once

// Private native Modeling/Features contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <memory>
#include <string>
#include <vector>

struct OcctSharp_FeatureHistoryEntry
{
  int32_t SourceIndex = 0;
  int32_t Kind = 0;
  TopoDS_Shape Shape;
  int32_t SubshapeIndex = -1;
  int32_t SourceKind = -1;
};

struct OcctSharp_LocalFeatureData;

struct OcctSharp_FeatureResultHandle
{
  OcctSharp_FeatureResultInfo Info{};
  TopoDS_Shape Result;
  std::vector<OcctSharp_FeatureHistoryEntry> History;
  std::vector<int32_t> Deleted;
  std::string Message;
  // Optional copied diagnostics owned by this existing result, never a builder or registry.
  std::shared_ptr<OcctSharp_LocalFeatureData> LocalFeature;
};

namespace OcctSharp::Native
{
OcctSharp_FeatureResultHandle* AllocateFeatureResult();

OcctSharp_FeatureResultHandle* RegisterFeatureResult(
  std::unique_ptr<OcctSharp_FeatureResultHandle> result);

bool IsLiveFeatureResult(const OcctSharp_FeatureResultHandle* result);

bool UnregisterFeatureResult(const OcctSharp_FeatureResultHandle* result);

void ValidateFeatureResult(const OcctSharp_FeatureResultHandle* result);
}
