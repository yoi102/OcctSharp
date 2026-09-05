file(GLOB_RECURSE OCCTSHARP_GENERATED_NATIVE_SOURCES CONFIGURE_DEPENDS
  "${CMAKE_CURRENT_SOURCE_DIR}/generated/*.cpp"
  "${CMAKE_CURRENT_SOURCE_DIR}/generated/*.h"
  "${CMAKE_CURRENT_SOURCE_DIR}/generated/*.hxx")

set(OCCTSHARP_MANUAL_NATIVE_SOURCES
  src/Runtime/Abi.cpp
  src/Runtime/Error.cpp
  src/Runtime/Registry.cpp
  src/Runtime/Shape.cpp
  src/Runtime/Validation.cpp
  src/Foundation/Collections.cpp
  src/Foundation/Text.cpp
  src/Foundation/Transient.cpp
  src/Geometry/Conversions.cpp
  src/Geometry/Transforms.cpp
  src/Geometry/Values.cpp
  src/Modeling/Construction.cpp
  src/Modeling/Drawing.cpp
  src/Modeling/Features.cpp
  src/Modeling/Freeform.cpp
  src/Modeling/GeometryInspection.cpp
  src/Modeling/Inspection.cpp
  src/Modeling/Operations.cpp
  src/Modeling/Properties.cpp
  src/Modeling/RepairData.cpp
  src/Modeling/RepairDiagnostics.cpp
  src/Modeling/RepairBoundaries.cpp
  src/Modeling/RepairFixers.cpp
  src/Modeling/RepairExecution.cpp
  src/Modeling/RepairSewing.cpp
  src/Modeling/TopologyNormalization.cpp
  src/Modeling/Sketch.cpp
  src/Modeling/Topology.cpp
  src/Mesh/Triangulation.cpp
  src/Mesh/MeshAuthoring.cpp
  src/Mesh/MeshEditing.cpp
  src/Mesh/MeshTopology.cpp
  src/Documents/Lifecycle.cpp
  src/Documents/State.cpp
  src/Xde/Document.cpp
  src/Xde/Metadata.cpp
  src/Xde/Pmi.cpp
  src/Xde/SavedViews.cpp
  src/Xde/Structure.cpp
  src/Xde/RepairPublication.cpp
  src/Exchange/ShapeExchange.cpp
  src/Exchange/MeshExchange.cpp
  src/Exchange/StepAssembly.cpp
  src/Exchange/XdeExchange.cpp
  src/Visualization/Clipping.cpp
  src/Visualization/Context.cpp
  src/Visualization/Dimensions.cpp
  src/Visualization/Manipulators.cpp
  src/Visualization/Navigation.cpp
  src/Visualization/Presentations.cpp
  src/Visualization/Selection.cpp
  src/Visualization/RepairReview.cpp
  src/Surfaces/SurfaceInspection.cpp
  src/Surfaces/SurfaceCurves.cpp
  src/Surfaces/SurfaceTopology.cpp)

# Prove each manual responsibility declares its own dependencies. Generated shards
# retain their existing PCH for build cost, but manual files cannot depend on it.
set_source_files_properties(${OCCTSHARP_MANUAL_NATIVE_SOURCES}
  PROPERTIES SKIP_PRECOMPILE_HEADERS ON)

add_library(OcctSharp.Native SHARED
  include/OcctSharp.Native.h
  include/OcctSharp.Native.Internal.hxx
  include/OcctSharp.Native.Surface.h
  include/OcctSharp.Native.Repair.h
  include/OcctSharp.Native.Mesh.h
  src/Surfaces/SurfaceCommon.hxx
  ${OCCTSHARP_MANUAL_NATIVE_SOURCES}
  ${OCCTSHARP_GENERATED_NATIVE_SOURCES})

target_compile_features(OcctSharp.Native PRIVATE cxx_std_20)
target_compile_definitions(OcctSharp.Native PRIVATE OCCTSHARP_NATIVE_EXPORTS NOMINMAX WIN32_LEAN_AND_MEAN)
if(MSVC)
  target_compile_options(OcctSharp.Native PRIVATE /bigobj)
endif()
target_include_directories(OcctSharp.Native
  PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/src"
  PUBLIC
    "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>")

