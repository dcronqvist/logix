cmake_minimum_required(VERSION 3.10)
project(LogiX VERSION 0.1.0)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
    link_directories(libs/raylib-cpp/win)
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    link_directories(libs/raylib-cpp/osx)
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
    link_directories(libs/raylib-cpp/linux)
endif()

include_directories(libs)
include_directories(include)

file(GLOB_RECURSE SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp)

add_executable(LogiX ${SOURCES})

target_link_libraries(LogiX raylib)

if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
    target_link_libraries(LogiX winmm)
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    target_link_libraries(LogiX
        "-framework CoreVideo"
        "-framework IOKit"
        "-framework Cocoa"
        "-framework GLUT"
        "-framework OpenGL"
    )
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
    target_link_libraries(LogiX
        "-lGL"
        "-lm"
        "-lpthread"
        "-ldl"
        "-lrt"
        "-lX11"
    )
endif()