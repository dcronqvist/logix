#include "raylib-cpp/raylib-cpp.hpp"
#include "display/base_window.hpp"
#include <string>

BaseWindow::BaseWindow(int windowWidth, int windowHeight, std::string tit) {
    this->windowWidth = windowWidth;
    this->windowHeight = windowHeight;
    this->title = tit;
    this->shouldClose = false;
}

bool BaseWindow::FocusingWindow() {
    bool focusing = IsWindowFocused() && this->isFocused == false;

    if (focusing)
        this->isFocused = true;

    return focusing;
}

bool BaseWindow::UnfocusingWindow() {
    bool unfocusing = !IsWindowFocused() && this->isFocused == true;

    if (unfocusing)
        this->isFocused = false;

    return unfocusing;
}

int BaseWindow::Run() {
    this->Initialize();

    this->handle = new raylib::Window(windowWidth, windowHeight, title);
    SetExitKey(-1);

    this->LoadContent();

    while (!shouldClose) {

        if (WindowShouldClose()) {
            shouldClose = AttemptExit();
            if (!shouldClose) {
                shouldClose = OnFailedClose();
            }
        }

        this->Update();
        this->Render();
    }

    this->Unload();
    return 0;
}

bool BaseWindow::KeyCombinationPressed(KeyboardKey modifier, KeyboardKey key) {
    return IsKeyDown(modifier) && IsKeyPressed(key);
}