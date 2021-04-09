#include "utils/utility.hpp"
#include "imgui/imgui.h"

void HelperMarker(const char* text) {
    ImGui::TextDisabled("(?)");

    if (ImGui::IsItemHovered()) {
        ImGui::BeginTooltip();
        ImGui::Text(text);
        ImGui::EndTooltip();
    }
}

Vector2 operator+(Vector2 v1, Vector2 v2) {
    return Vector2{ v1.x + v2.x, v1.y + v2.y };
}

Vector2 operator-(Vector2 v1, Vector2 v2) {
    return Vector2{ v1.x - v2.x, v1.y - v2.y };
}

Vector2 operator*(Vector2 v1, float scalar) {
    return Vector2{ v1.x * scalar, v1.y * scalar };
}

Vector2 operator/(Vector2 v1, float scalar) {
    return Vector2{ v1.x / scalar, v1.y / scalar };
}

Color operator*(Color col, float factor) {
    return Color{ col.r, col.g, col.b, (unsigned char)(col.a * factor) };
}