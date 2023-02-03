using static LogiX.OpenGL.GL;

using LogiX.Graphics;
using System.Numerics;

namespace LogiX.Rendering;

public class Framebuffer
{
    private uint _framebuffer;
    private static uint _quadVAO;
    private uint renderedTexture;

    private int width;
    private int height;

    public Framebuffer(bool acquireGLContext = false)
    {
        DisplayManager.OnFramebufferResize += (window, size) =>
        {
            DisplayManager.LockedGLContext(() =>
            {
                this.Resize((int)size.X, (int)size.Y);
                this.width = (int)size.X;
                this.height = (int)size.Y;
            });
        };

        if (acquireGLContext)
        {
            DisplayManager.LockedGLContext(() =>
            {
                var size = DisplayManager.GetWindowSizeInPixels();
                this.InitGL((int)size.X, (int)size.Y);
                this.width = (int)size.X;
                this.height = (int)size.Y;
            });
        }
        else
        {
            var size = DisplayManager.GetWindowSizeInPixels();
            this.InitGL((int)size.X, (int)size.Y);
            this.width = (int)size.X;
            this.height = (int)size.Y;
        }
    }

    public Framebuffer(int width, int height, bool acquireGLContext = false)
    {
        if (acquireGLContext)
        {
            DisplayManager.LockedGLContext(() =>
            {
                this.InitGL(width, height);
                this.width = width;
                this.height = height;
            });
        }
        else
        {
            this.InitGL(width, height);
            this.width = width;
            this.height = height;
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

    private uint textureColorBufferMultiSampled;
    private uint intermediateFBO;
    public unsafe void InitGL(int width, int height)
    {
        InitQuad();

        this._framebuffer = glGenFramebuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, this._framebuffer);

        this.textureColorBufferMultiSampled = glGenTexture();
        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, this.textureColorBufferMultiSampled);
        glTexImage2DMultisample(GL_TEXTURE_2D_MULTISAMPLE, 4, GL_RGBA, width, height, true);
        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, 0);
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D_MULTISAMPLE, this.textureColorBufferMultiSampled, 0);

        uint rbo = glGenRenderbuffer();
        glBindRenderbuffer(rbo);
        glRenderbufferStorageMultisample(GL_RENDERBUFFER, 4, GL_DEPTH24_STENCIL8, width, height);
        glBindRenderbuffer(0);
        glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rbo);

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("Framebuffer is not complete!");
        }

        glBindFramebuffer(GL_FRAMEBUFFER, 0);

        // configure second post-processing framebuffer
        this.intermediateFBO = glGenFramebuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, this.intermediateFBO);

        this.renderedTexture = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, this.renderedTexture);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, this.renderedTexture, 0);

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("Framebuffer is not complete!");
        }

        glBindFramebuffer(GL_FRAMEBUFFER, 0);


        //------------------ OLD
        // this.renderedTexture = glGenTexture();
        // glBindTexture(GL_TEXTURE_2D, this.renderedTexture);

        // glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

        // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
        // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

        // glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, this.renderedTexture, 0);
        // glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    private unsafe void Resize(int width, int height)
    {
        // glBindTexture(GL_TEXTURE_2D, this.renderedTexture);
        // glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);
        // glBindTexture(GL_TEXTURE_2D, 0);

        _defaultCam = new Camera2D(new Vector2(width / 2f, height / 2f), 1f);
    }

    public uint GetTexture()
    {
        return this.renderedTexture;
    }

    public void Bind(Action performInBuffer)
    {
        // Get old viewport
        int[] oldViewport = glGetIntegerv(GL_VIEWPORT, 4);
        var prev = GetCurrentBoundBuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, this._framebuffer);
        glViewport(0, 0, this.width, this.height);
        performInBuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, prev);
        glViewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
    }

    // STATIC STUFF
    public static void BindDefaultFramebuffer()
    {
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
        glBindFramebuffer(GL_READ_FRAMEBUFFER, 0);
        glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
    }

    public static uint GetCurrentBoundBuffer()
    {
        int[] buffer = glGetIntegerv(GL_FRAMEBUFFER_BINDING, 1);
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

        shader.Use(() =>
        {
            glBindFramebuffer(GL_READ_FRAMEBUFFER, framebuffer._framebuffer);
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, framebuffer.intermediateFBO);
            glBlitFramebuffer(0, 0, framebuffer.width, framebuffer.height, 0, 0, framebuffer.width, framebuffer.height, GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT, GL_NEAREST);

            BindDefaultFramebuffer();

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