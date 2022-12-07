using System.Drawing;
using System.Numerics;
using FreeTypeSharp;
using LogiX.Content;
using Symphony;
using static LogiX.OpenGL.GL;
using static FreeTypeSharp.Native.FT;
using FreeTypeSharp.Native;

namespace LogiX.Graphics;

public class FontData
{
    public enum FontFilter : int
    {
        NearestNeighbour = 0x2600,
        Linear = 0x2601
    }

    public byte[] Data { get; set; }
    public uint Size { get; set; }
    public FontFilter MagFilter { get; set; }
    public FontFilter MinFilter { get; set; }

    public FontData(byte[] data, uint size, FontFilter magFilter, FontFilter minFilter)
    {
        Data = data;
        Size = size;
        MagFilter = magFilter;
        MinFilter = minFilter;
    }
}

public struct FontCharacter
{
    public string Chara { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Bearing { get; set; }
    public int Advance { get; set; }
    public RectangleF Rectangle { get; set; }
}

public class Font : GLContentItem<FontData>
{
    public Dictionary<char, FontCharacter> Characters { get; set; }
    public FreeTypeLibrary Lib { get; set; }

    public float MaxY { get; private set; }
    public uint TextureID { get; private set; }
    public int AtlasWidth { get; private set; }
    public int AtlasHeight { get; private set; }

    public bool ApplyIconRange { get; private set; }

    public Font(string identifier, IContentSource source, FontData content, bool applyIconRange) : base(identifier, source, content)
    {
        this.Characters = new Dictionary<char, FontCharacter>();
        this.ApplyIconRange = applyIconRange;
        this.InitNoGL();
    }

    public unsafe void InitNoGL()
    {
        this.Characters.Clear();

        // Have to init the freetype2 lib.
        this.Lib = new FreeTypeLibrary();

        // Create empty pointer for the font.
        IntPtr aFace;
        var data = Content.Data;
        var size = Content.Size;

        // Init the font using FreeType2
        //FT_New_Face(Lib.Native, TTFFile, 0, out aFace);
        fixed (byte* ptr = &data[0])
        {
            FT_New_Memory_Face(Lib.Native, new IntPtr(ptr), data.Length, 0, out aFace);
        }
        // Set font size
        FT_Set_Pixel_Sizes(aFace, 0, size);
        // Then create facade for getting all the data.
        FreeTypeFaceFacade ftff = new FreeTypeFaceFacade(Lib, aFace);

        uint width = 0;
        uint height = 0;

        // Loop 128 times, first 128 characters for this font
        for (uint i = 0; i < 256; i++)
        {
            // Check if the character exists for this font.
            FT_Error error = FT_Load_Char(aFace, i, FT_LOAD_RENDER);
            if (error != FT_Error.FT_Err_Ok)
            {
                // TODO: Fix this shit man, should use integrated console when that is done.
                //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                continue;
            }

            width += ftff.GlyphBitmap.width;
            height = Math.Max(height, ftff.GlyphBitmap.rows);
        }

        int atlasWidth = (int)width;
        this.AtlasWidth = atlasWidth;
        int atlasHeight = (int)height;
        this.AtlasHeight = atlasHeight;

        uint x = 0;
        this.MaxY = height;

        for (uint i = 0; i < 256; i++)
        {
            // Check if the character exists for this font.
            FT_Error error = FT_Load_Char(aFace, i, FT_LOAD_RENDER);
            if (error != FT_Error.FT_Err_Ok)
            {
                // TODO: Fix this shit man, should use integrated console when that is done.
                //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                continue;
            }

            FontCharacter character = new FontCharacter()
            {
                Size = new Vector2(ftff.GlyphBitmap.width, ftff.GlyphBitmap.rows),
                Bearing = new Vector2(ftff.GlyphBitmapLeft, ftff.GlyphBitmapTop),
                Advance = ftff.GlyphMetricHorizontalAdvance,
                Chara = ((char)i).ToString(),
                Rectangle = new Rectangle((int)x, 0, (int)ftff.GlyphBitmap.width, (int)ftff.GlyphBitmap.rows)
            };

            // Add it to the character dictionary
            Characters.Add((char)i, character);

            x += ftff.GlyphBitmap.width;
        }

        FT_Done_Face(aFace);
        FT_Done_FreeType(Lib.Native);
    }

    public string GetFontBaseName()
    {
        var split = this.Identifier.Split('.');
        var contentFont = split.Take(2);
        var rest = split[2];

        return string.Join(".", contentFont) + "." + rest.Split("-")[0];
    }

    public bool IsBold => this.Identifier.Contains("bold");
    public bool IsItalic => this.Identifier.Contains("italic");

    public int GetSize()
    {
        return int.Parse(this.Identifier.Split("-").Last());
    }

    public override void DestroyGL()
    {
        glDeleteTexture(this.TextureID);
    }

