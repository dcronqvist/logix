using System;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;

namespace LogiX.UI
{
    /// <summary>
    /// ImGui controller for Raylib-cs.
    /// </summary>
    public class ImGUIController : IDisposable
    {
        IntPtr context;
        Texture2D fontTexture;
        Vector2 size;
        Vector2 scaleFactor = Vector2.One;

        public ImGUIController()
        {
            context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            ImGui.GetIO().Fonts.AddFontDefault();
        }

        public void Dispose()
        {
            ImGui.DestroyContext(context);
            Raylib.UnloadTexture(fontTexture);
        }

        /// <summary>
        /// Creates a texture and loads the font data from ImGui.
        /// </summary>
        public void Load(int width, int height)
        {
            size = new Vector2(width, height);
            LoadFontTexture();
            SetupInput();
            ImGui.NewFrame();
        }

        unsafe void LoadFontTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders.
            // If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

            // Upload texture to graphics system
            IntPtr data = new IntPtr(pixels);
            Image image = new Image
            {
                data = data,
                width = width,
                height = height,
                mipmaps = 1,
                format = PixelFormat.UNCOMPRESSED_R8G8B8A8,
            };
            fontTexture = Raylib.LoadTextureFromImage(image);

            // Store our identifier
            io.Fonts.SetTexID(new IntPtr(fontTexture.id));

            // Clears font data on the CPU side
            io.Fonts.ClearTexData();
        }

