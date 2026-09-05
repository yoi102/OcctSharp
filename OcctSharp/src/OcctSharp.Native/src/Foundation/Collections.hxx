#pragma once

// Private native Foundation/Collections contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <NCollection_Array1.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_DynamicArray.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_Sequence.hxx>
#include <utility>

struct OcctSharp_RealSequenceHandle { explicit OcctSharp_RealSequenceHandle(NCollection_Sequence<double> value) : Value(std::move(value)) {} NCollection_Sequence<double> Value; };

struct OcctSharp_RealArrayHandle { explicit OcctSharp_RealArrayHandle(NCollection_Array1<double> value) : Value(std::move(value)) {} NCollection_Array1<double> Value; };

struct OcctSharp_RealVectorHandle { explicit OcctSharp_RealVectorHandle(NCollection_DynamicArray<double> value) : Value(std::move(value)) {} NCollection_DynamicArray<double> Value; };

struct OcctSharp_IntRealMapHandle { explicit OcctSharp_IntRealMapHandle(NCollection_DataMap<int32_t, double> value) : Value(std::move(value)) {} NCollection_DataMap<int32_t, double> Value; };

struct OcctSharp_IntIndexedMapHandle { explicit OcctSharp_IntIndexedMapHandle(NCollection_IndexedMap<int32_t> value) : Value(std::move(value)) {} NCollection_IndexedMap<int32_t> Value; };

namespace OcctSharp::Native
{
void ValidateRealSequence(const OcctSharp_RealSequenceHandle* handle);

void ValidateRealArray(const OcctSharp_RealArrayHandle* handle);

void ValidateRealVector(const OcctSharp_RealVectorHandle* handle);

void ValidateIntRealMap(const OcctSharp_IntRealMapHandle* handle);

void ValidateIntIndexedMap(const OcctSharp_IntIndexedMapHandle* handle);

void ValidateSequenceIndex(const OcctSharp_RealSequenceHandle* sequence, const int32_t index);

void ValidateArrayIndex(const OcctSharp_RealArrayHandle* array, const int32_t index);

void ValidateVectorIndex(const OcctSharp_RealVectorHandle* vector, const int32_t index);
}
