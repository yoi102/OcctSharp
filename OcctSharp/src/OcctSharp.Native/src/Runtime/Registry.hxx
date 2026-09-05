#pragma once

// Private native Runtime/Registry contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "OcctSharp.Native.Repair.h"
#include <mutex>
#include <unordered_set>

namespace OcctSharp::Native
{
extern std::mutex LiveShapesMutex;

extern std::unordered_set<const OcctSharp_ShapeHandle*> LiveShapes;

extern std::unordered_set<const OcctSharp_TransientHandle*> LiveTransients;

extern std::unordered_set<const OcctSharp_TrsfHandle*> LiveTransforms;

extern std::unordered_set<const OcctSharp_LocationHandle*> LiveLocations;

extern std::unordered_set<const OcctSharp_VecHandle*> LiveVectors;

extern std::unordered_set<const OcctSharp_DirHandle*> LiveDirections;

extern std::unordered_set<const OcctSharp_Ax1Handle*> LiveAxes;

extern std::unordered_set<const OcctSharp_MatHandle*> LiveMatrices;

extern std::unordered_set<const OcctSharp_AsciiStringHandle*> LiveAsciiStrings;

extern std::unordered_set<const OcctSharp_ExtendedStringHandle*> LiveExtendedStrings;

extern std::unordered_set<const OcctSharp_RealSequenceHandle*> LiveRealSequences;

extern std::unordered_set<const OcctSharp_RealArrayHandle*> LiveRealArrays;

extern std::unordered_set<const OcctSharp_RealVectorHandle*> LiveRealVectors;

extern std::unordered_set<const OcctSharp_IntRealMapHandle*> LiveIntRealMaps;

extern std::unordered_set<const OcctSharp_IntIndexedMapHandle*> LiveIntIndexedMaps;

extern std::unordered_set<const OcctSharp_GPropsHandle*> LiveGProps;

extern std::unordered_set<const OcctSharp_OcafDocumentHandle*> LiveOcafDocuments;

extern std::unordered_set<const OcctSharp_ViewerHandle*> LiveViewers;

extern std::unordered_set<const OcctSharp_StepReaderHandle*> LiveStepReaders;

extern std::unordered_set<const OcctSharp_FeatureResultHandle*> LiveFeatureResults;
extern std::unordered_set<const OcctSharp_RepairResultHandle*> LiveRepairResults;

template <typename T>
void RegisterValue(T* handle, std::unordered_set<const T*>& live)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  live.insert(handle);
}

template <typename T>
bool IsLiveValue(const T* handle, const std::unordered_set<const T*>& live)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return live.contains(handle);
}

template <typename T>
bool UnregisterValue(const T* handle, std::unordered_set<const T*>& live)
{
  std::lock_guard<std::mutex> lock(LiveShapesMutex);
  return live.erase(handle) != 0;
}

template <typename T>
T* AllocateValue(T* handle, std::unordered_set<const T*>& live)
{
  try
  {
    RegisterValue(handle, live);
    return handle;
  }
  catch (...)
  {
    delete handle;
    throw;
  }
}
}
