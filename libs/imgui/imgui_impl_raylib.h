#pragma once

#define CIMGUI_DEFINE_ENUMS_AND_STRUCTS
#include "imgui/imconfig.h"
#include "imgui/imgui.h"
#include <float.h>
#include "raylib-cpp/raylib-cpp.hpp"
#include "raylib-cpp/rlgl.h"
#include <string>
#include <vector>

static double g_Time = 0.0;

static const char* ImGui_ImplRaylib_GetClipboardText(void* _) {
    return GetClipboardText();
}

static void ImGui_ImplRaylib_SetClipboardText(void* _, const char* text) {
    SetClipboardText(text);
}

Texture2D* ImGui_ImplRaylib_InitFontTexture(const char* font, float font_size) {
    ImGuiIO* io = &ImGui::GetIO();
    unsigned char* pixels = NULL;
    int width = 0;
    int height = 0;

    if (font != "") {
        if (FileExists(font)) {
            io->Fonts->AddFontFromFileTTF(font, font_size);
        }
        else {
            io->Fonts->AddFontDefault();
        }
    }
    else {
        io->Fonts->AddFontDefault();
    }
    io->Fonts->GetTexDataAsRGBA32(&pixels, &width, &height, NULL);

    Image* im = new Image{};
    im->data = pixels;
    im->width = width;
    im->height = height;
    im->mipmaps = 1;
    im->format = PixelFormat::UNCOMPRESSED_R8G8B8A8;

    Texture2D* tex = new Texture2D{ LoadTextureFromImage(*im) };
    return tex;
}

bool ImGui_ImplRaylib_Init(int w, int h, std::vector<const char*> fonts = {}, const float font_size = 12.0F) {
    IMGUI_CHECKVERSION();
    ImGui::CreateContext();
    ImGuiIO* io = &ImGui::GetIO(); (void)io;
    ImGui::StyleColorsDark();

    io->BackendPlatformName = "imgui_impl_raylib";

    io->KeyMap[ImGuiKey_Tab] = KEY_TAB;
    io->KeyMap[ImGuiKey_LeftArrow] = KEY_LEFT;
    io->KeyMap[ImGuiKey_RightArrow] = KEY_RIGHT;
    io->KeyMap[ImGuiKey_UpArrow] = KEY_UP;
    io->KeyMap[ImGuiKey_DownArrow] = KEY_DOWN;
    io->KeyMap[ImGuiKey_PageUp] = KEY_PAGE_DOWN;
    io->KeyMap[ImGuiKey_PageDown] = KEY_PAGE_UP;
    io->KeyMap[ImGuiKey_Home] = KEY_HOME;
    io->KeyMap[ImGuiKey_End] = KEY_END;
    io->KeyMap[ImGuiKey_Insert] = KEY_INSERT;
    io->KeyMap[ImGuiKey_Delete] = KEY_DELETE;
    io->KeyMap[ImGuiKey_Backspace] = KEY_BACKSPACE;
    io->KeyMap[ImGuiKey_Space] = KEY_SPACE;
    io->KeyMap[ImGuiKey_Enter] = KEY_ENTER;
    io->KeyMap[ImGuiKey_Escape] = KEY_ESCAPE;
    io->KeyMap[ImGuiKey_KeyPadEnter] = KEY_KP_ENTER;
    io->KeyMap[ImGuiKey_A] = KEY_A;
    io->KeyMap[ImGuiKey_C] = KEY_C;
    io->KeyMap[ImGuiKey_V] = KEY_V;
    io->KeyMap[ImGuiKey_X] = KEY_X;
    io->KeyMap[ImGuiKey_Y] = KEY_Y;
    io->KeyMap[ImGuiKey_Z] = KEY_Z;

    io->MousePos = ImVec2(-FLT_MAX, -FLT_MAX);
    io->SetClipboardTextFn = ImGui_ImplRaylib_SetClipboardText;
    io->GetClipboardTextFn = ImGui_ImplRaylib_GetClipboardText;
    io->ClipboardUserData = NULL;

    for (int i = 0; i < fonts.size(); i++) {
        Texture2D* tex = ImGui_ImplRaylib_InitFontTexture(fonts[i], font_size);
        ImTextureID id = &(tex->id);
        io->Fonts->SetTexID(id);
    }

    return true;
}

void ImGui_ImplRaylib_Shutdown() {
    g_Time = 0.0;
}

