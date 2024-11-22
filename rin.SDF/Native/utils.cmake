include(${CMAKE_CURRENT_LIST_DIR}/../../build.cmake)

# MsdfGen
macro(GetMsdfGen SPECIFIC_PROJECT VERSION)
  function(BuildMsdfGen B_TYPE B_SRC B_DEST)
    execute_process(#
      COMMAND ${CMAKE_COMMAND} -DCMAKE_BUILD_TYPE=${B_TYPE} -DMSDFGEN_DYNAMIC_RUNTIME=ON -DBUILD_SHARED_LIBS=ON -DMSDFGEN_USE_SKIA=OFF -DMSDFGEN_INSTALL=ON -DMSDFGEN_CORE_ONLY=ON -DMSDFGEN_BUILD_STANDALONE=OFF -DMSDFGEN_USE_VCPKG=OFF -S ${B_SRC} -B ${B_DEST}
    )
  endfunction()

  BuildThirdPartyDep(msdfgen ${CMAKE_CURRENT_LIST_DIR}/../../ext https://github.com/Chlumsky/msdfgen ${VERSION} RESULT_DIR "" "BuildMsdfGen")

  # list(APPEND CMAKE_PREFIX_PATH ${RESULT_DIR}/lib/cmake)

  find_package(msdfgen REQUIRED PATHS ${RESULT_DIR}/lib/cmake)

  target_include_directories(
    ${SPECIFIC_PROJECT}
    PUBLIC
    $<BUILD_INTERFACE:${RESULT_DIR}/include>
    $<INSTALL_INTERFACE:include>
  )
  
  target_link_libraries(${SPECIFIC_PROJECT} PUBLIC msdfgen::msdfgen)
endmacro()

macro(SetDynamicLibraryDir SPECIFIC_TARGET)
  set(TARGET_DIR ${CMAKE_CURRENT_LIST_DIR}/bin/${CMAKE_BUILD_TYPE})
  set_target_properties(${SPECIFIC_TARGET}
      PROPERTIES
      LIBRARY_OUTPUT_DIRECTORY  ${TARGET_DIR}
      RUNTIME_OUTPUT_DIRECTORY ${TARGET_DIR}
      LIBRARY_OUTPUT_DIRECTORY_DEBUG  ${TARGET_DIR}
      RUNTIME_OUTPUT_DIRECTORY_DEBUG ${TARGET_DIR}
      LIBRARY_OUTPUT_DIRECTORY_RELEASE  ${TARGET_DIR}
      RUNTIME_OUTPUT_DIRECTORY_RELEASE ${TARGET_DIR}
  )
  message(STATUS ${TARGET_DIR})
endmacro()


