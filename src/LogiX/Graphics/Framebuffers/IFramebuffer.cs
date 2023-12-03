using System;
using LogiX.Graphics.Textures;

namespace LogiX.Graphics.Framebuffers;

public interface IFramebuffer
{
    void Bind(Action actionToPerformInBoundBuffer);
    ITexture2D GetUnderlyingTexture2D();
}
