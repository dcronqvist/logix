#pragma once

#include "simulation/simulator.hpp"
#include "display/logix_window.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"

enum EditorState {
    EditorState_None,
    EditorState_MovingCamera,
    EditorState_MovingSelection
};

class Editor {
    private:
    EditorState currentState;
    LogiXWindow* logixWindow;
    public:
    Editor(LogiXWindow* lgx) {
        currentState = EditorState_None;
        logixWindow = lgx;
    }
    void Update();
    void SubmitUI();
    void Draw();
    void Unload();
};