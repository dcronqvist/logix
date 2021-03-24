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