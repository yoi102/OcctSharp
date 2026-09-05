#pragma once
#include "OcctSharp.Native.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct OcctSharp_RenderCaps {
  int32_t max_lights, max_texture, max_dump_x, max_dump_y, max_texture_units, max_msaa;
  int32_t pbr, raytracing, srgb, oit, oit_msaa, reserved;
  double max_anisotropy;
} OcctSharp_RenderCaps;
typedef struct OcctSharp_RenderProfile {
  int32_t mode, msaa, transparency, tone_mapping;
  double resolution_scale, oit_depth_factor, exposure, white_point;
  int32_t environment_power, environment_levels, diffuse_samples, specular_samples;
  double bake_probability;
  int32_t shading, reserved;
} OcctSharp_RenderProfile;
typedef struct OcctSharp_Light {
  int64_t id;
  int32_t kind, active, headlight, reserved;
  double red, green, blue, intensity;
  double x, y, z, dx, dy, dz, constant_attenuation, linear_attenuation, range, angle, concentration;
} OcctSharp_Light;
typedef struct OcctSharp_PixelInput {
  int32_t width, height, stride, format, bottom_up, reserved;
} OcctSharp_PixelInput;
typedef struct OcctSharp_ReviewMaterial {
  double red, green, blue, alpha, metallic, roughness, ior, emission;
} OcctSharp_ReviewMaterial;
typedef struct OcctSharp_Appearance {
  OcctSharp_ReviewMaterial front, back;
  int32_t shading, distinguish, culling, alpha_mode;
  double alpha_cutoff;
  int64_t texture;
  int32_t planar, repeat, filter, anisotropy;
  double scale_s, scale_t, translate_s, translate_t, rotation;
  double plane_s[4], plane_t[4];
} OcctSharp_Appearance;
typedef struct OcctSharp_ReviewLayer {
  int32_t depth_test, depth_write, clear_depth, immediate;
} OcctSharp_ReviewLayer;
typedef struct OcctSharp_FrameRequest {
  int32_t width, height, depth, tile_size, adjust_aspect, single_layer;
  int64_t layer;
} OcctSharp_FrameRequest;
typedef struct OcctSharp_FrameInfo {
  int32_t width, height, stride, zero_to_one_depth;
  double near_plane, far_plane;
  double inverse_view_projection[16];
} OcctSharp_FrameInfo;
typedef struct OcctSharp_ReviewCamera {
  double eye[3], target[3], up[3];
  double aspect, scale, fov_y, near_plane, far_plane;
  int32_t perspective, auto_depth;
} OcctSharp_ReviewCamera;

OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_render_caps(OcctSharp_ViewerHandle*, OcctSharp_RenderCaps*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_review_camera(OcctSharp_ViewerHandle*, const OcctSharp_ReviewCamera*, OcctSharp_ReviewCamera*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_render_profile(OcctSharp_ViewerHandle*, const OcctSharp_RenderProfile*, OcctSharp_RenderProfile*);
/* id=0 stages a fresh light; an existing id updates that light. Omitted ids retire. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_lights_replace(OcctSharp_ViewerHandle*, const OcctSharp_Light*, int32_t, int64_t*, int32_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_lights_snapshot(OcctSharp_ViewerHandle*, OcctSharp_Light*, int32_t, int32_t*);
/* id=0 creates; replacement keeps the texture identity and every presentation binding. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_texture_pixels(OcctSharp_ViewerHandle*, int64_t, const OcctSharp_PixelInput*, const uint8_t*, int32_t, int64_t*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_texture_file(OcctSharp_ViewerHandle*, const char*, int64_t*, OcctSharp_PixelInput*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_texture_remove(OcctSharp_ViewerHandle*, int64_t);
/* Null profile resets the saved effective shading aspects, including custom XDE styles. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_appearance(OcctSharp_ViewerHandle*, int64_t, const OcctSharp_Appearance*);
/* Six image ids in +X,-X,+Y,-Y,+Z,-Z order, or one packed image and six tile indices. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_environment_create(OcctSharp_ViewerHandle*, const int64_t*, int32_t, const int32_t*, int64_t*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_environment_set(OcctSharp_ViewerHandle*, int64_t, int32_t, int32_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_environment_remove(OcctSharp_ViewerHandle*, int64_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_layer_set(OcctSharp_ViewerHandle*, int64_t, const OcctSharp_ReviewLayer*, int64_t*);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_layer_assign(OcctSharp_ViewerHandle*, int64_t, int64_t);
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_layer_remove(OcctSharp_ViewerHandle*, int64_t);
/* One render into caller-owned bytes; no native frame owner or borrowed GPU resource. */
OCCTSHARP_API OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_frame_capture(OcctSharp_ViewerHandle*, const OcctSharp_FrameRequest*, uint8_t*, int32_t, OcctSharp_FrameInfo*);

#ifdef __cplusplus
}
#endif