static void ImGui_ImplRaylib_UpdateMouseCursor() {
    struct ImGuiIO* io = &ImGui::GetIO();
    if (io->ConfigFlags & ImGuiConfigFlags_NoMouseCursorChange)
        return;

    ImGuiMouseCursor imgui_cursor = ImGui::GetMouseCursor();
    if (io->MouseDrawCursor || imgui_cursor == ImGuiMouseCursor_None) {
        // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
        HideCursor();
    }
    else {
        // Show OS mouse cursor
        ShowCursor();
    }
}

static void ImGui_ImplRaylib_UpdateMousePosAndButtons() {
    struct ImGuiIO* io = &ImGui::GetIO();

    // Set OS mouse position if requested (rarely used, only when ImGuiConfigFlags_NavEnableSetMousePos is enabled by user)
    if (io->WantSetMousePos)
        SetMousePosition(io->MousePos.x, io->MousePos.y);
    else
        io->MousePos = ImVec2(-FLT_MAX, -FLT_MAX);

    io->MouseDown[0] = IsMouseButtonDown(MOUSE_LEFT_BUTTON);
    io->MouseDown[1] = IsMouseButtonDown(MOUSE_RIGHT_BUTTON);
    io->MouseDown[2] = IsMouseButtonDown(MOUSE_MIDDLE_BUTTON);

    if (!IsWindowMinimized()) {
        io->MousePos = ImVec2(GetTouchX(), GetTouchY());
    }
}

void ImGui_ImplRaylib_NewFrame() {
    struct ImGuiIO* io = &ImGui::GetIO();

    io->DisplaySize = ImVec2((float)GetScreenWidth(), (float)GetScreenHeight());

    double current_time = GetTime();
    io->DeltaTime = g_Time > 0.0 ? (float)(current_time - g_Time) : (float)(1.0f / 60.0f);
    g_Time = current_time;

    io->KeyCtrl = IsKeyDown(KEY_RIGHT_CONTROL) || IsKeyDown(KEY_LEFT_CONTROL);
    io->KeyShift = IsKeyDown(KEY_RIGHT_SHIFT) || IsKeyDown(KEY_LEFT_SHIFT);
    io->KeyAlt = IsKeyDown(KEY_RIGHT_ALT) || IsKeyDown(KEY_LEFT_ALT);
    io->KeySuper = IsKeyDown(KEY_RIGHT_SUPER) || IsKeyDown(KEY_LEFT_SUPER);

    ImGui_ImplRaylib_UpdateMousePosAndButtons();
    ImGui_ImplRaylib_UpdateMouseCursor();

    if (GetMouseWheelMove() > 0)
        io->MouseWheel += 1;
    else if (GetMouseWheelMove() < 0)
        io->MouseWheel -= 1;
}

