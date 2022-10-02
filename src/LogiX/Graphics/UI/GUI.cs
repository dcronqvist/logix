using System.Drawing;
using System.Numerics;
using LogiX.GLFW;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public static class GUI
{
    private static Camera2D _camera;
    private static int _nextID = 0;

    public static int _hotID = -1;
    public static int _activeID = -1;
    public static int _kbdFocusID = -1;

    private static string _fontPath;
    private static Font _font;
    private static string _textShaderPath;
    private static ShaderProgram _textShader;
    private static Queue<char> _charQueue;

    public static void Init(string font, string textShader)
    {
        _fontPath = font;
        _textShaderPath = textShader;
        _charQueue = new Queue<char>();

        Input.OnChar += (sender, c) =>
        {
            if (_caretPosition != -1)
            {
                _charQueue.Enqueue(c);
            }
        };

        Input.OnBackspace += (sender, e) =>
        {
            if (_caretPosition != -1)
            {
                _charQueue.Enqueue('\b');
            }
        };

        Input.OnEnterPressed += (sender, e) =>
        {
            if (_caretPosition != -1)
            {
                _charQueue.Enqueue('\n');
            }
        };

        Input.OnCharMods += (sender, e) =>
        {
            if (e.Item1 == 'v' && e.Item2.HasFlag(ModifierKeys.Control))
            {
                string s = Glfw.GetClipboardString(DisplayManager.WindowHandle);
                foreach (char c in s)
                {
                    _charQueue.Enqueue(c);
                }
            }
        };

        Input.OnKeyPressOrRepeat += (sender, e) =>
        {
            if (e.Item1 == Keys.Left)
            {
                _caretPosition--;
            }
            if (e.Item1 == Keys.Right)
            {
                _caretPosition++;
            }
        };
    }

    private static int GetNextID()
    {
        return _nextID++;
    }

    private static bool TryBecomeHot(int id)
    {
        if (_hotID == -1)
        {
            _hotID = id;
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool TryBecomeActive(int id)
    {
        if (_activeID == -1)
        {
            _showingDropdownID = -1;
            _activeID = id;
            return true;
        }
        else
        {
            return false;
        }
    }

    private static void ResetKeyboardFocus()
    {
        _kbdFocusID = -1;
    }

    private static void GetKeyboardFocus(int id)
    {
        _kbdFocusID = id;
    }

    private static bool IsActive(int id)
    {
        return _activeID == id;
    }

    private static bool IsHot(int id)
    {
        return _hotID == id;
    }

    private static bool HasKeyboardFocus(int id)
    {
        return _kbdFocusID == id;
    }

    // BEGIN AND END STUFF
    public static void Begin(Camera2D camera)
    {
        _nextID = 0;
        _camera = camera;

        if (_font is null)
        {
            _font = LogiX.ContentManager.GetContentItem<Font>(_fontPath);
        }

        if (_textShader is null)
        {
            _textShader = LogiX.ContentManager.GetContentItem<ShaderProgram>(_textShaderPath);
        }
    }

    public static void End()
    {
        _caretBlinkCounter += GameTime.DeltaTime;
        if (_caretBlinkCounter > _caretBlinkTime * 2f)
        {
            _caretBlinkCounter = 0f;
        }

        if ((Input.IsMouseButtonDown(MouseButton.Left) && _hotID == -1) || Input.IsKeyPressed(Keys.Escape))
        {
            _kbdFocusID = -1;
            _caretPosition = -1;
            _showingDropdownID = -1;
        }

        if (!Input.IsMouseButtonDown(MouseButton.Left) && !Input.IsKeyDown(Keys.Enter))
        {
            _hotID = -1;
            _activeID = -1;
        }

        if (Input.IsKeyPressed(Keys.Tab))
        {
            if (Input.IsKeyDown(Keys.LeftShift))
            {
                _kbdFocusID--;

                if (_kbdFocusID < 0)
                {
                    _kbdFocusID = _nextID - 1;
                }
            }
            else
            {
                _kbdFocusID++;

                if (_kbdFocusID >= _nextID)
                {
                    _kbdFocusID = 0;
                }
            }

            if (_kbdFocusID == -1)
            {
                _kbdFocusID = 0;
            }

            _caretPosition = -1;
            _showingDropdownID = -1;
        }

        if (_kbdFocusID == -1)
        {
            return;
        }
    }

    private static void RenderText(string text, float scale, Vector2 position, ColorF color, bool centerText = true)
    {
        if (_font is null || _textShader is null)
        {
            return;
        }

        var middleOfString = centerText ? _font.GetMiddleOfString(text, scale) : Vector2.Zero;

        TextRenderer.RenderText(_textShader, _font, text, position - middleOfString, scale, color, _camera);
    }

    public static bool Button(string text, Vector2 position, Vector2 size)
    {
        var defaultColor = ColorF.Gray;
        var hoverColor = ColorF.Darken(defaultColor, 1.2f);
        var activeColor = ColorF.LightGray;

        var id = GetNextID();
        RectangleF rect = new(position.X, position.Y, size.X, size.Y);

        var mousePosition = Input.GetMousePositionInWindow();

        if (rect.Contains(mousePosition))
        {
            // Mouse is over button
            if (TryBecomeHot(id))
            {
                // Button is hot and can become active
                if (Input.IsMouseButtonDown(MouseButton.Left) && TryBecomeActive(id))
                {
                    // Button is active
                    ResetKeyboardFocus();
                }
            }
        }

        if (HasKeyboardFocus(id))
        {
            if (Input.IsKeyDown(Keys.Enter))
            {
                TryBecomeActive(id);
            }
        }

        var col = defaultColor;
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");

        if (HasKeyboardFocus(id))
        {
            // Button has keyboard focus
            PrimitiveRenderer.RenderRectangle(shader, rect.Inflate(new Vector2(3, 3)), Vector2.Zero, 0f, ColorF.Orange, _camera);
            PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, col, _camera);
        }

        if (IsHot(id) && !IsActive(id))
        {
            col = hoverColor;
            // Button is just hovered
            PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, col, _camera);
        }
        else if (IsActive(id))
        {
            // Button is being clicked
            col = activeColor;
            PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, col, _camera);
        }
        else
        {
            // Button is not hovered or clicked
            PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, col, _camera);
        }

        RenderText(text, 1f, rect.GetMiddleOfRectangle(), ColorF.White);

        return (IsActive(id) && Input.IsMouseButtonPressed(MouseButton.Left)) || (IsActive(id) && Input.IsKeyPressed(Keys.Enter));
    }

    public static bool Slider(string text, Vector2 position, Vector2 size, ref float value)
    {
        var defaultColor = ColorF.Gray;
        var hoverColor = ColorF.PearlGray;
        var activeColor = ColorF.LightGray;

        int id = GetNextID();
        float oldValue = value;

        float sliderWidth = size.Y;
        float sliderX = position.X + value * (size.X - sliderWidth);

        RectangleF sliderRect = new(position.X, position.Y, size.X, size.Y);
        sliderRect = sliderRect.Inflate(new Vector2(-2, -2));
        Vector2 mousePos = Input.GetMousePositionInWindow();

        if (sliderRect.Contains(mousePos))
        {
            if (TryBecomeHot(id))
            {
                if (Input.IsMouseButtonDown(MouseButton.Left) && TryBecomeActive(id))
                {
                    // Slider is active
                    ResetKeyboardFocus();
                }
            }
        }

        var sliderColor = defaultColor;
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        if (shader is null)
        {
            return false;
        }

        if (HasKeyboardFocus(id))
        {
            PrimitiveRenderer.RenderRectangle(shader, sliderRect.Inflate(new Vector2(5, 5)), Vector2.Zero, 0f, ColorF.Orange, _camera);

            if (Input.IsKeyPressed(Keys.Left))
            {
                value -= 0.1f;
            }

            if (Input.IsKeyPressed(Keys.Right))
            {
                value += 0.1f;
            }

            value = Utilities.Clamp(value, 0f, 1f);
        }

        if (IsActive(id))
        {
            sliderX = Utilities.Clamp(mousePos.X - sliderWidth / 2, position.X, position.X + size.X - sliderWidth);
            value = (sliderX - position.X) / (size.X - sliderWidth);
            sliderColor = activeColor;
        }

        PrimitiveRenderer.RenderRectangle(shader, sliderRect.Inflate(new Vector2(2, 2)), Vector2.Zero, 0f, ColorF.Darken(defaultColor, 0.5f), _camera);
        PrimitiveRenderer.RenderRectangle(shader, new RectangleF(sliderX, position.Y, sliderWidth, size.Y).Inflate(new Vector2(-4, -4)), Vector2.Zero, 0f, sliderColor, _camera);

        RenderText(text, 1f, sliderRect.GetMiddleOfRectangle(), ColorF.White);

        return oldValue != value;
    }

    private static float _caretBlinkTime = 0.5f;
    private static float _caretBlinkCounter;
    private static bool _caretVisible => _caretBlinkCounter < _caretBlinkTime;
    public static int _caretPosition = -1;

    [Flags]
    public enum TextFieldFlags
    {
        None = 1 << 0,
        Password = 1 << 1,
    }

    public static bool TextField(string placeholder, Vector2 position, Vector2 size, ref string value, TextFieldFlags flags = TextFieldFlags.None)
    {
        var oldValue = value;
        var id = GetNextID();

        var mousePos = Input.GetMousePositionInWindow();
        var rect = new RectangleF(position.X, position.Y, size.X, size.Y);

        if (rect.Contains(mousePos))
        {
            if (TryBecomeHot(id))
            {
                if (Input.IsMouseButtonDown(MouseButton.Left) && TryBecomeActive(id))
                {
                    // TextField is active
                    _kbdFocusID = id;
                    _caretPosition = value.Length;
                    _caretBlinkCounter = 0f;
                }
            }
        }

        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        if (shader is null)
        {
            return false;
        }

        bool pressedEnter = false;

        if (HasKeyboardFocus(id))
        {
            if (_caretPosition == -1)
            {
                _caretPosition = value.Length;
                _caretBlinkCounter = 0f;
            }

            PrimitiveRenderer.RenderRectangle(shader, rect.Inflate(new Vector2(3, 3)), Vector2.Zero, 0f, ColorF.Orange, _camera);

            while (_charQueue.Count > 0)
            {
                char c = _charQueue.Dequeue();
                if (c == '\b')
                {
                    if (value.Length > 0)
                    {
                        value = value.Remove(_caretPosition - 1, 1);
                        _caretPosition--;
                    }
                }
                else if (c == '\n')
                {
                    _kbdFocusID = -1;
                    pressedEnter = true;
                }
                else
                {
                    value = value.Insert(_caretPosition, c.ToString());
                    _caretPosition++;
                }
            }

            _caretPosition = Math.Clamp(_caretPosition, 0, value.Length);
        }

        if (_font is null)
        {
            return false;
        }
        string text = value.Length > 0 ? value : placeholder;
        if (flags.HasFlag(TextFieldFlags.Password) && value.Length > 0)
        {
            text = new string('*', text.Length);
        }

        PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, ColorF.Gray, _camera);
        var measure = _font.MeasureString(text, 1f);
        var textPos = position + new Vector2(5, size.Y / 2 - measure.Y / 2);

        var textColor = value.Length > 0 ? ColorF.White : ColorF.LightGray;

        RenderText(text, 1f, textPos, textColor, false);

        if (HasKeyboardFocus(id))
        {
            var caretMeasure = _font.MeasureString(text.Substring(0, _caretPosition), 1f);
            RenderText("_", 1f, textPos + new Vector2(caretMeasure.X, 0), _caretVisible ? ColorF.White : ColorF.Transparent, false);
        }

        return pressedEnter;
    }

    public static bool Checkbox(string text, Vector2 position, Vector2 size, ref bool value)
    {
        var defaultColor = ColorF.Gray;
        var hoverColor = ColorF.Darken(ColorF.Gray, 1.2f);
        var activeColor = ColorF.LightGray;

        var id = GetNextID();
        var oldValue = value;

        var mousePos = Input.GetMousePositionInWindow();
        var rect = new RectangleF(position.X, position.Y, size.X, size.Y);

        if (rect.Contains(mousePos))
        {
            if (TryBecomeHot(id))
            {
                if (Input.IsMouseButtonDown(MouseButton.Left) && TryBecomeActive(id))
                {
                    // Checkbox is active
                    ResetKeyboardFocus();
                }
            }
        }

        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        if (shader is null)
        {
            return false;
        }

        if (HasKeyboardFocus(id))
        {
            if (Input.IsKeyPressed(Keys.Enter))
            {
                value = !value;
            }

            PrimitiveRenderer.RenderRectangle(shader, rect.Inflate(new Vector2(3, 3)), Vector2.Zero, 0f, ColorF.Orange, _camera);

        }

        var color = defaultColor;

        if (IsHot(id))
        {
            color = hoverColor;
        }

        if (IsActive(id))
        {
            color = activeColor;
        }

        if (IsActive(id) && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            value = !value;
        }

        PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, color, _camera);

        if (value)
        {
            PrimitiveRenderer.RenderRectangle(shader, rect.Inflate(new Vector2(-4, -4)), Vector2.Zero, 0f, ColorF.White, _camera);
        }

        if (_font is null)
        {
            return false;
        }

        var measure = _font.MeasureString(text, 1f);
        RenderText(text, 1f, position + new Vector2(size.X + 10, size.Y / 2 - measure.Y / 2f), ColorF.White, false);

        return (IsActive(id) && Input.IsMouseButtonPressed(MouseButton.Left)) || (HasKeyboardFocus(id) && Input.IsKeyPressed(Keys.Enter));
    }

    public static int _showingDropdownID = -1;
    public static int _hotDropdownIndex = -1;
    private static int _temporarySelectedIndex = -1;

    public static bool Dropdown(Vector2 position, Vector2 size, string[] options, ref int selected)
    {
        var defaultColor = ColorF.Gray;
        var hoverColor = ColorF.Darken(ColorF.Gray, 1.2f);
        var activeColor = ColorF.LightGray;

        var id = GetNextID();
        var oldValue = selected;

        var mousePos = Input.GetMousePositionInWindow();
        var rect = new RectangleF(position.X, position.Y, size.X, size.Y);

        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        if (shader is null)
        {
            return false;
        }

        if (rect.Contains(mousePos))
        {
            if (TryBecomeHot(id))
            {
                if (Input.IsMouseButtonDown(MouseButton.Left) && TryBecomeActive(id))
                {
                    // Dropdown is active
                    _kbdFocusID = id;
                    _temporarySelectedIndex = selected;
                    if (_showingDropdownID == -1)
                    {
                        _showingDropdownID = id;
                    }
                    else
                    {
                        _showingDropdownID = -1;
                    }
                }
            }
        }

        if (HasKeyboardFocus(id))
        {
            if (Input.IsKeyPressed(Keys.Enter))
            {
                if (_showingDropdownID == -1)
                {
                    _showingDropdownID = id;
                    _temporarySelectedIndex = selected;
                }
                else
                {
                    _showingDropdownID = -1;
                    selected = _temporarySelectedIndex;
                }
            }

            PrimitiveRenderer.RenderRectangle(shader, rect.Inflate(new Vector2(3, 3)), Vector2.Zero, 0f, ColorF.Orange, _camera);

            if (Input.IsKeyPressed(Keys.Up))
            {
                _temporarySelectedIndex--;
                if (_temporarySelectedIndex < 0)
                {
                    _temporarySelectedIndex = options.Length - 1;
                }
            }
            else if (Input.IsKeyPressed(Keys.Down))
            {
                _temporarySelectedIndex++;
                if (_temporarySelectedIndex >= options.Length)
                {
                    _temporarySelectedIndex = 0;
                }
            }
        }

        if (_font is null)
        {
            return false;
        }

        PrimitiveRenderer.RenderRectangle(shader, rect, Vector2.Zero, 0f, defaultColor, _camera);
        var measure = _font.MeasureString(options[selected], 1f);
        RenderText(options[selected], 1f, position + new Vector2(5, size.Y / 2 - measure.Y / 2), ColorF.White, false);

        if (_showingDropdownID == id)
        {
            for (int i = 0; i < options.Length; i++)
            {
                var optionRect = new RectangleF(position.X, position.Y + size.Y * (i + 1), size.X, size.Y);
                var color = defaultColor;
                if (optionRect.Contains(mousePos) && !IsActive(id))
                {
                    _hotDropdownIndex = i;
                    if (Input.IsMouseButtonDown(MouseButton.Left) || Input.IsKeyPressed(Keys.Enter))
                    {
                        selected = i;
                        _showingDropdownID = -1;
                    }
                    color = hoverColor;
                }

                if (_temporarySelectedIndex == i)
                {
                    color = activeColor;
                }

                PrimitiveRenderer.RenderRectangle(shader, optionRect, Vector2.Zero, 0f, color, _camera);
                measure = _font.MeasureString(options[i], 1f);
                RenderText(options[i], 1f, position + new Vector2(5, size.Y / 2 - measure.Y / 2) + new Vector2(0, size.Y * (i + 1)), ColorF.White, false);
            }
        }

        return oldValue != selected;
    }
}