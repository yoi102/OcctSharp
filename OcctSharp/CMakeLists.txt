cmake_minimum_required(VERSION 3.30)

project(OcctSharpNativeWorkspace LANGUAGES CXX)

if(NOT WIN32 OR NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
  message(FATAL_ERROR "The initial OcctSharp baseline supports Windows x64 only.")
endif()

if(NOT DEFINED OCCT_ROOT OR OCCT_ROOT STREQUAL "")
  message(FATAL_ERROR "OCCT_ROOT must point to the opencascade-8.0.1-vc14-64 directory.")
endif()

cmake_path(ABSOLUTE_PATH OCCT_ROOT NORMALIZE OUTPUT_VARIABLE OCCT_ROOT_NORMALIZED)
get_filename_component(OCCT_BUNDLE_ROOT "${OCCT_ROOT_NORMALIZED}" DIRECTORY)

if(NOT DEFINED OCCT_THIRDPARTY_ROOT OR OCCT_THIRDPARTY_ROOT STREQUAL "")
  set(OCCT_THIRDPARTY_ROOT "${OCCT_BUNDLE_ROOT}/3rdparty-vc14-64")
endif()

cmake_path(ABSOLUTE_PATH OCCT_THIRDPARTY_ROOT NORMALIZE OUTPUT_VARIABLE OCCT_THIRDPARTY_ROOT_NORMALIZED)

if(NOT EXISTS "${OCCT_ROOT_NORMALIZED}/cmake/OpenCASCADEConfig.cmake")
  message(FATAL_ERROR "OpenCASCADEConfig.cmake was not found under '${OCCT_ROOT_NORMALIZED}/cmake'.")
endif()

if(NOT EXISTS "${OCCT_THIRDPARTY_ROOT_NORMALIZED}/tbb-2021.13.0-x64/bin/tbb12.dll")
  message(FATAL_ERROR "The expected OCCT third-party runtime was not found under '${OCCT_THIRDPARTY_ROOT_NORMALIZED}'.")
endif()

find_package(
  OpenCASCADE 8.0.1 EXACT REQUIRED
  COMPONENTS FoundationClasses ModelingData ModelingAlgorithms Visualization ApplicationFramework DataExchange
  PATHS "${OCCT_ROOT_NORMALIZED}/cmake"
  NO_DEFAULT_PATH)

set(OCCTSHARP_NATIVE_OUTPUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/artifacts/native")

add_subdirectory(src/OcctSharp.Native)
