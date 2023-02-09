using LogiX.Graphics;

namespace LogiX;

public static class Constants
{
    public static ColorF COLOR_SELECTED => ColorF.Orange;

    public static int WIRE_WIDTH => 2;
    public static float WIRE_POINT_RADIUS => 2f;
    public static float PIN_RADIUS => 2f;
    public static int GRIDSIZE => 10;

    public static ColorF COLOR_HIGH => ColorF.SoftGreen;
    public static ColorF COLOR_LOW => ColorF.Darken(COLOR_HIGH, 0.3f);
    public static ColorF COLOR_UNDEFINED => ColorF.BlueGray;
    public static ColorF COLOR_ERROR => ColorF.Red;

    public static string NODE_FONT = "core.font.inconsolata";
    public static Font NODE_FONT_REAL => Utilities.GetFont(NODE_FONT);
    public static string UI_FONT = "core.font.inconsolata";
    public static Font UI_FONT_REAL => Utilities.GetFont(UI_FONT);
}