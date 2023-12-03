using System;
using System.Numerics;
using LogiX.Graphics.Textures;
using static DotGL.GL;

namespace LogiX.Graphics.Framebuffers;

public partial class Framebuffer : IFramebuffer
{
    private class Texture2DFramebufferProxy : ITexture2D
    {
        private Framebuffer _framebuffer;
        public Texture2DFramebufferProxy(Framebuffer framebuffer)
        {
            _framebuffer = framebuffer;
        }

        public uint OpenGLID
        {
            get
            {
                Blit();
                return _framebuffer._renderedTexture;
            }
        }
        public int Width => (int)_framebuffer._currentSize.X;
        public int Height => (int)_framebuffer._currentSize.Y;

        public byte[] GetPixelData()
        {
            throw new InvalidOperationException("Cannot get pixel data from a framebuffer texture!");
        }

        private void Blit()
        {
            uint previous = GetCurrentBoundBuffer();

            glBindFramebuffer(GL_READ_FRAMEBUFFER, _framebuffer._framebuffer);
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, _framebuffer._intermediateFBO);
            glBlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT, GL_NEAREST);

            glBindFramebuffer(GL_FRAMEBUFFER, previous);
        }
    }

    private static uint _quadVAO;

    private uint _textureColorBufferMultiSampled;
    private uint _intermediateFBO;
    private uint _rbo;
    private uint _renderedTexture;
    private uint _framebuffer;

    private Texture2DFramebufferProxy _textureProxy;
    private IProvider<Vector2> _framebufferSizeProvider;
    private Vector2 _currentSize;

    public Framebuffer(IProvider<Vector2> framebufferSizeProvider)
    {
        _framebufferSizeProvider = framebufferSizeProvider;
        _currentSize = framebufferSizeProvider.Get();

        this.InitGL((int)_currentSize.X, (int)_currentSize.Y);
        _textureProxy = new Texture2DFramebufferProxy(this);
    }

    // PUBLIC API
    public ITexture2D GetUnderlyingTexture2D()
    {
        return _textureProxy;
    }

    public void Bind(Action performInBuffer)
    {
        if (_framebufferSizeProvider.Get() != _currentSize)
            Resize(_framebufferSizeProvider.Get());

        // Get old viewport
        int[] oldViewport = new int[4];
        glGetIntegerv(GL_VIEWPORT, ref oldViewport);
        var prev = GetCurrentBoundBuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, _framebuffer);
        glViewport(0, 0, (int)_currentSize.X, (int)_currentSize.Y);
        performInBuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, prev);
        glViewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
    }

    // PRIVATE HELPER METHODS
    private unsafe void InitGL(int width, int height)
    {
        InitQuad();

        _framebuffer = glGenFramebuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, _framebuffer);

        _textureColorBufferMultiSampled = glGenTexture();
        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, _textureColorBufferMultiSampled);
        glTexImage2DMultisample(GL_TEXTURE_2D_MULTISAMPLE, 4, GL_RGBA, width, height, true);
        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, 0);
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D_MULTISAMPLE, _textureColorBufferMultiSampled, 0);

        _rbo = glGenRenderbuffer();
        glBindRenderbuffer(GL_RENDERBUFFER, _rbo);
        glRenderbufferStorageMultisample(GL_RENDERBUFFER, 4, GL_DEPTH24_STENCIL8, width, height);
        glBindRenderbuffer(GL_RENDERBUFFER, 0);
        glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, _rbo);

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("Framebuffer is not complete!");
        }

        glBindFramebuffer(GL_FRAMEBUFFER, 0);

        // configure second post-processing framebuffer
        _intermediateFBO = glGenFramebuffer();
        glBindFramebuffer(GL_FRAMEBUFFER, _intermediateFBO);

        _renderedTexture = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, _renderedTexture);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _renderedTexture, 0);

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("Framebuffer is not complete!");
        }

        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    private unsafe void Resize(Vector2 newSize)
    {
        _currentSize = newSize;

        glBindFramebuffer(GL_FRAMEBUFFER, _framebuffer);

        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, _textureColorBufferMultiSampled);
        glTexImage2DMultisample(GL_TEXTURE_2D_MULTISAMPLE, 4, GL_RGBA, (int)_currentSize.X, (int)_currentSize.Y, true);
        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, 0);
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D_MULTISAMPLE, _textureColorBufferMultiSampled, 0);

        glBindRenderbuffer(GL_RENDERBUFFER, _rbo);
        glRenderbufferStorageMultisample(GL_RENDERBUFFER, 4, GL_DEPTH24_STENCIL8, (int)_currentSize.X, (int)_currentSize.Y);
        glBindRenderbuffer(GL_RENDERBUFFER, 0);
        glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, _rbo);

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("Framebuffer is not complete!");
        }

        glBindFramebuffer(GL_FRAMEBUFFER, 0);

        // configure second post-processing framebuffer
        glBindFramebuffer(GL_FRAMEBUFFER, _intermediateFBO);

        glBindTexture(GL_TEXTURE_2D, _renderedTexture);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, (int)_currentSize.X, (int)_currentSize.Y, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _renderedTexture, 0);

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("Framebuffer is not complete!");
        }

        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    public static void Clear(ColorF color)
    {
        glClearColor(color.R, color.G, color.B, color.A);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    }

    // PRIVATE STATIC STUFF
    private static uint GetCurrentBoundBuffer()
    {
        int[] buffer = new int[1];
        glGetIntegerv(GL_FRAMEBUFFER_BINDING, ref buffer);
        return (uint)buffer[0];
    }

    private static unsafe void InitQuad()
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

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }
}
