#include "simulation/editor.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"
#include "utils/utility.hpp"
#include "drawables/drawable_gate.hpp"
#include "gate-logic/and_gate_logic.hpp"
#include "raylib-cpp/raylib-cpp.hpp"

void Editor::Update() {
    // Get current mouse pos
    currentMousePosWindow = GetMousePosition();
    mouseDelta = (currentMousePosWindow - previousMousePosWindow) / cam.zoom;

    // Perform logic simulation
    sim.Simulate();
    ImGuiIO* io = &ImGui::GetIO();

    // Get currently hovered component, no matter state.
    DrawableComponent* hoveredComponent = sim.GetComponentFromPosition(GetMousePositionInWorld());

#pragma region MOUSE WHEEL CAMERA ZOOM

    if (!io->WantCaptureMouse) {
        if (GetMouseWheelMove() > 0) {
            cam.zoom *= 1.05F;
        }
        if (GetMouseWheelMove() < 0) {
            cam.zoom *= 1.0F / 1.05F;
        }
    }

#pragma endregion

#pragma region DECIDE WHICH EDITOR STATE

    if (!io->WantCaptureMouse || newComponent != NULL) {

        // Start moving camera
        if (IsMouseButtonPressed(MOUSE_MIDDLE_BUTTON) && currentState == EditorState_None) {
            currentState = EditorState_MovingCamera;
        }
        // Stop moving camera
        if (IsMouseButtonReleased(MOUSE_MIDDLE_BUTTON) && currentState == EditorState_MovingCamera) {
            currentState = EditorState_None;
        }

        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && currentState == EditorState_None) {

            if (hoveredComponent != NULL)
                currentState = EditorState_MovingSelection;
        }

        // Start moving selection
        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && currentState == EditorState_None) {
            if (hoveredComponent != NULL) {
                currentState = EditorState_MovingSelection;
            }
            else {
                // Want to do rectangle selection here
            }
        }

        // Stop moving selection
        if (IsMouseButtonReleased(MOUSE_LEFT_BUTTON) && currentState == EditorState_MovingSelection) {
            currentState = EditorState_None;

            newComponent = NULL;
        }
    }

#pragma endregion

#pragma region PERFORM EDITOR STATE

    if (currentState == EditorState_MovingCamera) {
        cam.target = cam.target - mouseDelta;
    }

    if (currentState == EditorState_MovingSelection) {
        sim.MoveAllSelectedComponents(mouseDelta);
    }

#pragma endregion



    // Set previous mouse pos to old current
    previousMousePosWindow = currentMousePosWindow;
}

void Editor::SubmitUI() {
    // Main Menu
    ImGui::BeginMainMenuBar();

    ImGui::EndMainMenuBar();

    if (ImGui::Begin("Components", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {
        ImGui::Button("AND");
        if (ImGui::IsItemClicked()) {
            DrawableComponent* dc = new DrawableGate(GetMousePositionInWorld(), new ANDGateLogic(), 2);
            newComponent = dc;
            sim.ClearSelection();
            sim.AddComponent(newComponent);
            sim.SelectComponent(newComponent);
            currentState = EditorState_MovingSelection;
        }
    }
    ImGui::End();


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

    EndMode2D();
}

void Editor::Unload() {

}