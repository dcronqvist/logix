#include "utils/utility.hpp"
#include "imgui/imgui.h"
#include "gate-logic/and_gate_logic.hpp"
#include "gate-logic/nand_gate_logic.hpp"
#include "gate-logic/or_gate_logic.hpp"
#include "gate-logic/nor_gate_logic.hpp"
#include "gate-logic/xor_gate_logic.hpp"
#include "gate-logic/xnor_gate_logic.hpp"


void HelperMarker(const char* text) {
    ImGui::TextDisabled("(?)");

    if (ImGui::IsItemHovered()) {
        ImGui::BeginTooltip();
        ImGui::Text(text);
        ImGui::EndTooltip();
    }
}

GateLogic* GetGateLogic(const char* gate) {
    if (!strcmp(gate, "AND")) { return new ANDGateLogic(); }
    if (!strcmp(gate, "NAND")) { return new NANDGateLogic(); }
    if (!strcmp(gate, "OR")) { return new ORGateLogic(); }
    if (!strcmp(gate, "NOR")) { return new NORGateLogic(); }
    if (!strcmp(gate, "XOR")) { return new XORGateLogic(); }
    if (!strcmp(gate, "XNOR")) { return new XNORGateLogic(); }
    return NULL;
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