        void SetupInput()
        {
            // Setup back-end capabilities flags
            ImGuiIOPtr io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

            // Keyboard mapping. ImGui will use those indices to peek into the io.KeysDown[] array.
            io.KeyMap[(int)ImGuiKey.Tab] = (int)KeyboardKey.KEY_TAB;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)KeyboardKey.KEY_LEFT;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)KeyboardKey.KEY_RIGHT;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)KeyboardKey.KEY_UP;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)KeyboardKey.KEY_DOWN;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)KeyboardKey.KEY_PAGE_UP;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)KeyboardKey.KEY_PAGE_DOWN;
            io.KeyMap[(int)ImGuiKey.Home] = (int)KeyboardKey.KEY_HOME;
            io.KeyMap[(int)ImGuiKey.End] = (int)KeyboardKey.KEY_END;
            io.KeyMap[(int)ImGuiKey.Insert] = (int)KeyboardKey.KEY_INSERT;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)KeyboardKey.KEY_DELETE;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)KeyboardKey.KEY_BACKSPACE;
            io.KeyMap[(int)ImGuiKey.Space] = (int)KeyboardKey.KEY_SPACE;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)KeyboardKey.KEY_ENTER;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)KeyboardKey.KEY_ESCAPE;
            io.KeyMap[(int)ImGuiKey.A] = (int)KeyboardKey.KEY_A;
            io.KeyMap[(int)ImGuiKey.C] = (int)KeyboardKey.KEY_C;
            io.KeyMap[(int)ImGuiKey.V] = (int)KeyboardKey.KEY_V;
            io.KeyMap[(int)ImGuiKey.X] = (int)KeyboardKey.KEY_X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)KeyboardKey.KEY_Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)KeyboardKey.KEY_Z;
        }

        public void Update(float dt)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            SetPerFrameData(dt);
            UpdateInput();

            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        void SetPerFrameData(float dt)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = size / scaleFactor;
            io.DisplayFramebufferScale = Vector2.One;
            io.DeltaTime = dt;
        }

        void UpdateInput()
        {
            UpdateMousePosAndButtons();
            UpdateMouseCursor();
            UpdateGamepads();

            int keyPressed = Raylib.GetKeyPressed();
            if (keyPressed > 0)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                io.AddInputCharacter((uint)keyPressed);
            }
        }

        void UpdateMousePosAndButtons()
        {
            // Update mouse buttons
            ImGuiIOPtr io = ImGui.GetIO();
            for (int i = 0; i < io.MouseDown.Count; i++)
            {
                io.MouseDown[i] = Raylib.IsMouseButtonDown((MouseButton)i);
            }

            // Modifiers are not reliable across systems
            io.KeyCtrl = io.KeysDown[(int)KeyboardKey.KEY_LEFT_CONTROL] || io.KeysDown[(int)KeyboardKey.KEY_RIGHT_CONTROL];
            io.KeyShift = io.KeysDown[(int)KeyboardKey.KEY_LEFT_SHIFT] || io.KeysDown[(int)KeyboardKey.KEY_RIGHT_SHIFT];
            io.KeyAlt = io.KeysDown[(int)KeyboardKey.KEY_LEFT_ALT] || io.KeysDown[(int)KeyboardKey.KEY_RIGHT_ALT];
            io.KeySuper = io.KeysDown[(int)KeyboardKey.KEY_LEFT_SUPER] || io.KeysDown[(int)KeyboardKey.KEY_RIGHT_SUPER];

            // Mouse scroll
            io.MouseWheel += (float)Raylib.GetMouseWheelMove();

            // Key states
            for (int i = (int)KeyboardKey.KEY_SPACE; i < (int)KeyboardKey.KEY_KB_MENU + 1; i++)
            {
                io.KeysDown[i] = Raylib.IsKeyDown((KeyboardKey)i);
            }

            // Update mouse position
            Vector2 mousePositionBackup = io.MousePos;
            io.MousePos = new Vector2(-float.MaxValue, -float.MaxValue);
            const bool focused = true;

            if (focused)
            {
                if (io.WantSetMousePos)
                {
                    Raylib.SetMousePosition((int)mousePositionBackup.X, (int)mousePositionBackup.Y);
                }
                else
                {
                    io.MousePos = Raylib.GetMousePosition();
                }
            }
        }

        void UpdateMouseCursor()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) == 0 || Raylib.IsCursorHidden())
            {
                return;
            }

            ImGuiMouseCursor imgui_cursor = ImGui.GetMouseCursor();
            if (imgui_cursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
            {
                Raylib.HideCursor();
            }
            else
            {
                Raylib.ShowCursor();
            }
        }

        void UpdateGamepads()
        {
            ImGuiIOPtr io = ImGui.GetIO();
        }

        /// <summary>
        /// Gets the geometry as set up by ImGui and sends it to the graphics device
        /// </summary>
        public void Draw()
        {
            ImGui.Render();
            unsafe { RenderCommandLists(ImGui.GetDrawData()); }
        }

        // Returns a Color struct from hexadecimal value
        Color GetColor(uint hexValue)
        {
            Color color;

            color.r = (byte)(hexValue & 0xFF);
            color.g = (byte)((hexValue >> 8) & 0xFF);
            color.b = (byte)((hexValue >> 16) & 0xFF);
            color.a = (byte)((hexValue >> 24) & 0xFF);

            return color;
        }

        void DrawTriangleVertex(ImDrawVertPtr idxVert)
        {
            Color c = GetColor(idxVert.col);
            Rlgl.rlColor4ub(c.r, c.g, c.b, c.a);
            Rlgl.rlTexCoord2f(idxVert.uv.X, idxVert.uv.Y);
            Rlgl.rlVertex2f(idxVert.pos.X, idxVert.pos.Y);
        }

        // Draw the imgui triangle data
        void DrawTriangles(uint count, ImVector<ushort> idxBuffer, ImPtrVector<ImDrawVertPtr> idxVert, int idxOffset, int vtxOffset, IntPtr textureId)
        {
            uint texId = (uint)textureId;
            ushort index;
            ImDrawVertPtr vertex;

            if (Rlgl.rlCheckBufferLimit((int)count * 3))
            {
                Rlgl.rlglDraw();
            }

            Rlgl.rlPushMatrix();
            Rlgl.rlBegin(Rlgl.RL_TRIANGLES);
            Rlgl.rlEnableTexture(texId);

            for (int i = 0; i <= (count - 3); i += 3)
            {
                index = idxBuffer[idxOffset + i];
                vertex = idxVert[vtxOffset + index];
                DrawTriangleVertex(vertex);

                index = idxBuffer[idxOffset + i + 2];
                vertex = idxVert[vtxOffset + index];
                DrawTriangleVertex(vertex);

                index = idxBuffer[idxOffset + i + 1];
                vertex = idxVert[vtxOffset + index];
                DrawTriangleVertex(vertex);
            }

            Rlgl.rlDisableTexture();
            Rlgl.rlEnd();
            Rlgl.rlPopMatrix();
        }

        unsafe void RenderCommandLists(ImDrawDataPtr drawData)
        {
            // Scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
            int fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
            int fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);

            // Avoid rendering if display is minimized or if the command list is empty
            if (fbWidth <= 0 || fbHeight <= 0 || drawData.CmdListsCount == 0)
            {
                return;
            }

            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);
            Rlgl.rlDisableBackfaceCulling();

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                // Vertex buffer and index buffer generated by Dear ImGui
                ImPtrVector<ImDrawVertPtr> vtxBuffer = cmdList.VtxBuffer;
                ImVector<ushort> idxBuffer = cmdList.IdxBuffer;

                for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdi];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        // pcmd.UserCallback(cmdList, pcmd);
                    }
                    else
                    {
                        Vector2 pos = drawData.DisplayPos;
                        int rectX = (int)((pcmd.ClipRect.X - pos.X) * drawData.FramebufferScale.X);
                        int rectY = (int)((pcmd.ClipRect.Y - pos.Y) * drawData.FramebufferScale.Y);
                        int rectW = (int)((pcmd.ClipRect.Z - rectX) * drawData.FramebufferScale.X);
                        int rectH = (int)((pcmd.ClipRect.W - rectY) * drawData.FramebufferScale.Y);

                        if (rectX < fbWidth && rectY < fbHeight && rectW >= 0.0f && rectH >= 0.0f)
                        {
                            Raylib.BeginScissorMode(rectX, rectY, rectW, rectH);
                            DrawTriangles(pcmd.ElemCount, idxBuffer, vtxBuffer, (int)pcmd.IdxOffset, (int)pcmd.VtxOffset, pcmd.TextureId);
                        }
                    }
                }
            }

            Raylib.EndScissorMode();
            Rlgl.rlEnableBackfaceCulling();
        }
    }
}
