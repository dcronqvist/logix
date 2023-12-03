using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using DotGLFW;
using ImGuiNET;
using LogiX.Graphics.Text;
using LogiX.Input;
using static DotGL.GL;

namespace LogiX.UserInterfaceContext;

/// <summary>
/// A modified version of Veldrid.ImGui's ImGuiRenderer.
/// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
/// </summary>
public class OpenGLImGuiController : IImGuiController
{
    private bool _frameBegun;
    private int _vertexBufferSize;
    private int _indexBufferSize;

    public uint _fontTexture;
    private uint _shader;

    private int _windowWidth;
    private int _windowHeight;

    private Vector2 _scaleFactor = Vector2.One;

    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;
    private readonly IKeyboard<char, Keys, ModifierKeys> _keyboard;
    private readonly IMouse<MouseButton> _mouse;

    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public unsafe OpenGLImGuiController(
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext,
        IKeyboard<char, Keys, ModifierKeys> keyboard,
        IMouse<MouseButton> mouse)
    {
        _keyboard = keyboard;
        _mouse = mouse;
        _userInterfaceContext = userInterfaceContext;

        _windowWidth = userInterfaceContext.GetWindowWidth();
        _windowHeight = userInterfaceContext.GetWindowHeight();

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        var io = ImGui.GetIO();

        io.Fonts.AddFontDefault();
        byte[] forkawesomeTTFBytes = GetEmbeddedTTFBytes("LogiX._embeds.forkawesome-webfont.ttf");
        ImFontConfigPtr configuration = ImGuiNative.ImFontConfig_ImFontConfig();

        configuration.MergeMode = true;
        configuration.GlyphMinAdvanceX = 13.0f;
        configuration.GlyphMaxAdvanceX = 13.0f;
        configuration.GlyphOffset = new Vector2(0.0f, 1.0f);

        var ranges = new ushort[]{
            0xf000, 0xf372
        };

        fixed (void* p = &forkawesomeTTFBytes[0], r = &ranges[0])
        {
            io.Fonts.AddFontFromMemoryTTF((nint)p, forkawesomeTTFBytes.Length, 13.0f, configuration, (nint)r);
        }

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        // io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        // Remove imgui.ini
        io.NativePtr->IniFilename = null;

        CreateDeviceResources();
        SetPerFrameImGuiData(1f / 60f);

        ImGui.NewFrame();
        _frameBegun = true;

        _keyboard.CharacterTyped += (sender, e) =>
        {
            PressChar(e);
        };

        _mouse.MouseWheelScrolled += (sender, e) =>
        {
            MouseScroll(new Vector2(0f, e));
        };

        _userInterfaceContext.WindowSizeChanged += (sender, e) =>
        {
            WindowResized(e.Item1, e.Item2);
        };

        _keyboard.KeyPressed += (sender, e) =>
        {
            io.AddKeyEvent(MapGLFWKeysToImGuiKey(e.Item1), true);
        };

        _keyboard.KeyReleased += (sender, e) =>
        {
            io.AddKeyEvent(MapGLFWKeysToImGuiKey(e.Item1), false);
        };
    }

    private static ImGuiKey MapGLFWKeysToImGuiKey(Keys key) => key switch
    {
        Keys.Backspace => ImGuiKey.Backspace,
        Keys.Tab => ImGuiKey.Tab,
        Keys.Enter => ImGuiKey.Enter,
        Keys.CapsLock => ImGuiKey.CapsLock,
        Keys.Escape => ImGuiKey.Escape,
        Keys.Space => ImGuiKey.Space,
        Keys.PageUp => ImGuiKey.PageUp,
        Keys.PageDown => ImGuiKey.PageDown,
        Keys.End => ImGuiKey.End,
        Keys.Home => ImGuiKey.Home,
        Keys.Left => ImGuiKey.LeftArrow,
        Keys.Right => ImGuiKey.RightArrow,
        Keys.Up => ImGuiKey.UpArrow,
        Keys.Down => ImGuiKey.DownArrow,
        Keys.PrintScreen => ImGuiKey.PrintScreen,
        Keys.Insert => ImGuiKey.Insert,
        Keys.Delete => ImGuiKey.Delete,
        >= Keys.D0 and <= Keys.D9 => ImGuiKey._0 + (key - Keys.D0),
        >= Keys.A and <= Keys.Z => ImGuiKey.A + (key - Keys.A),
        Keys.KpMultiply => ImGuiKey.KeypadMultiply,
        Keys.KpAdd => ImGuiKey.KeypadAdd,
        Keys.KpSubtract => ImGuiKey.KeypadSubtract,
        Keys.KpDecimal => ImGuiKey.KeypadDecimal,
        Keys.KpDivide => ImGuiKey.KeypadDivide,
        >= Keys.F1 and <= Keys.F24 => ImGuiKey.F1 + (key - Keys.F1),
        Keys.NumLock => ImGuiKey.NumLock,
        Keys.ScrollLock => ImGuiKey.ScrollLock,
        Keys.LeftShift => ImGuiKey.ModShift,
        Keys.LeftControl => ImGuiKey.ModCtrl,
        Keys.LeftAlt => ImGuiKey.ModAlt,
        Keys.SemiColon => ImGuiKey.Semicolon,
        Keys.Comma => ImGuiKey.Comma,
        Keys.Minus => ImGuiKey.Minus,
        Keys.Period => ImGuiKey.Period,
        _ => ImGuiKey.None,
    };

