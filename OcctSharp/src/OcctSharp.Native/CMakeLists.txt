add_library(OcctSharp.Native SHARED
  include/OcctSharp.Native.h
  include/OcctSharp.Native.Internal.hxx
  generated/OcctSharp.Generated.h
  generated/OcctSharp.Generated.cpp
  generated/OcctSharp.SharedHandles.Generated.h
  generated/OcctSharp.SharedHandles.Generated.cpp
  generated/Topology/OcctSharp.Topology.Generated.h
  generated/Topology/OcctSharp.Topology.Generated.cpp
  src/OcctSharp.Native.cpp)

target_compile_features(OcctSharp.Native PRIVATE cxx_std_20)
target_compile_definitions(OcctSharp.Native PRIVATE OCCTSHARP_NATIVE_EXPORTS NOMINMAX WIN32_LEAN_AND_MEAN)
if(MSVC)
  target_compile_options(OcctSharp.Native PRIVATE /bigobj)
endif()
target_include_directories(OcctSharp.Native
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
