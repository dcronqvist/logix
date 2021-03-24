#include "raylib-cpp/raylib-cpp.hpp"
#include "display/base_window.hpp"
#include <string>

BaseWindow::BaseWindow(int windowWidth, int windowHeight, std::string tit) {
    this->windowWidth = windowWidth;
    this->windowHeight = windowHeight;
    this->title = tit;
}

void BaseWindow::CloseWindowSafely() {
    this->closeSafely = true;
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

void BaseWindow::Run() {
    this->Initialize();

    raylib::Window window(windowWidth, windowHeight, title);
    SetExitKey(-1);

    this->LoadContent();

    while (!WindowShouldClose() && !closeSafely) {
        this->Update();
        this->Render();
    }

    this->Unload();
}

bool BaseWindow::KeyCombinationPressed(KeyboardKey modifier, KeyboardKey key) {
    return IsKeyDown(modifier) && IsKeyPressed(key);
}