#define FOR_ALL_KEYS(X) \
    X(KEY_APOSTROPHE); \
    X(KEY_COMMA); \
    X(KEY_MINUS); \
    X(KEY_PERIOD); \
    X(KEY_SLASH); \
    X(KEY_ZERO); \
    X(KEY_ONE); \
    X(KEY_TWO); \
    X(KEY_THREE); \
    X(KEY_FOUR); \
    X(KEY_FIVE); \
    X(KEY_SIX); \
    X(KEY_SEVEN); \
    X(KEY_EIGHT); \
    X(KEY_NINE); \
    X(KEY_SEMICOLON); \
    X(KEY_EQUAL); \
    X(KEY_A); \
    X(KEY_B); \
    X(KEY_C); \
    X(KEY_D); \
    X(KEY_E); \
    X(KEY_F); \
    X(KEY_G); \
    X(KEY_H); \
    X(KEY_I); \
    X(KEY_J); \
    X(KEY_K); \
    X(KEY_L); \
    X(KEY_M); \
    X(KEY_N); \
    X(KEY_O); \
    X(KEY_P); \
    X(KEY_Q); \
    X(KEY_R); \
    X(KEY_S); \
    X(KEY_T); \
    X(KEY_U); \
    X(KEY_V); \
    X(KEY_W); \
    X(KEY_X); \
    X(KEY_Y); \
    X(KEY_Z); \
    X(KEY_SPACE); \
    X(KEY_ESCAPE); \
    X(KEY_ENTER); \
    X(KEY_TAB); \
    X(KEY_BACKSPACE); \
    X(KEY_INSERT); \
    X(KEY_DELETE); \
    X(KEY_RIGHT); \
    X(KEY_LEFT); \
    X(KEY_DOWN); \
    X(KEY_UP); \
    X(KEY_PAGE_UP); \
    X(KEY_PAGE_DOWN); \
    X(KEY_HOME); \
    X(KEY_END); \
    X(KEY_CAPS_LOCK); \
    X(KEY_SCROLL_LOCK); \
    X(KEY_NUM_LOCK); \
    X(KEY_PRINT_SCREEN); \
    X(KEY_PAUSE); \
    X(KEY_F1); \
    X(KEY_F2); \
    X(KEY_F3); \
    X(KEY_F4); \
    X(KEY_F5); \
    X(KEY_F6); \
    X(KEY_F7); \
    X(KEY_F8); \
    X(KEY_F9); \
    X(KEY_F10); \
    X(KEY_F11); \
    X(KEY_F12); \
    X(KEY_LEFT_SHIFT); \
    X(KEY_LEFT_CONTROL); \
    X(KEY_LEFT_ALT); \
    X(KEY_LEFT_SUPER); \
    X(KEY_RIGHT_SHIFT); \
    X(KEY_RIGHT_CONTROL); \
    X(KEY_RIGHT_ALT); \
    X(KEY_RIGHT_SUPER); \
    X(KEY_KB_MENU); \
    X(KEY_LEFT_BRACKET); \
    X(KEY_BACKSLASH); \
    X(KEY_RIGHT_BRACKET); \
    X(KEY_GRAVE); \
    X(KEY_KP_0); \
    X(KEY_KP_1); \
    X(KEY_KP_2); \
    X(KEY_KP_3); \
    X(KEY_KP_4); \
    X(KEY_KP_5); \
    X(KEY_KP_6); \
    X(KEY_KP_7); \
    X(KEY_KP_8); \
    X(KEY_KP_9); \
    X(KEY_KP_DECIMAL); \
    X(KEY_KP_DIVIDE); \
    X(KEY_KP_MULTIPLY); \
    X(KEY_KP_SUBTRACT); \
    X(KEY_KP_ADD); \
    X(KEY_KP_ENTER); \
    X(KEY_KP_EQUAL);

#define SET_KEY_DOWN(KEY) io->KeysDown[KEY] = IsKeyDown(KEY)

bool ImGui_ImplRaylib_ProcessEvent() {
    struct ImGuiIO* io = &ImGui::GetIO();

    FOR_ALL_KEYS(SET_KEY_DOWN);

    int keyPressed = GetCharPressed();
    if (keyPressed > 0) {

        io->AddInputCharacter(keyPressed);
    }

    return true;
}

void draw_triangle_vertex(ImDrawVert idx_vert) {
    Color* c;
    c = (Color*)&idx_vert.col;

    rlColor4ub(c->r, c->g, c->b, c->a);
    rlTexCoord2f(idx_vert.uv.x, idx_vert.uv.y);
    rlVertex2f(idx_vert.pos.x, idx_vert.pos.y);
}

void raylib_render_draw_triangles(unsigned int count, const ImDrawIdx* idx_buffer, const ImDrawVert* idx_vert, unsigned int texture_id) {
    // Draw the imgui triangle data
    for (unsigned int i = 0; i <= (count - 3); i += 3) {
        rlPushMatrix();
        rlBegin(RL_TRIANGLES);
        rlEnableTexture(texture_id);

        ImDrawIdx index;
        ImDrawVert vertex;

        index = idx_buffer[i];
        vertex = idx_vert[index];
        draw_triangle_vertex(vertex);

        index = idx_buffer[i + 2];
        vertex = idx_vert[index];
        draw_triangle_vertex(vertex);

        index = idx_buffer[i + 1];
        vertex = idx_vert[index];
        draw_triangle_vertex(vertex);
        rlDisableTexture();
        rlEnd();
        rlPopMatrix();
    }
}

