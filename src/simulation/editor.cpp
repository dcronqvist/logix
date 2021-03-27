#include "simulation/editor.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"

void Editor::Update() {
    // Get current mouse pos
    currentMousePosWindow = GetMousePosition();

    // Perform logic simulation
    sim.Simulate();

#pragma region MOUSE WHEEL CAMERA ZOOM

    ImGuiIO* io = &ImGui::GetIO();

    if (!io->WantCaptureMouse) {
        if (GetMouseWheelMove() > 0) {
            cam.zoom *= 1.05F;
        }
        if (GetMouseWheelMove() < 0) {
            cam.zoom *= 1.0F / 1.05F;
        }
    }

#pragma endregion

    // Set previous mouse pos to old current
    previousMousePosWindow = currentMousePosWindow;
}

void Editor::SubmitUI() {
    // Main Menu
    ImGui::BeginMainMenuBar();

    ImGui::EndMainMenuBar();


    ImGui::ShowDemoWindow();
}

void Editor::DrawGrid() {
    // Get camera's position and the size of the current view
    // This depends on the zoom of the camera. Dividing by the
    // zoom gives a correct representation of the actual visible view.
    Vector2 camPos = this->cam.target;
    Vector2 viewSize = this->GetViewSize();

    int pixelsInBetweenLines = 250;

    // Draw vertical lines
    for (int i = ((camPos.x - viewSize.x / 2.0F) / pixelsInBetweenLines); i < ((camPos.x + viewSize.x / 2.0F) / pixelsInBetweenLines); i++) {
        float lineX = i * pixelsInBetweenLines;
        float lineYstart = (camPos.y - viewSize.y / 2.0F);
        float lineYend = (camPos.y + viewSize.y / 2.0F);

        DrawLine(lineX, lineYstart, lineX, lineYend, DARKGRAY);
    }

    // Draw horizontal lines
    for (int i = ((camPos.y - viewSize.y / 2.0F) / pixelsInBetweenLines); i < ((camPos.y + viewSize.y / 2.0F) / pixelsInBetweenLines); i++) {
        float lineY = i * pixelsInBetweenLines;
        float lineXstart = (camPos.x - viewSize.x / 2.0F);
        float lineXend = (camPos.x + viewSize.x / 2.0F);
        DrawLine(lineXstart, lineY, lineXend, lineY, DARKGRAY);
    }
}

void Editor::Draw() {
    BeginMode2D(this->cam);
    ClearBackground(LIGHTGRAY);
    DrawGrid();

    sim.Draw();

    DrawCircleV(this->GetMousePositionInWorld(), 10.0F, RED);

    EndMode2D();
}

void Editor::Unload() {

}