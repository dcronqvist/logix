#include "simulation/editor.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"
#include "utils/utility.hpp"
#include "drawables/drawable_gate.hpp"
#include "drawables/drawable_switch.hpp"
#include "drawables/drawable_button.hpp"
#include "drawables/drawable_lamp.hpp"
#include "gate-logic/and_gate_logic.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "drawables/circuit_io_desc.hpp"

void Editor::Update() {
    // Get current mouse pos
    currentMousePosWindow = GetMousePosition();
    mouseDelta = (currentMousePosWindow - previousMousePosWindow) / cam.zoom;

    // Perform logic simulation
    sim.Simulate();
    sim.Update(GetMousePositionInWorld());
    ImGuiIO* io = &ImGui::GetIO();

    // Get currently hovered component, no matter state.
    DrawableComponent* hoveredComponent = sim.GetComponentFromPosition(GetMousePositionInWorld());
    CircuitIODesc* hoveredInput = sim.GetComponentInputIODescFromPos(GetMousePositionInWorld());
    CircuitIODesc* hoveredOutput = sim.GetComponentOutputIODescFromPos(GetMousePositionInWorld());
    DrawableWire* hoveredWire = sim.GetWireFromPosition(GetMousePositionInWorld());

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
        if ((IsMouseButtonPressed(MOUSE_MIDDLE_BUTTON) || IsKeyPressed(KEY_SPACE)) && currentState == EditorState_None) {
            currentState = EditorState_MovingCamera;
        }
        // Stop moving camera
        if ((IsMouseButtonReleased(MOUSE_MIDDLE_BUTTON) || IsKeyReleased(KEY_SPACE)) && currentState == EditorState_MovingCamera) {
            currentState = EditorState_None;
        }

        // Start moving selection
        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && currentState == EditorState_None) {
            if (hoveredComponent != NULL) {
                if (sim.IsSelected(hoveredComponent) && !IsKeyDown(KEY_LEFT_SHIFT)) {
                    currentState = EditorState_MovingSelection;
                }
            }
            else {
                if (!sim.IsSelected(hoveredComponent)) {
                    this->rectangleSelectionStart = GetMousePositionInWorld();
                    currentState = EditorState_RectangleSelecting;
                }
            }
        }

        // Stop moving selection
        if (IsMouseButtonReleased(MOUSE_LEFT_BUTTON) && (currentState == EditorState_MovingSelection || currentState == EditorState_RectangleSelecting)) {
            currentState = EditorState_None;
            newComponent = NULL;
            delete this->selectionRectangle;
            this->selectionRectangle = NULL;
        }

        // Hovering inputs & outputs
        if (currentState == EditorState_None) {
            if (hoveredInput != NULL) {
                currentState = EditorState_HoveringInput;
            }
            else if (hoveredOutput != NULL) {
                currentState = EditorState_HoveringOutput;
            }
        }
        else if (currentState == EditorState_HoveringInput || currentState == EditorState_HoveringOutput) {
            if (hoveredInput == NULL && hoveredOutput == NULL) {
                currentState = EditorState_None;
            }
        }

        // Hovering wires
        if (currentState == EditorState_None) {
            if (hoveredWire != NULL) {
                currentState = EditorState_HoveringWire;
            }
        }
        else if (currentState == EditorState_HoveringWire) {
            if (hoveredWire == NULL) {
                currentState = EditorState_None;
            }
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

    if (currentState == EditorState_RectangleSelecting) {
        Vector2 currPos = GetMousePositionInWorld();
        Vector2 start = (this->rectangleSelectionStart);

        if (this->selectionRectangle != NULL) {
            delete this->selectionRectangle;
            this->selectionRectangle = NULL;
        }

        if (currPos.y < start.y && currPos.x > start.x) {
            this->selectionRectangle = new Rectangle{ start.x, currPos.y, currPos.x - start.x, start.y - currPos.y };
        }
        else if (currPos.y < start.y && currPos.x < start.x) {
            this->selectionRectangle = new Rectangle{ currPos.x, currPos.y, start.x - currPos.x, start.y - currPos.y };
        }
        else if (currPos.y > start.y && currPos.x < start.x) {
            this->selectionRectangle = new Rectangle{ currPos.x, start.y, start.x - currPos.x, currPos.y - start.y };
        }
        else {
            this->selectionRectangle = new Rectangle{ start.x, start.y, currPos.x - start.x, currPos.y - start.y };
        }

        sim.SelectAllComponentsInRectangle(*(this->selectionRectangle));
    }

    if (currentState == EditorState_HoveringOutput) {
        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON)) {
            this->tempOutput = hoveredOutput;
            currentState = EditorState_OutputToInput;
        }
    }

    if (currentState == EditorState_HoveringInput) {
        if (IsMouseButtonPressed(MOUSE_RIGHT_BUTTON)) {
            sim.RemoveWire((DrawableWire*)(hoveredInput->component->inputs.at(hoveredInput->index)->GetSignal()));
            hoveredInput->component->RemoveInputWire(hoveredInput->index);
        }
    }

    if (currentState == EditorState_OutputToInput) {
        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && hoveredInput != NULL) {
            if (this->ConnectInputOutput(hoveredInput, tempOutput)) {
                this->tempOutput = NULL;
                currentState = EditorState_None;
            }
        }
    }

    if (currentState == EditorState_None) {
        if (IsKeyPressed(KEY_DELETE)) {
            sim.DeleteSelectedComponents();
        }
    }