void raylib_render_imgui(ImDrawData* draw_data) {
    rlDisableBackfaceCulling();
    for (int n = 0; n < draw_data->CmdListsCount; n++) {
        const ImDrawList* cmd_list = draw_data->CmdLists[n];
        const ImDrawVert* vtx_buffer = cmd_list->VtxBuffer.Data; // vertex buffer generated by Dear ImGui
        const ImDrawIdx* idx_buffer = cmd_list->IdxBuffer.Data;  // index buffer generated by Dear ImGui
        for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++) {
            const ImDrawCmd* pcmd = &(cmd_list->CmdBuffer.Data)[cmd_i]; // cmd_list->CmdBuffer->data[cmd_i];
            if (pcmd->UserCallback) {
                pcmd->UserCallback(cmd_list, pcmd);
            }
            else {
                ImVec2 pos = draw_data->DisplayPos;
                int rectX = (int)(pcmd->ClipRect.x - pos.x);
                int rectY = (int)(pcmd->ClipRect.y - pos.y);
                int rectW = (int)(pcmd->ClipRect.z - rectX);
                int rectH = (int)(pcmd->ClipRect.w - rectY);
                BeginScissorMode(rectX, rectY, rectW, rectH);
                {
                    unsigned int ti = *((unsigned int*)(pcmd->TextureId));
                    raylib_render_draw_triangles(pcmd->ElemCount, idx_buffer, vtx_buffer, ti);
                }
            }
            idx_buffer += pcmd->ElemCount;
        }
    }
    EndScissorMode();
    rlEnableBackfaceCulling();
}

void raylib_draw_triangle_vertex(ImDrawVert idxVert) {
    Color c = GetColor(idxVert.col);
    rlColor4ub(c.r, c.g, c.b, c.a);
    rlTexCoord2f(idxVert.uv.x, idxVert.uv.y);
    rlVertex2f(idxVert.pos.x, idxVert.pos.y);
}

void raylib_draw_triangles(unsigned int count, ImVector<ImWchar> idxBuffer, ImVector<ImDrawVert> idxVert, int idxOffset, int vtxOffset, unsigned int textureId) {
    ImWchar index;
    ImDrawVert vertex;

    if (rlCheckBufferLimit((int)count) * 3) {
        rlglDraw();
    }

    rlPushMatrix();
    rlBegin(RL_TRIANGLES);
    rlEnableTexture(textureId);

    for (int i = 0; i <= (count - 3); i += 3) {
        index = idxBuffer[idxOffset + i];
        vertex = idxVert[vtxOffset + index];
        raylib_draw_triangle_vertex(vertex);

        index = idxBuffer[idxOffset + i + 2];
        vertex = idxVert[vtxOffset + index];
        raylib_draw_triangle_vertex(vertex);

        index = idxBuffer[idxOffset + i + 1];
        vertex = idxVert[vtxOffset + index];
        raylib_draw_triangle_vertex(vertex);
    }

    rlDisableTexture();
    rlEnd();
    rlPopMatrix();
}

void raylib_render_imgui_own(ImDrawData* draw_data) {

    int fbWidth = (int)(draw_data->DisplaySize.x * draw_data->FramebufferScale.x);
    int fbHeight = (int)(draw_data->DisplaySize.y * draw_data->FramebufferScale.y);

    rlDisableBackfaceCulling();

    for (int n = 0; n < draw_data->CmdListsCount; n++) {
        ImDrawList* cmd_list = draw_data->CmdLists[n];

        ImVector<ImDrawVert> txBuffer = cmd_list->VtxBuffer;
        ImVector<ImWchar> idxBuffer = cmd_list->IdxBuffer;

        for (int cmdi = 0; cmdi < cmd_list->CmdBuffer.Size; cmdi++) {
            ImDrawCmd pcmd = cmd_list->CmdBuffer[cmdi];
            if (pcmd.UserCallback != NULL) {
                pcmd.UserCallback(cmd_list, NULL);
            }
            else {
                ImVec2 pos = draw_data->DisplayPos;

                int rectX = (pcmd.ClipRect.x - pos.x) * draw_data->FramebufferScale.x;
                int rectY = (pcmd.ClipRect.y - pos.y) * draw_data->FramebufferScale.y;
                int rectW = (pcmd.ClipRect.z - rectX) * draw_data->FramebufferScale.x;
                int rectH = (pcmd.ClipRect.w - rectY) * draw_data->FramebufferScale.y;

                if (rectX < fbWidth && rectY < fbHeight && rectW >= 0.0F && rectH >= 0.0F) {
                    BeginScissorMode(rectX, rectY, rectW, rectH);
                    raylib_draw_triangles(pcmd.ElemCount, idxBuffer, txBuffer, pcmd.IdxOffset, pcmd.VtxOffset, *((unsigned int*)pcmd.TextureId));
                }
            }
        }
    }

    EndScissorMode();
    rlEnableBackfaceCulling();
}