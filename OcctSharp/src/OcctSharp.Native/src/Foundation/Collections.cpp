// Native Foundation/Collections implementation. Public contracts and ownership are unchanged.
#include "Foundation/Collections.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <NCollection_Array1.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_DynamicArray.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_Sequence.hxx>
#include <utility>

namespace OcctSharp::Native
{
void ValidateRealSequence(const OcctSharp_RealSequenceHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The real sequence handle is null.");
  if (!IsLiveValue(handle, LiveRealSequences)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The real sequence handle is invalid or already released.");
}

void ValidateRealArray(const OcctSharp_RealArrayHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The real array handle is null.");
  if (!IsLiveValue(handle, LiveRealArrays)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The real array handle is invalid or already released.");
}

void ValidateRealVector(const OcctSharp_RealVectorHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The real vector handle is null.");
  if (!IsLiveValue(handle, LiveRealVectors)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The real vector handle is invalid or already released.");
}

void ValidateIntRealMap(const OcctSharp_IntRealMapHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The integer-real map handle is null.");
  if (!IsLiveValue(handle, LiveIntRealMaps)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The integer-real map handle is invalid or already released.");
}

void ValidateIntIndexedMap(const OcctSharp_IntIndexedMapHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The integer indexed map handle is null.");
  if (!IsLiveValue(handle, LiveIntIndexedMaps)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The integer indexed map handle is invalid or already released.");
}

void ValidateSequenceIndex(const OcctSharp_RealSequenceHandle* sequence, const int32_t index)
{
  const int32_t length = sequence->Value.Length();
  if (index < 1 || index > length) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sequence indices are 1-based and must be within the sequence length.");
}

void ValidateArrayIndex(const OcctSharp_RealArrayHandle* array, const int32_t index)
{
  if (index < array->Value.Lower() || index > array->Value.Upper()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Array indices are outside the native lower and upper bounds.");
}

void ValidateVectorIndex(const OcctSharp_RealVectorHandle* vector, const int32_t index)
{
  if (index < 0 || index >= vector->Value.Length()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Vector indices are zero-based and must be within the vector length.");
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_create(
  const double* values, const int32_t count, OcctSharp_RealSequenceHandle** out_sequence)
{
  if (out_sequence == nullptr) { SetLastError("The output real sequence pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_sequence = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sequence count or input values are invalid.");
    NCollection_Sequence<double> sequence;
    for (int32_t index = 0; index < count; ++index) { ValidateFinite(values[index], "Sequence values must be finite."); sequence.Append(values[index]); }
    *out_sequence = AllocateValue(new OcctSharp_RealSequenceHandle(std::move(sequence)), LiveRealSequences);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_clone(
  const OcctSharp_RealSequenceHandle* source, OcctSharp_RealSequenceHandle** out_sequence)
{
  if (out_sequence == nullptr) { SetLastError("The output real sequence pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_sequence = nullptr;
  return Guard([&] { ValidateRealSequence(source); *out_sequence = AllocateValue(new OcctSharp_RealSequenceHandle(source->Value), LiveRealSequences); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_length(const OcctSharp_RealSequenceHandle* sequence, int32_t* length)
{
  if (length == nullptr) { SetLastError("The real sequence length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealSequence(sequence); *length = sequence->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_value(const OcctSharp_RealSequenceHandle* sequence, const int32_t index, double* value)
{
  if (value == nullptr) { SetLastError("The real sequence value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateRealSequence(sequence); ValidateSequenceIndex(sequence, index); *value = sequence->Value.Value(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_append(OcctSharp_RealSequenceHandle* sequence, const double value)
{
  return Guard([&] { ValidateRealSequence(sequence); ValidateFinite(value, "Sequence values must be finite."); sequence->Value.Append(value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_set_value(OcctSharp_RealSequenceHandle* sequence, const int32_t index, const double value)
{
  return Guard([&] { ValidateRealSequence(sequence); ValidateSequenceIndex(sequence, index); ValidateFinite(value, "Sequence values must be finite."); sequence->Value.SetValue(index, value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_remove(OcctSharp_RealSequenceHandle* sequence, const int32_t index)
{
  return Guard([&] { ValidateRealSequence(sequence); ValidateSequenceIndex(sequence, index); sequence->Value.Remove(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_snapshot(
  const OcctSharp_RealSequenceHandle* sequence, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real sequence snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateRealSequence(sequence);
    const int32_t length = sequence->Value.Length();
    if (capacity < length || (length > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real sequence snapshot buffer is too small or null.");
    for (int32_t index = 0; index < length; ++index) values[index] = sequence->Value.Value(index + 1);
    *written = length;
  });
}

void OCCTSHARP_CALL occtsharp_real_sequence_release(OcctSharp_RealSequenceHandle* sequence)
{ if (sequence != nullptr && UnregisterValue(sequence, LiveRealSequences)) delete sequence; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_create(
  const double* values, const int32_t count, OcctSharp_RealArrayHandle** out_array)
{
  if (out_array == nullptr) { SetLastError("The output real array pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_array = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Array count or input values are invalid.");
    NCollection_Array1<double> array(1, count);
    for (int32_t index = 0; index < count; ++index) { ValidateFinite(values[index], "Array values must be finite."); array.SetValue(index + 1, values[index]); }
    *out_array = AllocateValue(new OcctSharp_RealArrayHandle(std::move(array)), LiveRealArrays);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_clone(
  const OcctSharp_RealArrayHandle* source, OcctSharp_RealArrayHandle** out_array)
{
  if (out_array == nullptr) { SetLastError("The output real array pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_array = nullptr;
  return Guard([&] { ValidateRealArray(source); *out_array = AllocateValue(new OcctSharp_RealArrayHandle(source->Value), LiveRealArrays); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_length(const OcctSharp_RealArrayHandle* array, int32_t* length)
{
  if (length == nullptr) { SetLastError("The real array length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealArray(array); *length = array->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_lower(const OcctSharp_RealArrayHandle* array, int32_t* lower)
{
  if (lower == nullptr) { SetLastError("The real array lower-bound output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealArray(array); *lower = array->Value.Lower(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_value(const OcctSharp_RealArrayHandle* array, const int32_t index, double* value)
{
  if (value == nullptr) { SetLastError("The real array value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateRealArray(array); ValidateArrayIndex(array, index); *value = array->Value.Value(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_set_value(OcctSharp_RealArrayHandle* array, const int32_t index, const double value)
{
  return Guard([&] { ValidateRealArray(array); ValidateArrayIndex(array, index); ValidateFinite(value, "Array values must be finite."); array->Value.SetValue(index, value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_snapshot(
  const OcctSharp_RealArrayHandle* array, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real array snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateRealArray(array);
    const int32_t length = array->Value.Length();
    if (capacity < length || (length > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real array snapshot buffer is too small or null.");
    for (int32_t index = 0; index < length; ++index) values[index] = array->Value.Value(array->Value.Lower() + index);
    *written = length;
  });
}

void OCCTSHARP_CALL occtsharp_real_array_release(OcctSharp_RealArrayHandle* array)
{ if (array != nullptr && UnregisterValue(array, LiveRealArrays)) delete array; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_create(
  const double* values, const int32_t count, OcctSharp_RealVectorHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output real vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Vector count or input values are invalid.");
    NCollection_DynamicArray<double> vector;
    for (int32_t index = 0; index < count; ++index) { ValidateFinite(values[index], "Vector values must be finite."); vector.Append(values[index]); }
    *out_vector = AllocateValue(new OcctSharp_RealVectorHandle(std::move(vector)), LiveRealVectors);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_clone(
  const OcctSharp_RealVectorHandle* source, OcctSharp_RealVectorHandle** out_vector)
{
  if (out_vector == nullptr) { SetLastError("The output real vector pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_vector = nullptr;
  return Guard([&] { ValidateRealVector(source); *out_vector = AllocateValue(new OcctSharp_RealVectorHandle(source->Value), LiveRealVectors); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_length(const OcctSharp_RealVectorHandle* vector, int32_t* length)
{
  if (length == nullptr) { SetLastError("The real vector length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateRealVector(vector); *length = vector->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_value(const OcctSharp_RealVectorHandle* vector, const int32_t index, double* value)
{
  if (value == nullptr) { SetLastError("The real vector value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateRealVector(vector); ValidateVectorIndex(vector, index); *value = vector->Value.Value(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_append(OcctSharp_RealVectorHandle* vector, const double value)
{
  return Guard([&] { ValidateRealVector(vector); ValidateFinite(value, "Vector values must be finite."); vector->Value.Append(value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_set_value(OcctSharp_RealVectorHandle* vector, const int32_t index, const double value)
{
  return Guard([&] { ValidateRealVector(vector); ValidateVectorIndex(vector, index); ValidateFinite(value, "Vector values must be finite."); vector->Value.SetValue(index, value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_snapshot(
  const OcctSharp_RealVectorHandle* vector, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real vector snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateRealVector(vector);
    const int32_t length = vector->Value.Length();
    if (capacity < length || (length > 0 && values == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real vector snapshot buffer is too small or null.");
    for (int32_t index = 0; index < length; ++index) values[index] = vector->Value.Value(index);
    *written = length;
  });
}

void OCCTSHARP_CALL occtsharp_real_vector_release(OcctSharp_RealVectorHandle* vector)
{ if (vector != nullptr && UnregisterValue(vector, LiveRealVectors)) delete vector; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_create(
  const int32_t* keys, const double* values, const int32_t count, OcctSharp_IntRealMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output integer-real map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && (keys == nullptr || values == nullptr))) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Map count or input arrays are invalid.");
    NCollection_DataMap<int32_t, double> map;
    for (int32_t i = 0; i < count; ++i) { ValidateFinite(values[i], "Map values must be finite."); if (!map.Bind(keys[i], values[i])) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Map keys must be unique."); }
    *out_map = AllocateValue(new OcctSharp_IntRealMapHandle(std::move(map)), LiveIntRealMaps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_clone(const OcctSharp_IntRealMapHandle* source, OcctSharp_IntRealMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output integer-real map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&] { ValidateIntRealMap(source); *out_map = AllocateValue(new OcctSharp_IntRealMapHandle(source->Value), LiveIntRealMaps); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_extent(const OcctSharp_IntRealMapHandle* map, int32_t* extent)
{
  if (extent == nullptr) { SetLastError("The integer-real map extent output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateIntRealMap(map); *extent = map->Value.Extent(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_is_bound(const OcctSharp_IntRealMapHandle* map, const int32_t key, int32_t* is_bound)
{
  if (is_bound == nullptr) { SetLastError("The integer-real map bound output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateIntRealMap(map); *is_bound = map->Value.IsBound(key) ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_find(const OcctSharp_IntRealMapHandle* map, const int32_t key, double* value)
{
  if (value == nullptr) { SetLastError("The integer-real map value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateIntRealMap(map); if (!map->Value.Find(key, *value)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The map key is not bound."); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_bind(OcctSharp_IntRealMapHandle* map, const int32_t key, const double value)
{
  return Guard([&] { ValidateIntRealMap(map); ValidateFinite(value, "Map values must be finite."); map->Value.Bind(key, value); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_unbind(OcctSharp_IntRealMapHandle* map, const int32_t key, int32_t* removed)
{
  if (removed == nullptr) { SetLastError("The integer-real map removal output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *removed = 0;
  return Guard([&] { ValidateIntRealMap(map); *removed = map->Value.UnBind(key) ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_snapshot(
  const OcctSharp_IntRealMapHandle* map, int32_t* keys, double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The integer-real map snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateIntRealMap(map);
    const int32_t extent = map->Value.Extent();
    if (capacity < extent || (extent > 0 && (keys == nullptr || values == nullptr))) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The integer-real map snapshot buffers are too small or null.");
    int32_t index = 0;
    for (NCollection_DataMap<int32_t, double>::Iterator iterator(map->Value); iterator.More(); iterator.Next())
    {
      keys[index] = iterator.Key();
      values[index] = iterator.Value();
      ++index;
    }
    *written = extent;
  });
}

void OCCTSHARP_CALL occtsharp_int_real_map_release(OcctSharp_IntRealMapHandle* map)
{ if (map != nullptr && UnregisterValue(map, LiveIntRealMaps)) delete map; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_create(
  const int32_t* keys, const int32_t count, OcctSharp_IntIndexedMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output indexed map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&]
  {
    if (count < 0 || (count > 0 && keys == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Indexed map count or input keys are invalid.");
    NCollection_IndexedMap<int32_t> map;
    for (int32_t i = 0; i < count; ++i) { const int before = map.Extent(); const int index = map.Add(keys[i]); if (index != before + 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Indexed map keys must be unique."); }
    *out_map = AllocateValue(new OcctSharp_IntIndexedMapHandle(std::move(map)), LiveIntIndexedMaps);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_clone(const OcctSharp_IntIndexedMapHandle* source, OcctSharp_IntIndexedMapHandle** out_map)
{
  if (out_map == nullptr) { SetLastError("The output indexed map pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_map = nullptr;
  return Guard([&] { ValidateIntIndexedMap(source); *out_map = AllocateValue(new OcctSharp_IntIndexedMapHandle(source->Value), LiveIntIndexedMaps); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_extent(const OcctSharp_IntIndexedMapHandle* map, int32_t* extent)
{
  if (extent == nullptr) { SetLastError("The indexed map extent output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateIntIndexedMap(map); *extent = map->Value.Extent(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_add(OcctSharp_IntIndexedMapHandle* map, const int32_t key, int32_t* index, int32_t* added)
{
  if (index == nullptr || added == nullptr) { SetLastError("The indexed map add output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *index = 0; *added = 0;
  return Guard([&] { ValidateIntIndexedMap(map); const int before = map->Value.Extent(); *index = map->Value.Add(key); *added = *index == before + 1 ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_key(const OcctSharp_IntIndexedMapHandle* map, const int32_t index, int32_t* key)
{
  if (key == nullptr) { SetLastError("The indexed map key output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *key = 0;
  return Guard([&] { ValidateIntIndexedMap(map); if (index < 1 || index > map->Value.Extent()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Indexed map indices are 1-based and must be within the extent."); *key = map->Value.FindKey(index); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_find_index(const OcctSharp_IntIndexedMapHandle* map, const int32_t key, int32_t* index)
{
  if (index == nullptr) { SetLastError("The indexed map index output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *index = 0;
  return Guard([&] { ValidateIntIndexedMap(map); *index = map->Value.FindIndex(key); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_remove_last(OcctSharp_IntIndexedMapHandle* map, int32_t* removed_key)
{
  if (removed_key == nullptr) { SetLastError("The indexed map removed-key output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *removed_key = 0;
  return Guard([&] { ValidateIntIndexedMap(map); const int extent = map->Value.Extent(); if (extent <= 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Cannot remove the last key from an empty indexed map."); *removed_key = map->Value.FindKey(extent); map->Value.RemoveLast(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_snapshot(
  const OcctSharp_IntIndexedMapHandle* map, int32_t* keys, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The indexed map snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateIntIndexedMap(map);
    const int32_t extent = map->Value.Extent();
    if (capacity < extent || (extent > 0 && keys == nullptr)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The indexed map snapshot buffer is too small or null.");
    for (int32_t index = 0; index < extent; ++index) keys[index] = map->Value.FindKey(index + 1);
    *written = extent;
  });
}

void OCCTSHARP_CALL occtsharp_int_indexed_map_release(OcctSharp_IntIndexedMapHandle* map)
{ if (map != nullptr && UnregisterValue(map, LiveIntIndexedMaps)) delete map; }
