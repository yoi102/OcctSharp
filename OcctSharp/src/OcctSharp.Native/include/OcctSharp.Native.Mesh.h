#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif

/* Copied input/output only. flags: 1 defined normal, 2 UV channel, 4 normal channel.
   Channels are absent or full-cardinality. All indices are zero-based. */
typedef struct OcctSharp_AuthoredVertex {
  double x, y, z, nx, ny, nz, u, v;
  int32_t flags, reserved;
} OcctSharp_AuthoredVertex;
typedef struct OcctSharp_AuthoredTriangle { int32_t a, b, c, group; } OcctSharp_AuthoredTriangle;

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_author_face(
  const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangle_count, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_existing_snapshot(
  const OcctSharp_ShapeHandle* shape, OcctSharp_AuthoredVertex* vertices, int32_t vertex_capacity, int32_t* vertex_count,
  OcctSharp_AuthoredTriangle* triangles, int32_t triangle_capacity, int32_t* triangle_count, int32_t* face_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_replace_face(
  const OcctSharp_ShapeHandle* source, int32_t face_index, const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangle_count, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_remesh_faces(
  const OcctSharp_ShapeHandle* source, const int32_t* faces, int32_t face_count,
  double linear_deflection, double angular_deflection, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_is_exact(const OcctSharp_ShapeHandle* source, int32_t* exact);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_copy_shape(const OcctSharp_ShapeHandle* source, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_transform(
  const OcctSharp_AuthoredVertex* vertices, int32_t count, const double* matrix, int32_t matrix_count,
  OcctSharp_AuthoredVertex* output, int32_t capacity, double* determinant);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_weld_nodes(
  const OcctSharp_AuthoredVertex* vertices, int32_t count, const int32_t* partitions, int32_t partition_count,
  double tolerance, int32_t* representatives, int32_t capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_coherent_patch(
  const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangle_count,
  const int32_t* replaced, const OcctSharp_AuthoredTriangle* replacements, int32_t replacement_count,
  const OcctSharp_AuthoredTriangle* appended, int32_t appended_count,
  OcctSharp_AuthoredTriangle* output, int32_t capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_remove_degenerate(
  const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangle_count, double minimum_area, double minimum_length,
  int32_t* removed, int32_t capacity, int32_t* removed_count);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_poly_connect(
  const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count,
  const OcctSharp_AuthoredTriangle* triangles, int32_t triangle_count, int32_t* neighbors, int32_t capacity);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_polyline(
  const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count, const int32_t* indices, int32_t index_count,
  const double* parameters, int32_t parameter_count, OcctSharp_ShapeHandle** output);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_read_editable(
  const char* path, int32_t format, int64_t maximum_bytes, OcctSharp_ShapeHandle** output, int32_t* disclosures);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_write_stl(
  const OcctSharp_AuthoredVertex* vertices, int32_t vertex_count, const OcctSharp_AuthoredTriangle* triangles,
  int32_t triangle_count, const char* path, int32_t binary);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_convert_coordinates(
  const OcctSharp_AuthoredVertex* vertices, int32_t count, double source_unit, int32_t source_up, int32_t source_left,
  double target_unit, int32_t target_up, int32_t target_left, OcctSharp_AuthoredVertex* output, int32_t capacity);
/* Document geometry is canonical millimetres, right-handed Z-up. No mesher is invoked.
   channels: 1 complete defined normals, 2 complete UVs; controls optional PLY fields. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_mesh_write_document(
  const OcctSharp_OcafDocumentHandle* document, const char* path, int32_t format, int32_t binary, int32_t channels);

#ifdef __cplusplus
}
#endif
