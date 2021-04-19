#include "simulation/editor.hpp"
#include "simulation/simulator.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"
#include "imgui/imgui_stdlib.h"
#include "utils/utility.hpp"
#include "drawables/drawable_gate.hpp"
#include "drawables/drawable_switch.hpp"
#include "drawables/drawable_button.hpp"
#include "drawables/drawable_lamp.hpp"
#include "drawables/drawable_ic.hpp"
#include "drawables/drawable_hex_viewer.hpp"
#include "gate-logic/and_gate_logic.hpp"
#include "gate-logic/or_gate_logic.hpp"
#include "gate-logic/nor_gate_logic.hpp"
#include "gate-logic/xor_gate_logic.hpp"
#include "gate-logic/xnor_gate_logic.hpp"
#include "gate-logic/nand_gate_logic.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "drawables/circuit_io_desc.hpp"
#include "integrated/ic_desc.hpp"
#include <iostream>
#include <fstream>

char buf[64];

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
        /*
        if (currentState == EditorState_None) {
            if (hoveredWire != NULL) {
                currentState = EditorState_HoveringWire;
            }
        }
        else if (currentState == EditorState_HoveringWire) {
            if (hoveredWire == NULL) {
                currentState = EditorState_None;
            }
        }*/
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
    if (currentState == EditorState_None && !io->WantCaptureMouse) {
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
        else if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) && IsKeyDown(KEY_LEFT_SHIFT) && !io->WantCaptureMouse) {
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

#pragma region MAIN MENU BAR
    ImGui::BeginMainMenuBar();

    if (ImGui::BeginMenu("File")) {
        ImGui::EndMenu();
    }

    if (ImGui::BeginMenu("Edit")) {
        ImGui::EndMenu();
    }

    if (ImGui::BeginMenu("Simulation")) {
        ImGui::EndMenu();
    }

    if (ImGui::BeginMenu("View")) {
        ImGui::SliderFloat("Camera Zoom", &cam.zoom, 0.1F, 15.0F, "%.2f", ImGuiSliderFlags_Logarithmic);

        ImGui::EndMenu();
    }

    if (this->IsKeyCombinationPressed(KEY_LEFT_CONTROL, KEY_I)) { this->currentState = EditorState_MakingIC; }
    if (ImGui::BeginMenu("Integrated Circuits")) {
        if (ImGui::MenuItem("New...", "CTRL + I")) {
            this->currentState = EditorState_MakingIC;
        }

        ImGui::EndMenu();
    }

    ImGui::Separator();
    ImGui::Text("Current state: %d", currentState);

    ImGui::EndMainMenuBar();
#pragma endregion

#pragma region COMPONENTS WINDOW
    if (ImGui::Begin("Components", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {

        AddNewGateButton("AND");
        AddNewGateButton("NAND");
        AddNewGateButton("OR");
        AddNewGateButton("NOR");
        AddNewGateButton("XOR");
        AddNewGateButton("XNOR");

        ImGui::Button("Switch", ImVec2(65, 22));
        if (ImGui::IsItemClicked()) {
            this->AddNewComponent(new DrawableSwitch(GetMousePositionInWorld(), 1));
        }
        if (ImGui::BeginPopupContextItem("SwitchN Context Menu")) {

            ImGui::SetNextItemWidth(80);
            ImGui::InputInt("Bits", &(this->switchNBits), 1, 1);

            if (ImGui::Button("Create")) {
                this->AddNewComponent(new DrawableSwitch(GetMousePositionInWorld(), this->switchNBits));
            }

            ImGui::EndPopup();
        }

        ImGui::Button("Button", ImVec2(65, 22));
        if (ImGui::IsItemClicked()) {
            this->AddNewComponent(new DrawableButton(GetMousePositionInWorld()));
        }

        ImGui::Button("Lamp", ImVec2(65, 22));
        if (ImGui::IsItemClicked()) {
            this->AddNewComponent(new DrawableLamp(GetMousePositionInWorld()));
        }

        ImGui::Button("Hex Viewer", ImVec2(65, 22));
        if (ImGui::IsItemClicked()) {
            this->AddNewComponent(new DrawableHexViewer(4, GetMousePositionInWorld(), new std::vector<int>{ 1, 1, 1, 1 }));
        }
        if (ImGui::BeginPopupContextItem("HexN Context Menu")) {

            ImGui::SetNextItemWidth(80);
            ImGui::InputInt("Bits", &(this->switchNBits), 1, 1);
            ImGui::Checkbox("Multibit Input", &this->groupBits);
            ImGui::Separator();
            if (ImGui::Button("Create")) {
                if (this->groupBits) {
                    this->AddNewComponent(new DrawableHexViewer(this->switchNBits, GetMousePositionInWorld(), new std::vector<int>{ this->switchNBits }));
                }
                else {
                    this->AddNewComponent(new DrawableHexViewer(this->switchNBits, GetMousePositionInWorld(), new std::vector<int>(this->switchNBits, 1)));
                }
            }

            ImGui::EndPopup();
        }
    }
    ImGui::End();
#pragma endregion

#pragma region CREATE NEW IC WINDOW

    if (currentState == EditorState_MakingIC) {
        ImGui::OpenPopup("Create Integrated Circuit");

        // This resets all states that are required for creating an IC
        if (this->icInputs.size() == 0) {
            this->icInputs = this->sim.GetAllSelectedOfType<DrawableSwitch>();
            this->icOutputs = this->sim.GetAllSelectedOfType<DrawableLamp>();
            this->icNonIOs = this->sim.GetAllSelectedNonIOs();
            this->icName = "";
            this->icAdditionalText = "";
            this->icSaveToFile = true;

            // Gather all selected input id's and create their starting group numbers
            this->icInputIds = {};
            this->icInputGroupNumbers = {};
            for (int i = 0; i < this->icInputs.size(); i++) {
                this->icInputIds.push_back(*(this->icInputs.at(i)->id));
                this->icInputGroupNumbers.push_back(i);
            }
            // Gather all selected output id's and create their starting group numbers
            this->icOutputIds = {};
            this->icOutputGroupNumbers = {};
            for (int i = 0; i < this->icOutputs.size(); i++) {
                this->icOutputIds.push_back(*(this->icOutputs.at(i)->id));
                this->icOutputGroupNumbers.push_back(i);
            }
        }
    }

    ImVec2 middleOfWindow = ImVec2{ (float)(this->logixWindow->windowWidth) / 2.0F, (float)(this->logixWindow->windowHeight) / 2.0F };
    ImGui::SetNextWindowPos(middleOfWindow, ImGuiCond_Always, ImVec2{ 0.5F, 0.5F });
    if (ImGui::BeginPopupModal("Create Integrated Circuit", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {
        if (!ImGui::IsAnyItemActive()) { ImGui::SetKeyboardFocusHere(0); }
        ImGui::InputText("Name", &(this->icName));
        ImGui::InputTextMultiline("", &(this->icAdditionalText));

        ImGui::Columns(2);
        ImGui::Text("Inputs");
        ImGui::NextColumn();
        ImGui::Text("Outputs");
        ImGui::Separator();
        ImGui::NextColumn();

        for (int i = 0; i < this->icInputIds.size(); i++) {
            ImGui::PushID(this->icInputIds.at(i).c_str());
            ImGui::SetNextItemWidth(80.0F);
            ImGui::InputInt("", &(this->icInputGroupNumbers.at(i)), 1, 100);
            ImGui::SameLine();
            ImGui::Selectable(this->icInputIds.at(i).c_str());

            if (ImGui::IsItemActive() && !ImGui::IsItemHovered()) {
                int iNext = i + (ImGui::GetMouseDragDelta(0).y < 0.f ? -1 : 1);
                if (iNext >= 0 && iNext < this->icInputIds.size()) {
                    std::swap(this->icInputIds.at(i), this->icInputIds.at(iNext));
                    std::swap(this->icInputGroupNumbers.at(i), this->icInputGroupNumbers.at(iNext));
                    ImGui::ResetMouseDragDelta();
                }
            }

            ImGui::PopID();
        }

        ImGui::NextColumn();

        for (int i = 0; i < this->icOutputIds.size(); i++) {
            ImGui::PushID(this->icOutputIds.at(i).c_str());
            ImGui::SetNextItemWidth(80.0F);
            ImGui::InputInt("", &(this->icOutputGroupNumbers.at(i)), 1, 100);
            ImGui::SameLine();
            ImGui::Selectable(this->icOutputIds.at(i).c_str());

            if (ImGui::IsItemActive() && !ImGui::IsItemHovered()) {
                int iNext = i + (ImGui::GetMouseDragDelta(0).y < 0.f ? -1 : 1);
                if (iNext >= 0 && iNext < this->icOutputIds.size()) {
                    std::swap(this->icOutputIds.at(i), this->icOutputIds.at(iNext));
                    std::swap(this->icOutputGroupNumbers.at(i), this->icOutputGroupNumbers.at(iNext));
                    ImGui::ResetMouseDragDelta();
                }
            }

            ImGui::PopID();
        }

        ImGui::Columns(1);
        ImGui::Separator();

        if (ImGui::Button("Cancel")) {
            currentState = EditorState_None;
            this->icInputs = {};
            this->icOutputs = {};
            ImGui::CloseCurrentPopup();
        }

        ImGui::SameLine();

        if (ImGui::Button("Create")) {
            std::vector<DrawableComponent*> comps = {};
            for (int i = 0; i < this->icInputs.size(); i++) {
                comps.push_back(this->icInputs.at(i));
            }
            for (int i = 0; i < this->icOutputs.size(); i++) {
                comps.push_back(this->icOutputs.at(i));
            }
            for (int i = 0; i < this->icNonIOs.size(); i++) {
                comps.push_back(this->icNonIOs.at(i));
            }

            ICDesc icdesc = ICDesc{ this->icName, comps, this->GetICInputVector(), this->GetICOutputVector() };
            icdesc.SetAdditionalText(this->icAdditionalText);
            this->icDescriptions.push_back(icdesc);
            json j = icdesc;
            std::cout << j << std::endl;

            currentState = EditorState_None;
            this->icInputs = {};
            this->icOutputs = {};
            ImGui::CloseCurrentPopup();
            DrawableIC* dic = new DrawableIC(GetMousePositionInWorld(), icdesc);
            AddNewComponent(dic);

            // If the user has chosen to save the new IC to a file
            if (this->icSaveToFile) {
                std::ofstream o("ic/" + icName + ".ic");
                o << j << std::endl;
                o.close();
            }
        }

        ImGui::SameLine();

        ImGui::Checkbox("Save to file", &(this->icSaveToFile));

        ImGui::EndPopup();
    }

#pragma endregion

#pragma region EDIT SELECTED IO

    if (sim.selectedComponents.size() == 1) {
        DrawableComponent* dc = sim.selectedComponents.at(0);

        DrawableSwitch* ds = dynamic_cast<DrawableSwitch*>(dc);
        DrawableLamp* dl = dynamic_cast<DrawableLamp*>(dc);

        if (ds != NULL) {
            if (ImGui::Begin("Setting IO ID", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {
                if (!ImGui::IsAnyItemActive() && !ImGui::IsAnyItemHovered()) { ImGui::SetKeyboardFocusHere(0); }
                ImGui::InputText("ID", ds->id, ImGuiInputTextFlags_AlwaysOverwrite);
            }
            ImGui::End();
        }
        else if (dl != NULL) {
            if (ImGui::Begin("Setting IO ID", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {
                if (!ImGui::IsAnyItemActive() && !ImGui::IsAnyItemHovered()) { ImGui::SetKeyboardFocusHere(0); }
                ImGui::InputText("ID", dl->id, ImGuiInputTextFlags_AlwaysOverwrite);
            }
            ImGui::End();
        }
    }

#pragma endregion

    // Testing stuff

    if (ImGui::Begin("ICs", NULL, ImGuiWindowFlags_AlwaysAutoResize)) {
        for (int i = 0; i < this->icDescriptions.size(); i++) {
            ImGui::Button(this->icDescriptions.at(i).name.c_str(), ImVec2(65, 22));
            if (ImGui::IsItemClicked()) {
                this->AddNewComponent(new DrawableIC(GetMousePositionInWorld(), this->icDescriptions.at(i)));
            }
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
        if (input->bits == output->bits) {
            DrawableWire* newWire = new DrawableWire{ output->component, output->index, input->component, input->index, input->bits };
            sim.AddWire(newWire);
            input->component->SetInputWire(input->index, newWire);
            output->component->AddOutputWire(output->index, newWire);
            return true;
        }
    }

    return false;
}

void Editor::AddNewComponent(DrawableComponent* comp) {
    newComponent = comp;
    sim.ClearSelection();
    sim.AddComponent(newComponent);
    sim.SelectComponent(newComponent);
    currentState = EditorState_MovingSelection;
}

void Editor::AddNewGateButton(const char* gate) {
    ImGui::Button(gate, ImVec2(65, 22));
    if (ImGui::IsItemClicked()) {
        this->AddNewComponent(new DrawableGate(GetMousePositionInWorld(), GetGateLogic(gate), new std::vector<int>{ 1, 1 }));
    }
    if (ImGui::BeginPopupContextItem(gate)) {

        ImGui::SetNextItemWidth(80);
        ImGui::InputInt("Bits", &(this->gateBits), 1, 1);
        ImGui::Checkbox("Multibit Input", &this->groupBits);
        ImGui::Separator();
        if (ImGui::Button("Create")) {

            if (this->groupBits) {
                this->AddNewComponent(new DrawableGate(GetMousePositionInWorld(), GetGateLogic(gate), new std::vector<int>{ this->gateBits }));
            }
            else {
                this->AddNewComponent(new DrawableGate(GetMousePositionInWorld(), GetGateLogic(gate), new std::vector<int>(this->gateBits, 1)));
            }
        }
        ImGui::EndPopup();
    }
}

std::vector<std::vector<std::string>> Editor::GetICInputVector() {
    std::vector<std::vector<std::string>> v = {};

    for (int i = 0; i < this->icInputIds.size(); i++) {
        v.push_back(std::vector<std::string>{});

        int groupNumber = this->icInputGroupNumbers.at(i);
        std::string id = this->icInputIds.at(i);

        v.at(groupNumber).push_back(id);
    }

    std::vector<std::vector<std::string>> fi = {};

    for (int i = 0; i < v.size(); i++) {
        if (v.at(i).size() != 0) {
            fi.push_back(v.at(i));
        }
    }

    return fi;
}

std::vector<std::vector<std::string>> Editor::GetICOutputVector() {
    std::vector<std::vector<std::string>> v = {};

    for (int i = 0; i < this->icOutputIds.size(); i++) {
        v.push_back(std::vector<std::string>{});

        int groupNumber = this->icOutputGroupNumbers.at(i);
        std::string id = this->icOutputIds.at(i);

        v.at(groupNumber).push_back(id);
    }

    std::vector<std::vector<std::string>> fi = {};

    for (int i = 0; i < v.size(); i++) {
        if (v.at(i).size() != 0) {
            fi.push_back(v.at(i));
        }
    }

    return fi;
}

bool Editor::IsKeyCombinationPressed(KeyboardKey modifier, KeyboardKey key) {
    return IsKeyDown(modifier) && IsKeyPressed(key);
}