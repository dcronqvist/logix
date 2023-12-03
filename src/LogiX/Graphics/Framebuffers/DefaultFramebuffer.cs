using System;
using DotGLFW;
using LogiX.Graphics.Textures;
using LogiX.UserInterfaceContext;
using static DotGL.GL;

namespace LogiX.Graphics.Framebuffers;

public partial class Framebuffer
{
    public class DefaultFramebuffer : IFramebuffer
    {
        private class DefaultFramebufferTexture2DProxy : ITexture2D
        {
            private readonly DefaultFramebuffer _defaultFramebuffer;
            private uint _defaultRenderedTexture;

            public DefaultFramebufferTexture2DProxy(DefaultFramebuffer defaultFramebuffer)
            {
                _defaultFramebuffer = defaultFramebuffer;
                _defaultFramebuffer._userInterfaceContext.WindowSizeChanged += (sender, size) =>
                {
                    InitGL();
                };
                InitGL();
            }

            public uint OpenGLID
            {
                get
                {
                    UpdateTexture();
                    return _defaultRenderedTexture;
                }
            }
            public int Width => _defaultFramebuffer._userInterfaceContext.GetWindowWidth();
            public int Height => _defaultFramebuffer._userInterfaceContext.GetWindowHeight();

            public byte[] GetPixelData()
            {
                throw new InvalidOperationException("Cannot get pixel data from a framebuffer texture!");
            }

            private unsafe void InitGL()
            {
                _defaultRenderedTexture = glGenTexture();
                glBindTexture(GL_TEXTURE_2D, _defaultRenderedTexture);

                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, Width, Height, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

                glBindTexture(GL_TEXTURE_2D, 0);
            }

            private void UpdateTexture()
            {
                glBindTexture(GL_TEXTURE_2D, _defaultRenderedTexture);
                // Read all pixels from default framebuffer
                glReadBuffer(GL_LEFT);
                glCopyTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, 0, 0, Width, Height, 0);
                glBindTexture(GL_TEXTURE_2D, 0);
            }
        }

        private IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;
        private DefaultFramebufferTexture2DProxy _textureProxy;

        public DefaultFramebuffer(IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext)
        {
            _userInterfaceContext = userInterfaceContext;
            _userInterfaceContext.WindowSizeChanged += (sender, size) =>
            {
                glViewport(0, 0, size.Item1, size.Item2);
            };
            _textureProxy = new DefaultFramebufferTexture2DProxy(this);
        }

        public void Bind(Action actionToPerformInBoundBuffer)
        {
            // Get old viewport
            int[] oldViewport = new int[4];
            glGetIntegerv(GL_VIEWPORT, ref oldViewport);
            var prev = GetCurrentBoundBuffer();
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
            glViewport(0, 0, oldViewport[2], oldViewport[3]);
            actionToPerformInBoundBuffer();
            glBindFramebuffer(GL_FRAMEBUFFER, prev);
            glViewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
        }

        public ITexture2D GetUnderlyingTexture2D()
        {
            return _textureProxy;
        }
    }
}
