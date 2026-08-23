#pragma once

#include <stdint.h>

#if defined(_WIN32)
  #if defined(OCCTSHARP_NATIVE_EXPORTS)
    #define OCCTSHARP_API __declspec(dllexport)
  #else
    #define OCCTSHARP_API __declspec(dllimport)
  #endif
  #define OCCTSHARP_CALL __cdecl
#else
  #define OCCTSHARP_API __attribute__((visibility("default")))
  #define OCCTSHARP_CALL
#endif
#ifdef __cplusplus
extern "C" {
#endif

typedef enum OcctSharp_Status
{
  OCCTSHARP_STATUS_SUCCESS = 0,
  OCCTSHARP_STATUS_INVALID_ARGUMENT = 1,
  OCCTSHARP_STATUS_NULL_HANDLE = 2,
  OCCTSHARP_STATUS_OCCT_FAILURE = 3,
  OCCTSHARP_STATUS_STANDARD_EXCEPTION = 4,
  OCCTSHARP_STATUS_UNKNOWN_EXCEPTION = 5,
  OCCTSHARP_STATUS_FILE_IO_ERROR = 6,
  OCCTSHARP_STATUS_TRANSFER_FAILED = 7,
  OCCTSHARP_STATUS_INVALID_HANDLE = 8,
  OCCTSHARP_STATUS_TYPE_MISMATCH = 9
} OcctSharp_Status;

typedef struct OcctSharp_ShapeHandle OcctSharp_ShapeHandle;
typedef struct OcctSharp_TransientHandle OcctSharp_TransientHandle;
typedef struct OcctSharp_TrsfHandle OcctSharp_TrsfHandle;
typedef struct OcctSharp_LocationHandle OcctSharp_LocationHandle;
typedef struct OcctSharp_VecHandle OcctSharp_VecHandle;
typedef struct OcctSharp_DirHandle OcctSharp_DirHandle;
typedef struct OcctSharp_Ax1Handle OcctSharp_Ax1Handle;
typedef struct OcctSharp_MatHandle OcctSharp_MatHandle;
typedef struct OcctSharp_AsciiStringHandle OcctSharp_AsciiStringHandle;
typedef struct OcctSharp_ExtendedStringHandle OcctSharp_ExtendedStringHandle;
typedef struct OcctSharp_RealSequenceHandle OcctSharp_RealSequenceHandle;
typedef struct OcctSharp_RealArrayHandle OcctSharp_RealArrayHandle;
typedef struct OcctSharp_RealVectorHandle OcctSharp_RealVectorHandle;
typedef struct OcctSharp_IntRealMapHandle OcctSharp_IntRealMapHandle;
typedef struct OcctSharp_IntIndexedMapHandle OcctSharp_IntIndexedMapHandle;

typedef struct OcctSharp_StepAssemblyInput
{
  const char* file_path;
  double translation_x;
  double translation_y;
  double translation_z;
  double rotation_axis_x;
  double rotation_axis_y;
  double rotation_axis_z;
  double rotation_angle_radians;
} OcctSharp_StepAssemblyInput;

OCCTSHARP_API uint32_t OCCTSHARP_CALL occtsharp_get_abi_version(void);
OCCTSHARP_API const char* OCCTSHARP_CALL occtsharp_get_bridge_version(void);
OCCTSHARP_API const char* OCCTSHARP_CALL occtsharp_get_occt_version(void);
OCCTSHARP_API const char* OCCTSHARP_CALL occtsharp_get_last_error(void);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_box(
  double size_x,
  double size_y,
  double size_z,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_get_face_count(
  const OcctSharp_ShapeHandle* shape,
  int32_t* out_face_count);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_read_step(
  const char* file_path,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_step(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_stl(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path,
  double linear_deflection,
  double angular_deflection,
  int32_t binary);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_write_iges(
  const OcctSharp_ShapeHandle* shape,
  const char* file_path);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform(
  const OcctSharp_ShapeHandle* shape,
  double translation_x,
  double translation_y,
  double translation_z,
  double rotation_axis_x,
  double rotation_axis_y,
  double rotation_axis_z,
  double rotation_angle_radians,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_identity(
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_rotation(
  double translation_x,
  double translation_y,
  double translation_z,
  double rotation_axis_x,
  double rotation_axis_y,
  double rotation_axis_z,
  double rotation_angle_radians,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_clone(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_inverted(
  const OcctSharp_TrsfHandle* source,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_multiplied(
  const OcctSharp_TrsfHandle* left,
  const OcctSharp_TrsfHandle* right,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_value(
  const OcctSharp_TrsfHandle* transform,
  int32_t row,
  int32_t column,
  double* out_value);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_trsf_release(OcctSharp_TrsfHandle* transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_transform_trsf(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_identity(
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_create_from_trsf(
  const OcctSharp_TrsfHandle* transform,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_clone(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_inverted(
  const OcctSharp_LocationHandle* source,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_multiplied(
  const OcctSharp_LocationHandle* left,
  const OcctSharp_LocationHandle* right,
  OcctSharp_LocationHandle** out_location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_is_identity(
  const OcctSharp_LocationHandle* location,
  int32_t* out_is_identity);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_location_to_trsf(
  const OcctSharp_LocationHandle* location,
  OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_location_release(OcctSharp_LocationHandle* location);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_located(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_LocationHandle* location,
  int32_t moved,
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_create(
  double x, double y, double z, OcctSharp_VecHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_clone(
  const OcctSharp_VecHandle* source, OcctSharp_VecHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_components(
  const OcctSharp_VecHandle* vector, double* x, double* y, double* z);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_magnitude(
  const OcctSharp_VecHandle* vector, double* magnitude);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_dot(
  const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, double* dot);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_vec_crossed(
  const OcctSharp_VecHandle* left, const OcctSharp_VecHandle* right, OcctSharp_VecHandle** result);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_vec_release(OcctSharp_VecHandle* vector);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_create(
  double x, double y, double z, OcctSharp_DirHandle** out_direction);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_clone(
  const OcctSharp_DirHandle* source, OcctSharp_DirHandle** out_direction);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_components(
  const OcctSharp_DirHandle* direction, double* x, double* y, double* z);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_dot(
  const OcctSharp_DirHandle* left, const OcctSharp_DirHandle* right, double* dot);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_dir_reversed(
  const OcctSharp_DirHandle* source, OcctSharp_DirHandle** result);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_dir_release(OcctSharp_DirHandle* direction);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_create(
  double origin_x, double origin_y, double origin_z,
  double direction_x, double direction_y, double direction_z,
  OcctSharp_Ax1Handle** out_axis);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_clone(
  const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** out_axis);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_components(
  const OcctSharp_Ax1Handle* axis,
  double* origin_x, double* origin_y, double* origin_z,
  double* direction_x, double* direction_y, double* direction_z);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ax1_reversed(
  const OcctSharp_Ax1Handle* source, OcctSharp_Ax1Handle** result);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_ax1_release(OcctSharp_Ax1Handle* axis);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_create(
  const double* values, OcctSharp_MatHandle** out_matrix);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_identity(
  OcctSharp_MatHandle** out_matrix);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_clone(
  const OcctSharp_MatHandle* source, OcctSharp_MatHandle** out_matrix);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_value(
  const OcctSharp_MatHandle* matrix, int32_t row, int32_t column, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mat_determinant(
  const OcctSharp_MatHandle* matrix, double* determinant);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_mat_release(OcctSharp_MatHandle* matrix);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_translation_vec(
  const OcctSharp_VecHandle* vector, OcctSharp_TrsfHandle** out_transform);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_trsf_create_rotation_axis(
  const OcctSharp_Ax1Handle* axis, double angle_radians, OcctSharp_TrsfHandle** out_transform);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_create(
  const char* utf8, int32_t length, OcctSharp_AsciiStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_clone(
  const OcctSharp_AsciiStringHandle* source, OcctSharp_AsciiStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_length(
  const OcctSharp_AsciiStringHandle* string, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_append(
  OcctSharp_AsciiStringHandle* string, const char* utf8, int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_utf8(
  const OcctSharp_AsciiStringHandle* string, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_extended(
  const OcctSharp_AsciiStringHandle* string, OcctSharp_ExtendedStringHandle** out_string);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_ascii_release(OcctSharp_AsciiStringHandle* string);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_create_utf8(
  const char* utf8, int32_t length, OcctSharp_ExtendedStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_clone(
  const OcctSharp_ExtendedStringHandle* source, OcctSharp_ExtendedStringHandle** out_string);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_length(
  const OcctSharp_ExtendedStringHandle* string, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_utf8_length(
  const OcctSharp_ExtendedStringHandle* string, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_append_utf8(
  OcctSharp_ExtendedStringHandle* string, const char* utf8, int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_utf8(
  const OcctSharp_ExtendedStringHandle* string, char* buffer, int32_t capacity, int32_t* written);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_value(
  const OcctSharp_ExtendedStringHandle* string, int32_t index, uint16_t* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_ascii(
  const OcctSharp_ExtendedStringHandle* string, OcctSharp_AsciiStringHandle** out_string);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_extended_release(OcctSharp_ExtendedStringHandle* string);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_create(
  const double* values, int32_t count, OcctSharp_RealSequenceHandle** out_sequence);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_clone(
  const OcctSharp_RealSequenceHandle* source, OcctSharp_RealSequenceHandle** out_sequence);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_length(
  const OcctSharp_RealSequenceHandle* sequence, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_value(
  const OcctSharp_RealSequenceHandle* sequence, int32_t index, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_append(
  OcctSharp_RealSequenceHandle* sequence, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_set_value(
  OcctSharp_RealSequenceHandle* sequence, int32_t index, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_sequence_remove(
  OcctSharp_RealSequenceHandle* sequence, int32_t index);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_real_sequence_release(OcctSharp_RealSequenceHandle* sequence);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_create(
  const double* values, int32_t count, OcctSharp_RealArrayHandle** out_array);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_clone(
  const OcctSharp_RealArrayHandle* source, OcctSharp_RealArrayHandle** out_array);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_length(
  const OcctSharp_RealArrayHandle* array, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_lower(
  const OcctSharp_RealArrayHandle* array, int32_t* lower);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_value(
  const OcctSharp_RealArrayHandle* array, int32_t index, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_array_set_value(
  OcctSharp_RealArrayHandle* array, int32_t index, double value);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_real_array_release(OcctSharp_RealArrayHandle* array);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_create(
  const double* values, int32_t count, OcctSharp_RealVectorHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_clone(
  const OcctSharp_RealVectorHandle* source, OcctSharp_RealVectorHandle** out_vector);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_length(
  const OcctSharp_RealVectorHandle* vector, int32_t* length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_value(
  const OcctSharp_RealVectorHandle* vector, int32_t index, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_append(
  OcctSharp_RealVectorHandle* vector, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_real_vector_set_value(
  OcctSharp_RealVectorHandle* vector, int32_t index, double value);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_real_vector_release(OcctSharp_RealVectorHandle* vector);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_create(
  const int32_t* keys, const double* values, int32_t count, OcctSharp_IntRealMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_clone(
  const OcctSharp_IntRealMapHandle* source, OcctSharp_IntRealMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_extent(
  const OcctSharp_IntRealMapHandle* map, int32_t* extent);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_is_bound(
  const OcctSharp_IntRealMapHandle* map, int32_t key, int32_t* is_bound);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_find(
  const OcctSharp_IntRealMapHandle* map, int32_t key, double* value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_bind(
  OcctSharp_IntRealMapHandle* map, int32_t key, double value);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_real_map_unbind(
  OcctSharp_IntRealMapHandle* map, int32_t key, int32_t* removed);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_int_real_map_release(OcctSharp_IntRealMapHandle* map);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_create(
  const int32_t* keys, int32_t count, OcctSharp_IntIndexedMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_clone(
  const OcctSharp_IntIndexedMapHandle* source, OcctSharp_IntIndexedMapHandle** out_map);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_extent(
  const OcctSharp_IntIndexedMapHandle* map, int32_t* extent);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_add(
  OcctSharp_IntIndexedMapHandle* map, int32_t key, int32_t* index, int32_t* added);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_key(
  const OcctSharp_IntIndexedMapHandle* map, int32_t index, int32_t* key);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_find_index(
  const OcctSharp_IntIndexedMapHandle* map, int32_t key, int32_t* index);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_int_indexed_map_remove_last(
  OcctSharp_IntIndexedMapHandle* map, int32_t* removed_key);
OCCTSHARP_API void OCCTSHARP_CALL occtsharp_int_indexed_map_release(OcctSharp_IntIndexedMapHandle* map);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_compound(
  OcctSharp_ShapeHandle** out_shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_compound_add(
  OcctSharp_ShapeHandle* compound,
  const OcctSharp_ShapeHandle* child);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_step_merge_xde(
  const OcctSharp_StepAssemblyInput* inputs,
  int32_t input_count,
  const char* output_path);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_shape_release(OcctSharp_ShapeHandle* shape);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create(
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_null(
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_create_derived(
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_clone(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_try_cast_derived(
  const OcctSharp_TransientHandle* source,
  OcctSharp_TransientHandle** out_handle);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_null(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_is_null);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_ref_count(
  const OcctSharp_TransientHandle* handle,
  int32_t* out_ref_count);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_get_type_name(
  const OcctSharp_TransientHandle* handle,
  const char** out_type_name);

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_transient_is_kind(
  const OcctSharp_TransientHandle* handle,
  const char* type_name,
  int32_t* out_is_kind);

OCCTSHARP_API void OCCTSHARP_CALL occtsharp_transient_release(OcctSharp_TransientHandle* handle);

#ifdef __cplusplus
}
#endif
