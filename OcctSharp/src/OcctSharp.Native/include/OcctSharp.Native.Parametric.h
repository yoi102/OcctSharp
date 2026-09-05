#pragma once
#include "OcctSharp.Native.h"
#include "OcctSharp.Native.Authoring.h"

#ifdef __cplusplus
extern "C" {
#endif

/* Document/label arguments are borrowed for one call. Mutations require an open
   document command. Strings are UTF-8. Counts and indices are zero-based Int32. */
/* Seven finite doubles: translation XYZ, axis XYZ, radians. Exact copied topology history. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_transform(
  const OcctSharp_ShapeHandle* source, const double* values,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_function_register(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const char* driver, int32_t* id);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_function_remove(
  const OcctSharp_OcafDocumentHandle* document, const char* entry);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_function_rewire(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t* previous, int32_t count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_function_links(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t next,
  int32_t* values, int32_t capacity, int32_t* count, int32_t* id, int32_t* state);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_function_state(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t state, int32_t failure);
/* logbook operation: 0 clear, 1 touched, 2 impacted, 3 valid, 4 done, 5 read flags. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_function_logbook(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t operation, int32_t* flags);

/* Dedicated parameter labels own a NamedData attribute. kind 0 missing, 1 integer,
   2 real, 3 text, 4 integer array, 5 real array. Empty arrays have an explicit count. */
typedef struct OcctSharp_ParameterInfo {
  int32_t kind, integer_value, count, reserved;
  double real_value;
} OcctSharp_ParameterInfo;
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_parameter_set(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_ParameterInfo* info,
  const char* text, int32_t text_length, const int32_t* integers, const double* reals);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_parameter_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, OcctSharp_ParameterInfo* info,
  char* text, int32_t text_capacity, int32_t* written, int32_t* integers, double* reals, int32_t capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_text_set(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const char* key,
  const char* text, int32_t length);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_text_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const char* key,
  int32_t* found, char* text, int32_t capacity, int32_t* written);

/* Evolution kind: 0 primitive, 1 generated, 2 modified, 3 deleted. One kind per
   dedicated history label, arrays of old/new owners borrowed for this call only. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_record(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  const OcctSharp_ShapeHandle* const* old_shapes, const OcctSharp_ShapeHandle* const* new_shapes, int32_t count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_history(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t transaction, int32_t index,
  int32_t* count, int32_t* evolution, OcctSharp_ShapeHandle** old_shape, OcctSharp_ShapeHandle** new_shape);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_select(
  const OcctSharp_OcafDocumentHandle* document, const char* selector_parent, const char* context_entry,
  const OcctSharp_ShapeHandle* selection, int32_t expected_kind, int32_t* selected);
/* Resolution status: 0 resolved, 1 missing, 2 ambiguous, 3 unsupported, 4 wrong
   topology type, 5 deleted. Only resolved returns an owning shape. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_resolve(
  const OcctSharp_OcafDocumentHandle* document, const char* selector_parent,
  int32_t expected_kind, int32_t* status, OcctSharp_ShapeHandle** shape);
/* Copy root pairs within one document. Roots/descendants must not overlap;
   external references are retained only when retain_external is explicitly 1. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_relocate(
  const OcctSharp_OcafDocumentHandle* document, const char* const* sources,
  const char* const* destinations, int32_t count, int32_t retain_external);

#ifdef __cplusplus
}
#endif
