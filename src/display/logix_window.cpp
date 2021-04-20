#include "display/logix_window.hpp"
#include "simulation/editor.hpp"

Image windowIcon;
RenderTexture2D renTexUI;
Rectangle uiSourceRectangle;
Editor* editor;

void LogiXWindow::Initialize() {
    SetConfigFlags(FLAG_WINDOW_HIGHDPI | FLAG_VSYNC_HINT);
}

void LogiXWindow::LoadContent() {
    // Initialize window icon
    windowIcon = LoadImage("../assets/logo.png");
    ImageFormat(&windowIcon, UNCOMPRESSED_R8G8B8A8);
    SetWindowIcon(windowIcon);

    // Fix high dpi font
    SetTextureFilter(GetFontDefault().texture, FILTER_POINT);
    //SetTargetFPS(144);

    // Init imgui
    ImGui_ImplRaylib_Init(windowWidth, windowHeight, { "../assets/opensans.ttf", "../assets/opensans-bold.ttf" }, 16.0F);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 3);
    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 3);

    // Fix UI render texture
    renTexUI = LoadRenderTexture(windowWidth, windowHeight);
    uiSourceRectangle = { 0, static_cast<float>(windowHeight), static_cast<float>(windowWidth), static_cast<float>(-windowHeight) };
    editor = new Editor(this);
}

void LogiXWindow::Update() {
    ImGui_ImplRaylib_NewFrame();
    ImGui_ImplRaylib_ProcessEvent();
    ImGui::NewFrame();
    editor->SubmitUI();


    if (this->FocusingWindow()) {
        SetConfigFlags(FLAG_WINDOW_HIGHDPI | FLAG_VSYNC_HINT);
        SetTargetFPS(-1);
    }
    if (this->UnfocusingWindow()) {
        SetConfigFlags(FLAG_WINDOW_HIGHDPI);
        SetTargetFPS(10);
    }

    editor->Update();
}

void LogiXWindow::Render() {

    // Draw UI to a RenderTexture2D
    BeginTextureMode(renTexUI); {
        ClearBackground(BLANK);
        ImGui::Render();
        ImDrawData* draw_data = ImGui::GetDrawData();
        raylib_render_imgui(draw_data);
        EndTextureMode();
    }

    // Begin drawing to screen
    BeginDrawing();
    // Clear to a white background color

    editor->Draw();

    // Draw UI rendertexture to screen. uiSourceRectangle is a 
    // source rec that is specified to be upside down, since rendering
    // actually takes place upside down when rendering to texture.
    DrawTextureRec(renTexUI.texture, uiSourceRectangle, { 0, 0 }, WHITE);

    DrawFPS(this->windowWidth - 120, this->windowHeight - 30);

    EndDrawing();
}

void LogiXWindow::Unload() {
    editor->Unload();
}