#pragma once

#include "raylib-cpp/raylib-cpp.hpp"
#include "gate-logic/gate_logic.hpp"

const float DRAWABLE_TEXT_FONT_SIZE = 12.0F;
const float DRAWABLE_ADDITIONAL_TEXT_FONT_SIZE = 10.0F;

void HelperMarker(const char* text);
GateLogic* GetGateLogic(const char* gate);
Vector2 operator+(Vector2 v1, Vector2 v2);
Vector2 operator-(Vector2 v1, Vector2 v2);
Vector2 operator*(Vector2 v1, float scalar);
Vector2 operator/(Vector2 v1, float scalar);
Color operator*(Color col, float factor);