    private static byte[] GetEmbeddedTTFBytes(string embedFileName)
    {
        var assembly = typeof(OpenGLImGuiController).Assembly;
        var embeddedResourceStream = assembly.GetManifestResourceStream(embedFileName)!;
        using var ms = new MemoryStream();
        embeddedResourceStream.CopyTo(ms);
        return ms.ToArray();
    }

    private void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    private uint _vao;
    private uint _vbo;
    private uint _ebo;

    private unsafe uint CreateShader(int shadertype, string source)
    {
        // Create either fragment or vertex shader
        var shader = glCreateShader(shadertype);

        // Set shader source
        glShaderSource(shader, source);

        // Compile shader
        glCompileShader(shader);

        return shader;
    }

    private unsafe uint CreateProgram(uint vertex, uint fragment)
    {
        // Create shader program
        var program = glCreateProgram();

        // Attach shaders
        glAttachShader(program, vertex);
        glAttachShader(program, fragment);

        // Link program
        glLinkProgram(program);

        // Delete shaders
        glDeleteShader(vertex);
        glDeleteShader(fragment);

        // Use program
        glUseProgram(program);

        return program;
    }

    private unsafe void CreateDeviceResources()
    {
        _vao = glGenVertexArray();

        _vertexBufferSize = 10000;
        _indexBufferSize = 2000;

        _vbo = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);
        glBufferData(GL_ARRAY_BUFFER, _vertexBufferSize, null, GL_DYNAMIC_DRAW);