#pragma endregion

#pragma region SELECTING/DESELECTING COMPONENTS

    // When pressing LMB, we want to select the hovered component.
    // If we're also pressing LSHIFT, we want to add the hovered component to
    // the current selection, to select multiple components
    // Pressing on nothing should clear the selected components.
    // Pressing on one, when we have other selected, we want only the new one
    // to be the selected one.

    // LMB + hovering component -> clear current selection + add hovered to selection
    // LMB + LSHIFT + hovering component -> add hovered to selection/remove hovered from selection
    // (LMB or (LMB + LSHIFT)) + not hovering component -> clear current selection

    // Can only perform selection/deselection when doing nothing else.
    // Pressing LMB (no LSHIFT)
    if (currentState == EditorState_None) {
        if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && !IsKeyDown(KEY_LEFT_SHIFT)) {
            // Clear entire current selection no matter if we are hovering
            // a component at all.
            sim.ClearSelection();
            // If we are hovering a component -> select it.
            if (hoveredComponent != NULL) {
                sim.SelectComponent(hoveredComponent);

                // Setting the current state to moving_selection allows for
                // instant movement when selecting a component.
                currentState = EditorState_MovingSelection;
            }
        }
        else if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && IsKeyDown(KEY_LEFT_SHIFT)) {
            // Pressing LMB + LSHIFT
            // Hoving a component should toggle its selection state.
            // If selected -> deselect & vice versa.
            if (hoveredComponent != NULL) {
                sim.ToggleComponentSelected(hoveredComponent);
            }
            else {
                // If we aren't hovering anything, clear current selection.
                sim.ClearSelection();
            }
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

    if (ImGui::Begin("Components", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {
        ImGui::Button("AND");
        if (ImGui::IsItemClicked()) {
            DrawableComponent* dc = new DrawableGate(GetMousePositionInWorld(), new ANDGateLogic(), new std::vector<int>{ 2, 1 });
            newComponent = dc;
            sim.ClearSelection();
            sim.AddComponent(newComponent);
            sim.SelectComponent(newComponent);
            currentState = EditorState_MovingSelection;
        }

        ImGui::Button("Switch");
        if (ImGui::IsItemClicked()) {
            DrawableComponent* dc = new DrawableSwitch(GetMousePositionInWorld());
            newComponent = dc;
            sim.ClearSelection();
            sim.AddComponent(newComponent);
            sim.SelectComponent(newComponent);
            currentState = EditorState_MovingSelection;
        }

        ImGui::Button("Button");
        if (ImGui::IsItemClicked()) {
            DrawableComponent* dc = new DrawableButton(GetMousePositionInWorld());
            newComponent = dc;
            sim.ClearSelection();
            sim.AddComponent(newComponent);
            sim.SelectComponent(newComponent);
            currentState = EditorState_MovingSelection;
        }

        ImGui::Button("Lamp");
        if (ImGui::IsItemClicked()) {
            DrawableComponent* dc = new DrawableLamp(GetMousePositionInWorld());
            newComponent = dc;
            sim.ClearSelection();
            sim.AddComponent(newComponent);
            sim.SelectComponent(newComponent);
            currentState = EditorState_MovingSelection;
        }

        ImGui::Text("Current state: %d", currentState);
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

    sim.Draw(GetMousePositionInWorld());

    if (this->selectionRectangle != NULL) {
        DrawRectangleRec(*(this->selectionRectangle), RED * 0.3F);
    }

    if (this->currentState == EditorState_OutputToInput) {
        Vector2 start = tempOutput->component->GetOutputPosition(tempOutput->index);
        Vector2 end = GetMousePositionInWorld();

        DrawLineEx(start, end, 3.0F, WHITE);
    }

    EndMode2D();
}

void Editor::Unload() {

}

bool Editor::ConnectInputOutput(CircuitIODesc* input, CircuitIODesc* output) {
    // If there is no wire to this input
    // then assign a new wire between the tempOutput & this input
    // if there is a wire to this input, do nothing

    // Check no wire to input
    if (!input->component->GetInputFromIndex(input->index)->HasSignal()) {
        DrawableWire* newWire = new DrawableWire{ output->component, output->index, input->component, input->index, 1 };
        sim.AddWire(newWire);
        input->component->SetInputWire(input->index, newWire);
        output->component->AddOutputWire(output->index, newWire);
        return true;
    }

    return false;
}