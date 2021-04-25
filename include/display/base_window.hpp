#pragma once

#include "raylib-cpp/raylib-cpp.hpp"
#include <string>

class BaseWindow {
    public:
    int windowWidth;
    int windowHeight;
    std::string title;
    raylib::Window* handle;

    private:
    bool shouldClose;
    bool isFocused;

    public:
    BaseWindow(int windowWidth, int windowHeight, std::string tit);
    int Run();
    bool UnfocusingWindow();
    bool FocusingWindow();
    bool KeyCombinationPressed(KeyboardKey modifier, KeyboardKey key);

    protected:
    virtual void Initialize() = 0;
    virtual void LoadContent() = 0;
    virtual void Update() = 0;
    virtual void Render() = 0;
    virtual void Unload() = 0;
    virtual bool AttemptExit() = 0;
    virtual bool OnFailedClose() = 0;
};