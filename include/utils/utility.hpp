#pragma once

#include "raylib-cpp/raylib-cpp.hpp"

void HelperMarker(const char* text);
Vector2 operator+(Vector2 v1, Vector2 v2);
Vector2 operator-(Vector2 v1, Vector2 v2);
Vector2 operator*(Vector2 v1, float scalar);
Vector2 operator/(Vector2 v1, float scalar);