#include "raylib-cpp/raylib-cpp.hpp"
#include "display/logix_window.hpp"
#include "imgui/imgui.h"
#include "imgui/imgui_impl_raylib.h"
#include "utils/utility.hpp"
#include <string>

Image windowIcon;
RenderTexture2D renTexUI;

char textiBuf[128];
Rectangle uiSourceRectangle;

const char* stuff[] = { "hej", "på", "rej", "rin", "lilla", "söting", "aslångt alternativ" };
int selected = 0;
bool open = true;

void LogiXWindow::Initialize() {
    SetConfigFlags(FLAG_WINDOW_HIGHDPI);
}

void LogiXWindow::LoadContent() {
    // Initialize window icon
    windowIcon = LoadImage("../assets/logo.png");
    ImageFormat(&windowIcon, UNCOMPRESSED_R8G8B8A8);
    SetWindowIcon(windowIcon);

    // Fix high dpi font
    SetTextureFilter(GetFontDefault().texture, FILTER_POINT);
    SetTargetFPS(144);

    // Init imgui
    ImGui_ImplRaylib_Init(windowWidth, windowHeight, { "../assets/opensans.ttf", "../assets/opensans-bold.ttf" }, 16.0F);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 3);
    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 3);

    // Fix UI render texture
    renTexUI = LoadRenderTexture(windowWidth, windowHeight);
    uiSourceRectangle = { 0, static_cast<float>(windowHeight), static_cast<float>(windowWidth), static_cast<float>(-windowHeight) };
}

void LogiXWindow::Update() {
    if (this->FocusingWindow()) {
        SetTargetFPS(144);
    }
    if (this->UnfocusingWindow()) {
        SetTargetFPS(15);
    }
}

void SubmitUI(LogiXWindow& window) {
    // Main Menu
    ImGui::BeginMainMenuBar();

    if (window.KeyCombinationPressed(KEY_LEFT_CONTROL, KEY_A)) {
        open = !open;
    }
    if (ImGui::BeginMenu("Stuff")) {
        if (ImGui::MenuItem("Open?", "CTRL+A", &open)) {

        }
        ImGui::EndMenu();
    }
    ImGui::EndMainMenuBar();

    if (open) {
        if (ImGui::Begin("Testing", &open, ImGuiWindowFlags_AlwaysAutoResize)) {
            if (ImGui::Button("Push me")) {
                window.CloseWindowSafely();
            }
            ImGui::SameLine();
            ImGui::SetNextItemWidth(100);
            ImGui::InputText("Texti", textiBuf, IM_ARRAYSIZE(textiBuf));
            ImGui::SameLine();
            HelperMarker("This is a test tooltip which has a lot of text with\nsome actual text wrapping going on.\nThis is hella nice.");

            float maxWidth = 0;

            for (int i = 0; i < IM_ARRAYSIZE(stuff); i++) {
                float calcWidth = ImGui::CalcTextSize(stuff[i]).x;

                if (calcWidth > maxWidth)
                    maxWidth = calcWidth;
            }

            ImGui::SetNextItemWidth(maxWidth + 30);
            ImGui::Combo("Combobox", &selected, stuff, IM_ARRAYSIZE(stuff));
        }
        ImGui::End();
    }
}

void LogiXWindow::Render() {

    // Draw UI to a RenderTexture2D
    BeginTextureMode(renTexUI); {
        ClearBackground(BLANK);
        ImGui_ImplRaylib_NewFrame();
        ImGui_ImplRaylib_ProcessEvent();
        ImGui::NewFrame();

        SubmitUI(*this);

        ImGui::Render();
        ImDrawData* draw_data = ImGui::GetDrawData();
        raylib_render_imgui(draw_data);
        EndTextureMode();
    }

    // Begin drawing to screen
    BeginDrawing();
    // Clear to a white background color
    ClearBackground(GRAY);

    // Draw UI rendertexture to screen. uiSourceRectangle is a 
    // source rec that is specified to be upside down, since rendering
    // actually takes place upside down when rendering to texture.
    DrawTextureRec(renTexUI.texture, uiSourceRectangle, { 0, 0 }, WHITE);

    DrawFPS(10, 10);

    EndDrawing();
}

void LogiXWindow::Unload() {}