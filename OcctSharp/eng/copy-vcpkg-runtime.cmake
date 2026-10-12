# Resolve the bridge's actual PE import closure instead of copying a historical
# distribution's DLL count or unrelated files from the dependency prefix.
if(NOT EXISTS "${BRIDGE}" OR NOT IS_DIRECTORY "${DEPENDENCY_BIN}" OR NOT IS_DIRECTORY "${OUTPUT}")
  message(FATAL_ERROR "Runtime closure inputs are missing.")
endif()
if(NOT EXISTS "${DUMPBIN}")
  message(FATAL_ERROR "The selected MSVC dumpbin tool is missing.")
endif()
set(CMAKE_GET_RUNTIME_DEPENDENCIES_PLATFORM "windows+pe")
set(CMAKE_GET_RUNTIME_DEPENDENCIES_TOOL "dumpbin")
set(CMAKE_GET_RUNTIME_DEPENDENCIES_COMMAND "${DUMPBIN}")
file(GET_RUNTIME_DEPENDENCIES
  LIBRARIES "${BRIDGE}"
  DIRECTORIES "${DEPENDENCY_BIN}"
  RESOLVED_DEPENDENCIES_VAR resolved
  UNRESOLVED_DEPENDENCIES_VAR unresolved
  CONFLICTING_DEPENDENCIES_PREFIX conflict
  PRE_EXCLUDE_REGEXES "^[Aa][Pp][Ii]-[Mm][Ss]-" "^[Ee][Xx][Tt]-[Mm][Ss]-"
  POST_EXCLUDE_REGEXES "/[Ss][Yy][Ss][Tt][Ee][Mm]32/" "/[Ss][Yy][Ss][Ww][Oo][Ww]64/")
if(unresolved OR conflict_FILENAMES)
  message(FATAL_ERROR "Runtime closure unresolved=${unresolved}; conflicts=${conflict_FILENAMES}")
endif()
file(REAL_PATH "${DEPENDENCY_BIN}" prefix)
foreach(dll IN LISTS resolved)
  file(REAL_PATH "${dll}" source)
  cmake_path(IS_PREFIX prefix "${source}" NORMALIZE inside)
  if(NOT inside)
    message(FATAL_ERROR "Non-system runtime dependency escaped the pinned prefix: ${source}")
  endif()
  get_filename_component(name "${source}" NAME)
  file(COPY_FILE "${source}" "${OUTPUT}/${name}" ONLY_IF_DIFFERENT)
endforeach()
