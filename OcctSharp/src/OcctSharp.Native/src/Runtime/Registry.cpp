// Native Runtime/Registry implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <mutex>
#include <unordered_set>

namespace OcctSharp::Native
{
std::mutex LiveShapesMutex;

std::unordered_set<const OcctSharp_ShapeHandle*> LiveShapes;

std::unordered_set<const OcctSharp_TransientHandle*> LiveTransients;

std::unordered_set<const OcctSharp_TrsfHandle*> LiveTransforms;

std::unordered_set<const OcctSharp_LocationHandle*> LiveLocations;

std::unordered_set<const OcctSharp_VecHandle*> LiveVectors;

std::unordered_set<const OcctSharp_DirHandle*> LiveDirections;

std::unordered_set<const OcctSharp_Ax1Handle*> LiveAxes;

std::unordered_set<const OcctSharp_MatHandle*> LiveMatrices;

std::unordered_set<const OcctSharp_AsciiStringHandle*> LiveAsciiStrings;

std::unordered_set<const OcctSharp_ExtendedStringHandle*> LiveExtendedStrings;

std::unordered_set<const OcctSharp_RealSequenceHandle*> LiveRealSequences;

std::unordered_set<const OcctSharp_RealArrayHandle*> LiveRealArrays;

std::unordered_set<const OcctSharp_RealVectorHandle*> LiveRealVectors;

std::unordered_set<const OcctSharp_IntRealMapHandle*> LiveIntRealMaps;

std::unordered_set<const OcctSharp_IntIndexedMapHandle*> LiveIntIndexedMaps;

std::unordered_set<const OcctSharp_GPropsHandle*> LiveGProps;

std::unordered_set<const OcctSharp_OcafDocumentHandle*> LiveOcafDocuments;

std::unordered_set<const OcctSharp_ViewerHandle*> LiveViewers;

std::unordered_set<const OcctSharp_StepReaderHandle*> LiveStepReaders;

std::unordered_set<const OcctSharp_FeatureResultHandle*> LiveFeatureResults;
}
