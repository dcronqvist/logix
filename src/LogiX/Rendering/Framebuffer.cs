using static LogiX.OpenGL.GL;

using LogiX.Graphics;
using System.Numerics;

namespace LogiX.Rendering;

public class Framebuffer
{
    private uint _framebuffer;
    private static uint _quadVAO;
    private uint renderedTexture;

    public Framebuffer(bool acquireGLContext = false)
    {
        DisplayManager.OnFramebufferResize += (window, size) =>
        {
            DisplayManager.LockedGLContext(() =>
            {
                this.Resize((int)size.X, (int)size.Y);
            });
        };

        if (acquireGLContext)
        {
            DisplayManager.LockedGLContext(() =>
            {
                this.InitGL();
            });
        }
        else
        {
            this.InitGL();
        }
    }

    private static Camera2D _defaultCam;
    public static Camera2D GetDefaultCamera()
    {
        if (_defaultCam == null)
        {
            _defaultCam = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2f, 1f);
        }

        return _defaultCam;
    }

    public static unsafe void InitQuad()
    {
        if (_quadVAO != 0)
        {
            return;
        }

        float[] vertices = { 
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 1.0f
        };

        _quadVAO = glGenVertexArray();
        glBindVertexArray(_quadVAO);

        uint vbo = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, vbo);

        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
        }

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
    }

    public unsafe void InitGL()
    {
        InitQuad();

        var size = DisplayManager.GetWindowSizeInPixels();
        int width = (int)size.X;
        int height = (int)size.Y;

        this._framebuffer = glGenFramebuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, this._framebuffer);

        this.renderedTexture = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, this.renderedTexture);

        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, this.renderedTexture, 0);
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    private unsafe void Resize(int width, int height)
    {
        glBindTexture(GL_TEXTURE_2D, this.renderedTexture);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);
        glBindTexture(GL_TEXTURE_2D, 0);

        _defaultCam = new Camera2D(new Vector2(width / 2f, height / 2f), 1f);
    }

    public uint GetTexture()
    {
        return this.renderedTexture;
    }

    public void Bind(Action performInBuffer)
    {
        var prev = GetCurrentBoundBuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, this._framebuffer);
        performInBuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, prev);
    }

    // STATIC STUFF
    public static void BindDefaultFramebuffer()
    {
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    public static uint GetCurrentBoundBuffer()
    {
        int[] buffer = new int[1];
        glGetIntegerv(GL_FRAMEBUFFER_BINDING, 1);
        return (uint)buffer[0];
    }

    public static void Clear(ColorF color)
    {
        glClearColor(color.R, color.G, color.B, color.A);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    }

    private static bool IsShaderValid(ShaderProgram shader, out string[] missingAttribs, out string[] missingUniforms)
    {
        bool hasAttribs = shader.HasAttribs(out missingAttribs, ("aPos", "vec2", 0), ("aTexCoords", "vec2", 1));
        bool hasUniforms = shader.HasUniforms(out missingUniforms, ("screenTexture", "sampler2D"));

        return hasAttribs && hasUniforms;
    }

    public static void RenderFrameBufferToScreen(ShaderProgram shader, Framebuffer framebuffer)
    {
        if (!IsShaderValid(shader, out var missingAttribs, out var missingUn))
        {
            throw new ArgumentException($"Shader is invalid! Missing attributes: {string.Join(", ", missingAttribs)}, missing uniforms: {string.Join(", ", missingUn)}");
        }

        var oldBuffer = GetCurrentBoundBuffer();

        BindDefaultFramebuffer();

        shader.Use(() =>
        {
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, framebuffer.renderedTexture);
            shader.SetInt("screenTexture", 0);

            glBindVertexArray(_quadVAO);
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        });

        glBindFramebuffer(GL_FRAMEBUFFER, oldBuffer);
    }
}