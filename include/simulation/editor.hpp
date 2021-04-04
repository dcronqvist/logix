#pragma once

#include "simulation/simulator.hpp"
#include "display/logix_window.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"

enum EditorState {
    EditorState_None = 0,
    EditorState_MovingCamera = 1,
    EditorState_MovingSelection = 2,
    EditorState_RectangleSelecting = 3
};

class Editor {
public:
    // View and simulation variables
    LogiXWindow* logixWindow;
    Camera2D cam;
    Simulator sim;

private:
    // Mouse variables
    Vector2 currentMousePosWindow;
    Vector2 previousMousePosWindow;
    Vector2 mouseDelta;

private:
    // Editor FSM variables
    EditorState currentState;
    DrawableComponent* newComponent;

    // Rectangle selection starting point
    Vector2 rectangleSelectionStart;
    Rectangle* selectionRectangle;

public:
    Editor(LogiXWindow* lgx) {
        currentState = EditorState_None;
        logixWindow = lgx;
        cam = { Vector2{lgx->handle->GetWidth() / 2.0F, lgx->handle->GetHeight() / 2.0F}, Vector2{0.0F, 0.0F}, 0.0F, 1.0F };
        sim = {};
        newComponent = NULL;
        selectionRectangle = NULL;
    }
    void Update();
    void SubmitUI();
    void Draw();
    void Unload();

    void DrawGrid();
    Vector2 GetMousePositionInWorld() {
        Vector2 viewSize = GetViewSize();
        Vector2 topLeft = { cam.target.x - viewSize.x / 2.0F, cam.target.y - viewSize.y / 2.0F };
        return Vector2{ topLeft.x + currentMousePosWindow.x / cam.zoom, topLeft.y + currentMousePosWindow.y / cam.zoom };
    }
    Vector2 GetViewSize() {
        int windowWidth = this->logixWindow->handle->GetWidth();
        int windowHeight = this->logixWindow->handle->GetHeight();
        Vector2 viewSize = Vector2{ windowWidth / this->cam.zoom, windowHeight / this->cam.zoom };
        return viewSize;
    }
};