set(OCCTSHARP_CORE_TOOLKITS
  # FoundationClasses
  TKernel
  TKMath
  # ModelingData
  TKG2d
  TKG3d
  TKGeomBase
  TKBRep
  # ModelingAlgorithms
  TKGeomAlgo
  TKTopAlgo
  TKPrim
  TKBO
  TKShHealing
  TKBool
  TKHLR
  TKHelix
  TKFillet
  TKOffset
  TKFeat
  TKMesh
  TKXMesh
  TKExpress
  # Supported Windows visualization closure. IVtk and OpenGL ES are isolated
  # optional profiles and Draw/test toolkits are intentionally absent.
  TKService
  TKV3d
  TKOpenGl
  TKMeshVS
  TKD3DHost
  # ApplicationFramework
  TKCDF
  TKLCAF
  TKCAF
  TKBinL
  TKXmlL
  TKBin
  TKXml
  TKStdL
  TKStd
  TKTObj
  TKBinTObj
  TKXmlTObj
  TKVCAF
  # DataExchange
  TKDE
  TKXSBase
  TKDESTEP
  TKXCAF
  TKDEIGES
  TKDESTL
  TKDEVRML
  TKRWMesh
  TKDECascade
  TKBinXCAF
  TKXmlXCAF
  TKDEOBJ
  TKDEGLTF
  TKDEPLY)

target_link_libraries(OcctSharp.Native PRIVATE ${OCCTSHARP_CORE_TOOLKITS})

set_target_properties(OcctSharp.Native PROPERTIES
  UNITY_BUILD OFF
  OUTPUT_NAME "OcctSharp.Native"
  RUNTIME_OUTPUT_DIRECTORY "${OCCTSHARP_NATIVE_OUTPUT_DIR}/$<CONFIG>"
  LIBRARY_OUTPUT_DIRECTORY "${OCCTSHARP_NATIVE_OUTPUT_DIR}/$<CONFIG>"
  ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/lib/$<CONFIG>")

set(OCCTSHARP_RUNTIME_TARGETS ${OCCTSHARP_CORE_TOOLKITS})

foreach(OCCT_TARGET IN LISTS OCCTSHARP_RUNTIME_TARGETS)
  add_custom_command(TARGET OcctSharp.Native POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E copy_if_different
      "$<TARGET_FILE:${OCCT_TARGET}>"
      "$<TARGET_FILE_DIR:OcctSharp.Native>"
    VERBATIM)
endforeach()

set(OCCTSHARP_THIRDPARTY_RUNTIME_FILES
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/ffmpeg-3.3.4-64/bin/avcodec-57.dll"
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/ffmpeg-3.3.4-64/bin/avformat-57.dll"
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/ffmpeg-3.3.4-64/bin/avutil-55.dll"
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/ffmpeg-3.3.4-64/bin/swscale-4.dll"
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/freeimage-3.18.0-x64/bin/FreeImage.dll"
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/freetype-2.13.3-x64/bin/freetype.dll"
  "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/openvr-1.14.15-64/bin/win64/openvr_api.dll")

foreach(OCCTSHARP_RUNTIME_FILE IN LISTS OCCTSHARP_THIRDPARTY_RUNTIME_FILES)
  if(NOT EXISTS "${OCCTSHARP_RUNTIME_FILE}")
    message(FATAL_ERROR "Required OCCT third-party runtime was not found: '${OCCTSHARP_RUNTIME_FILE}'.")
  endif()
  add_custom_command(TARGET OcctSharp.Native POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E copy_if_different
      "${OCCTSHARP_RUNTIME_FILE}"
      "$<TARGET_FILE_DIR:OcctSharp.Native>"
    VERBATIM)
endforeach()

add_custom_command(TARGET OcctSharp.Native POST_BUILD
  COMMAND "${CMAKE_COMMAND}" -E copy_if_different
    "$<IF:$<CONFIG:Debug>,${OCCT_THIRDPARTY_ROOT_NORMALIZED}/tbb-2021.13.0-x64/bin/tbb12_debug.dll,${OCCT_THIRDPARTY_ROOT_NORMALIZED}/tbb-2021.13.0-x64/bin/tbb12.dll>"
    "$<TARGET_FILE_DIR:OcctSharp.Native>"
  COMMAND "${CMAKE_COMMAND}" -E copy_if_different
    "$<IF:$<CONFIG:Debug>,${OCCT_THIRDPARTY_ROOT_NORMALIZED}/jemalloc-vc14-64/debug/bin/jemalloc.dll,${OCCT_THIRDPARTY_ROOT_NORMALIZED}/jemalloc-vc14-64/bin/jemalloc.dll>"
    "$<TARGET_FILE_DIR:OcctSharp.Native>"
  VERBATIM)