        _ebo = glGenBuffer();
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, _indexBufferSize, null, GL_DYNAMIC_DRAW);

        glBindVertexArray(0);

        RecreateFontDeviceTexture();

        string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
        string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

        var vs = CreateShader(GL_VERTEX_SHADER, VertexSource);
        var fs = CreateShader(GL_FRAGMENT_SHADER, FragmentSource);
        _shader = CreateProgram(vs, fs);

        glBindVertexArray(_vao);
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, Unsafe.SizeOf<ImDrawVert>(), (void*)0);

        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 2, GL_FLOAT, false, Unsafe.SizeOf<ImDrawVert>(), (void*)8);

        glEnableVertexAttribArray(2);
        glVertexAttribPointer(2, 4, GL_UNSIGNED_BYTE, true, Unsafe.SizeOf<ImDrawVert>(), (void*)16);
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    private unsafe void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        // Create opengl texture with data
        _fontTexture = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, _fontTexture);

        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, (void*)pixels);

        // Set texture options
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_LINEAR);

        // Store our identifier
        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();
    }

    /// <summary>
    /// Renders the ImGui draw list data.
    /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
    /// or index data has increased beyond the capacity of the existing buffers.
    /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
    /// </summary>
    public void Render()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
            ImGui.EndFrame();
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(float deltaSeconds)
    {
        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    readonly List<char> PressedChars = new List<char>();

    private bool IsMouseButtonDown(MouseButton button)
    {
        return _mouse.IsMouseButtonDown(button);
    }

    private bool IsKeyDown(Keys key)
    {
        return _keyboard.IsKeyDown(key);
    }

    private Vector2 GetMousePositionInWindow()
    {
        int x = _mouse.GetMouseXInWindow();
        int y = _mouse.GetMouseYInWindow();
        return new Vector2((float)x, (float)y);
    }

    private void UpdateImGuiInput()
    {
        var io = ImGui.GetIO();

        bool focused = _userInterfaceContext.IsWindowFocused();

        io.SetAppAcceptingEvents(focused);
        io.AddFocusEvent(focused);

        var mousePos = GetMousePositionInWindow();
        io.AddMousePosEvent(mousePos.X, mousePos.Y);
        io.AddMouseButtonEvent(0, _mouse.IsMouseButtonDown(MouseButton.Left));
        io.AddMouseButtonEvent(1, _mouse.IsMouseButtonDown(MouseButton.Right));
        io.AddMouseButtonEvent(2, _mouse.IsMouseButtonDown(MouseButton.Middle));

        foreach (var c in PressedChars)
        {
            io.AddInputCharacter(c);
        }
        PressedChars.Clear();

        io.KeyCtrl = IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);
        io.KeyAlt = IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);
        io.KeyShift = IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        io.KeySuper = IsKeyDown(Keys.LeftSuper) || IsKeyDown(Keys.RightSuper);
    }

    private void PressChar(char keyChar)
    {
        PressedChars.Add(keyChar);
    }

    private void MouseScroll(Vector2 offset)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseWheel = offset.Y;
        io.MouseWheelH = offset.X;
    }

    private unsafe void RenderImDrawData(ImDrawDataPtr draw_data)
    {
        if (draw_data.CmdListsCount == 0)
        {
            return;
        }

        for (int i = 0; i < draw_data.CmdListsCount; i++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[i];

            int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                glBindBuffer(GL_ARRAY_BUFFER, _vbo);
                glBufferData(GL_ARRAY_BUFFER, newSize, DotGL.GL.NULL, GL_DYNAMIC_DRAW);
                _vertexBufferSize = newSize;
            }

            int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, newSize, DotGL.GL.NULL, GL_DYNAMIC_DRAW);
                _indexBufferSize = newSize;
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);


        glUseProgram(_shader);


        var loc = glGetUniformLocation(_shader, "projection_matrix");
        glUniformMatrix4fv(loc, 1, false, (float*)&mvp);

        loc = glGetUniformLocation(_shader, "in_fontTexture");
        glUniform1i(loc, 0);

        glBindVertexArray(_vao);
        draw_data.ScaleClipRects(io.DisplayFramebufferScale);

        int[] oldBlend = new int[1];
        glGetIntegerv(GL_BLEND, ref oldBlend);
        glEnable(GL_BLEND);

        int[] oldScissor = new int[1];
        glGetIntegerv(GL_SCISSOR_TEST, ref oldScissor);
        glEnable(GL_SCISSOR_TEST);

        int[] oldBlendEquation = new int[1];
        glGetIntegerv(GL_BLEND_EQUATION, ref oldBlendEquation);

        int[] oldBlendSrc = new int[1];
        glGetIntegerv(GL_BLEND_SRC, ref oldBlendSrc);

        int[] oldBlendDst = new int[1];
        glGetIntegerv(GL_BLEND_DST, ref oldBlendDst);

        glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);

        int[] oldCullFace = new int[1];
        glGetIntegerv(GL_CULL_FACE, ref oldCullFace);
        glDisable(GL_CULL_FACE);

        int[] oldDepthTest = new int[1];
        glGetIntegerv(GL_DEPTH_TEST, ref oldDepthTest);
        glDisable(GL_DEPTH_TEST);

        // Render command lists
        for (int n = 0; n < draw_data.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];

            //GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
            glBindBuffer(GL_ARRAY_BUFFER, _vbo);
            glBufferData(GL_ARRAY_BUFFER, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), (void*)cmd_list.VtxBuffer.Data, GL_DYNAMIC_DRAW);

            //GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, cmd_list.IdxBuffer.Size * sizeof(ushort), (void*)cmd_list.IdxBuffer.Data, GL_DYNAMIC_DRAW);
            //Util.CheckGLError($"Data Idx {n}");

            for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    //GL.ActiveTexture(TextureUnit.Texture0);
                    glActiveTexture(GL_TEXTURE0);
                    //GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                    glBindTexture(GL_TEXTURE_2D, (uint)pcmd.TextureId);
                    //Util.CheckGLError("Texture");

                    // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                    var clip = pcmd.ClipRect;
                    //GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                    glScissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                    //Util.CheckGLError("Scissor");

                    if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                    {
                        //GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                        glDrawElementsBaseVertex(GL_TRIANGLES, (int)pcmd.ElemCount, GL_UNSIGNED_SHORT, (void*)(pcmd.IdxOffset * sizeof(ushort)), (int)pcmd.VtxOffset);
                    }
                    else
                    {
                        //GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        glDrawElements(GL_TRIANGLES, (int)pcmd.ElemCount, GL_UNSIGNED_INT, (void*)(pcmd.IdxOffset * sizeof(ushort)));
                    }
                    //Util.CheckGLError("Draw");
                }

                //idx_offset += (int)pcmd.ElemCount;
            }
        }

        //GL.Disable(EnableCap.Blend);
        //glDisable(GL_BLEND);
        //GL.Disable(EnableCap.ScissorTest);
        //glDisable(GL_SCISSOR_TEST);
        if (oldBlend[0] == 0)
        {
            glDisable(GL_BLEND);
        }
        if (oldScissor[0] == 0)
        {
            glDisable(GL_SCISSOR_TEST);
        }
        glBlendEquation(oldBlendEquation[0]);
        glBlendFunc(oldBlendSrc[0], oldBlendDst[0]);
        if (oldCullFace[0] == 1)
        {
            glEnable(GL_CULL_FACE);
        }
        if (oldDepthTest[0] == 1)
        {
            glEnable(GL_DEPTH_TEST);
        }

        glUseProgram(0);
    }
}

public static class ImGuiExt
{
    public static void ImageFlipVertical(IntPtr userTextureId, Vector2 size)
    {
        var uv = new Vector2(0, 1f);
        var uv2 = new Vector2(1f, 0f);
        var tint_col = new Vector4(1f, 1f, 1f, 1f);
        ImGuiNative.igImage(userTextureId, size, uv, uv2, tint_col, default);
    }

    public static void MouseTooltip(Action submit)
    {
        ImGui.BeginTooltip();
        submit();
        ImGui.EndTooltip();
    }

    public static void ImGuiHelp(string text)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(text);
            ImGui.EndTooltip();
        }
    }
}
