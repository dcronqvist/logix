#pragma once

#include "raylib-cpp/raylib-cpp.hpp"
#include "display/base_window.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"
#include "utils/utility.hpp"

class LogiXWindow : public BaseWindow {
    public:
    LogiXWindow(int w, int h) : BaseWindow(w, h, "LogiX") {};
    void Initialize();
    void LoadContent();
    void Update();
    void Render();
    void Unload();
    bool AttemptExit();
    bool OnFailedClose();
};