    public unsafe override void InitGL(FontData newContent)
    {
        this.Characters.Clear();

        // Have to init the freetype2 lib.
        this.Lib = new FreeTypeLibrary();

        // Create empty pointer for the font.
        IntPtr aFace;
        var data = newContent.Data;
        var size = newContent.Size;

        // Init the font using FreeType2
        //FT_New_Face(Lib.Native, TTFFile, 0, out aFace);
        fixed (byte* ptr = &data[0])
        {
            FT_New_Memory_Face(Lib.Native, new IntPtr(ptr), data.Length, 0, out aFace);
        }
        // Set font size
        FT_Set_Pixel_Sizes(aFace, 0, size);
        // Then create facade for getting all the data.
        FreeTypeFaceFacade ftff = new FreeTypeFaceFacade(Lib, aFace);

        uint width = 0;
        uint height = 0;

        int flags = FT_LOAD_RENDER;

        // Loop 128 times, first 128 characters for this font
        for (uint i = 0; i < 256; i++)
        {
            // Check if the character exists for this font.
            FT_Error error = FT_Load_Char(aFace, i, flags);
            if (error != FT_Error.FT_Err_Ok)
            {
                // TODO: Fix this shit man, should use integrated console when that is done.
                //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                continue;
            }

            width += ftff.GlyphBitmap.width;
            height = Math.Max(height, ftff.GlyphBitmap.rows);
        }

        if (this.ApplyIconRange)
        {
            for (uint i = 0xe000; i < 0xf8ff; i++)
            {
                // Check if the character exists for this font.
                FT_Error error = FT_Load_Char(aFace, i, flags);
                if (error != FT_Error.FT_Err_Ok)
                {
                    // TODO: Fix this shit man, should use integrated console when that is done.
                    //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                    continue;
                }

                width += ftff.GlyphBitmap.width;
                height = Math.Max(height, ftff.GlyphBitmap.rows);
            }
        }

        int atlasWidth = (int)width;
        this.AtlasWidth = atlasWidth;
        int atlasHeight = (int)height;
        this.AtlasHeight = atlasHeight;

        // Create the atlas texture.
        this.TextureID = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, this.TextureID);
        glPixelStorei(GL_UNPACK_ALIGNMENT, 1);

        glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, atlasWidth, atlasHeight, 0, GL_RED, GL_UNSIGNED_BYTE, IntPtr.Zero);

        uint x = 0;
        this.MaxY = height;

        for (uint i = 0; i < 256; i++)
        {
            // Check if the character exists for this font.
            FT_Error error = FT_Load_Char(aFace, i, flags);
            if (error != FT_Error.FT_Err_Ok)
            {
                // TODO: Fix this shit man, should use integrated console when that is done.
                //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                continue;
            }

            glTexSubImage2D(GL_TEXTURE_2D, 0, (int)x, 0, (int)ftff.GlyphBitmap.width, (int)ftff.GlyphBitmap.rows, GL_RED, GL_UNSIGNED_BYTE, ftff.GlyphBitmap.buffer);

            FontCharacter character = new FontCharacter()
            {
                Size = new Vector2(ftff.GlyphBitmap.width, ftff.GlyphBitmap.rows),
                Bearing = new Vector2(ftff.GlyphBitmapLeft, ftff.GlyphBitmapTop),
                Advance = ftff.GlyphMetricHorizontalAdvance,
                Chara = ((char)i).ToString(),
                Rectangle = new Rectangle((int)x, 0, (int)ftff.GlyphBitmap.width, (int)ftff.GlyphBitmap.rows)
            };

            // Add it to the character dictionary
            Characters.Add((char)i, character);

            x += ftff.GlyphBitmap.width;
        }

        if (this.ApplyIconRange)
        {
            for (uint i = 0xe000; i < 0xf8ff; i++)
            {
                // Check if the character exists for this font.
                FT_Error error = FT_Load_Char(aFace, i, flags);
                if (error != FT_Error.FT_Err_Ok)
                {
                    // TODO: Fix this shit man, should use integrated console when that is done.
                    //Debug.WriteLine("FREETYPE ERROR: FAILED TO LOAD GLYPH FOR INDEX: " + i);
                    continue;
                }

                glTexSubImage2D(GL_TEXTURE_2D, 0, (int)x, 0, (int)ftff.GlyphBitmap.width, (int)ftff.GlyphBitmap.rows, GL_RED, GL_UNSIGNED_BYTE, ftff.GlyphBitmap.buffer);

                FontCharacter character = new FontCharacter()
                {
                    Size = new Vector2(ftff.GlyphBitmap.width, ftff.GlyphBitmap.rows),
                    Bearing = new Vector2(ftff.GlyphBitmapLeft, ftff.GlyphBitmapTop),
                    Advance = ftff.GlyphMetricHorizontalAdvance,
                    Chara = ((char)i).ToString(),
                    Rectangle = new Rectangle((int)x, 0, (int)ftff.GlyphBitmap.width, (int)ftff.GlyphBitmap.rows)
                };

                // Add it to the character dictionary
                Characters.Add((char)i, character);

                x += ftff.GlyphBitmap.width;
            }
        }

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)newContent.MinFilter);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)newContent.MagFilter);

        // Unbind the texture.
        glBindTexture(GL_TEXTURE_2D, 0);

        FT_Done_Face(aFace);
        FT_Done_FreeType(Lib.Native);
    }

    public override bool IsGLInitialized()
    {
        return this.TextureID != 0;
    }

    public Vector2 MeasureString(string text, float scale)
    {
        float sizeX = 0;

        foreach (char c in text)
        {
            FontCharacter ch = Characters[c];

            sizeX += ch.Advance * scale;
        }

        return new Vector2(sizeX, this.MaxY * scale);
    }
}