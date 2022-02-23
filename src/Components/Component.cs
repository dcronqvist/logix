using LogiX.SaveSystem;

namespace LogiX.Components;

public abstract class Component : ISelectable
{
    public List<(IO, IOConfig)> IOs { get; private set; }
    public ComponentType Type { get; private set; }
    public Vector2 Position { get; set; }
    public int Rotation { get; set; }

    public string UniqueID { get; set; }
    public Vector2 MiddleOfComponent => this.Position + this.Size / 2f;
    public virtual Vector2 Size
    {
        get
        {
            int IOsOnLeft = this.FilterIOsOnConfig(x => x.Side == ComponentSide.LEFT).Count();
            int IOsOnRight = this.FilterIOsOnConfig(x => x.Side == ComponentSide.RIGHT).Count();
            int IOsOnTop = this.FilterIOsOnConfig(x => x.Side == ComponentSide.TOP).Count();
            int IOsOnBottom = this.FilterIOsOnConfig(x => x.Side == ComponentSide.BOTTOM).Count();

            int maxIOsWidth = Math.Max(IOsOnTop, IOsOnBottom);
            int maxIOsHeight = Math.Max(IOsOnLeft, IOsOnRight);

            float minHeight = Raylib.MeasureTextEx(Util.OpenSans, this.Text, this.TextSize, 0).Y;
            float minWidth = Raylib.MeasureTextEx(Util.OpenSans, this.Text, this.TextSize, 0).X;

            // Guaranteed to only contain non-null strings
            string[] ioIdentifiers = this.IOs.Select(x => x.Item2.Identifier!).Where(x => x != null).ToArray();
            float maxIOIdentifierWidth = Util.GetMaxWidthOfStrings(Util.OpenSans, this.TextSize / 2, 0, ioIdentifiers);

            float width = Math.Max(minWidth, maxIOsWidth * (this.IORadius * 2 + this.IODistBetween) - this.IODistBetween) + (maxIOIdentifierWidth > 0 ? maxIOIdentifierWidth + this.PaddingWidth : 0);
            float height = Math.Max(minHeight, maxIOsHeight * (this.IORadius * 2 + this.IODistBetween) - this.IODistBetween);

            Vector2 size = new Vector2(width + this.PaddingWidth * 2, height + this.PaddingHeight * 2);

            if (this.Rotation == 1 || this.Rotation == 3)
            {
                return new Vector2(size.Y, size.X);
            }

            return size;
        }
    }
    public virtual string Text => this.Type.GetComponentTypeAsString();
    public virtual float IODistToComp => 5f;
    public virtual float IODistBetween => 4f;
    public virtual float IORadius => 7f;
    public virtual float PaddingWidth => 7;
    public virtual float PaddingHeight => 0;
    public virtual int TextSize => 18;

    public Component(Vector2 position, ComponentType type)
    {
        this.Type = type;
        this.Position = position;
        this.UniqueID = Guid.NewGuid().ToString();
        this.IOs = new List<(IO, IOConfig)>();
    }

    public void SetRotation(int rotation)
    {
        this.Rotation = rotation;
    }

    public virtual Rectangle GetRectangle()
    {
        return new Rectangle(this.Position.X, this.Position.Y, this.Size.X, this.Size.Y);
    }

    private List<IO> FilterIOsOnConfig(Func<IOConfig, bool> filter)
    {
        return this.IOs.Where(x => filter(x.Item2)).Select(io => io.Item1).ToList();
    }

    protected void AddIO(int bitWidth, IOConfig config)
    {
        this.IOs.Add((new IO(bitWidth, this), config));
    }

    public int GetIndexOfIO(IO io)
    {
        return this.IOs.FindIndex(x => x.Item1 == io);
    }

    public IO GetIO(int index)
    {
        return this.IOs[index].Item1;
    }

    public abstract void PerformLogic();

    public Vector2 GetIOPosition(IO io)
    {
        return GetIOPosition(io, out Vector2 posAtComp);
    }

    public Vector2 GetIOPosition(IO io, out Vector2 posAtComponent)
    {
        IOConfig config = this.IOs.First(x => x.Item1 == io).Item2; // Get IOConfig for this IO
        List<IO> iosOnSameSide = this.FilterIOsOnConfig(x => x.Side == config.Side); // Get all IOs on the same side as this IO
        int indexOfIO = iosOnSameSide.IndexOf(io); // Get index of this IO

        float xLeft = this.Position.X;
        float xRight = this.Position.X + this.Size.X;
        float yTop = this.Position.Y;
        float yBottom = this.Position.Y + this.Size.Y;

        float height = this.Size.Y;
        float width = this.Size.X;

        // CONSTANTS DESCRIBING WHERE IOS SHOULD BE RELATIVE TO THE COMPONENT
        float distanceToComp = this.IODistToComp;
        float distanceBetweenIOs = this.IODistBetween;
        float radius = this.IORadius;

        float realDistBetweenIOs = radius * 2 + distanceBetweenIOs;
        float totalDistanceByIOs = realDistBetweenIOs * (iosOnSameSide.Count - 1);

        ComponentSide side = Util.GetRotatedComponentSide(config.Side, this.Rotation);

        // Calculate position of IO
        if (side == ComponentSide.LEFT)
        {
            // LEFT
            posAtComponent = new Vector2(xLeft, yTop + height / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs);
            return new Vector2(xLeft - distanceToComp - radius, yTop + height / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs);
        }
        else if (side == ComponentSide.TOP)
        {
            // TOP
            posAtComponent = new Vector2(xLeft + width / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs, yTop);
            return new Vector2(xLeft + width / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs, yTop - distanceToComp - radius);
        }
        else if (side == ComponentSide.RIGHT)
        {
            // RIGHT
            posAtComponent = new Vector2(xRight, yTop + height / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs);
            return new Vector2(xRight + distanceToComp + radius, yTop + height / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs);
        }
        else if (side == ComponentSide.BOTTOM)
        {
            // BOTTOM
            posAtComponent = new Vector2(xLeft + width / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs, yBottom);
            return new Vector2(xLeft + width / 2 - totalDistanceByIOs / 2 + indexOfIO * realDistBetweenIOs, yBottom + distanceToComp + radius);
        }

        posAtComponent = Vector2.Zero;
        return Vector2.Zero;
    }

    public virtual void RenderRectangle()
    {
        Rectangle rect = this.GetRectangle();
        Raylib.DrawRectangleRec(rect.Inflate(1), Color.DARKGRAY);
        Raylib.DrawRectangleRec(rect, Color.WHITE);
    }

    public virtual void RenderText()
    {
        Vector2 textMeasure = Raylib.MeasureTextEx(Util.OpenSans, this.Text, this.TextSize, 0);
        Vector2 pos = this.MiddleOfComponent - textMeasure / 2f;
        Util.RenderTextRotated(pos.RotateAround(this.MiddleOfComponent, -(this.Rotation % 2) * MathF.PI / 2f), Util.OpenSans, this.TextSize, 0, this.Text, (this.Rotation * 90f) % 180f, Color.BLACK);
    }

    public virtual void RenderIOs()
    {
        foreach ((IO io, IOConfig conf) in this.IOs)
        {
            Vector2 ioPos = this.GetIOPosition(io, out Vector2 posAtComp);
            Raylib.DrawLineEx(posAtComp, ioPos, 2f, Color.BLACK);
            Raylib.DrawCircleV(ioPos, this.IORadius + 1f, Color.BLACK);
            Raylib.DrawCircleV(ioPos, this.IORadius, io.GetColor());
        }
    }

    private void RenderIOIdentifiers()
    {
        foreach ((IO io, IOConfig conf) in this.IOs)
        {
            Vector2 ioPos = this.GetIOPosition(io, out Vector2 posAtComp);
            if (conf.Identifier != null)
            {
                Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, conf.Identifier, this.TextSize / 2, 0);
                Vector2 textPos = posAtComp;

                ComponentSide side = Util.GetRotatedComponentSide(conf.Side, this.Rotation);

                if (side == ComponentSide.LEFT)
                {
                    textPos = posAtComp + new Vector2(2, -measure.Y / 2);
                }
                else if (side == ComponentSide.RIGHT)
                {
                    textPos = posAtComp + new Vector2(-2 - measure.X, -measure.Y / 2);
                }
                else if (side == ComponentSide.TOP)
                {
                    textPos = posAtComp + new Vector2(measure.Y / 2, 2);
                }
                else if (side == ComponentSide.BOTTOM)
                {
                    textPos = posAtComp + new Vector2(measure.Y / 2, -measure.X - 2);
                }
                Util.RenderTextRotated(textPos, Util.OpenSans, this.TextSize / 2, 0, conf.Identifier, (this.Rotation * 90f) % 180f, Color.BLACK);
            }
        }
    }

    public virtual void Render()
    {
        this.RenderIOs();
        this.RenderRectangle();
        this.RenderText();
        this.RenderIOIdentifiers();
    }

    public void UpdateLogic()
    {
        this.PerformLogic();

        foreach ((IO io, IOConfig conf) in this.IOs)
        {
            if (io.IsPushing())
            {
                if (io.Wire == null)
                {
                    io.SetValues(io.PushedValues);
                }
            }
            else
            {
                if (io.Wire == null)
                {
                    io.SetValues(Util.NValues(LogicValue.UNKNOWN, io.BitWidth).ToArray());
                }
            }
        }
    }

    public virtual void Interact(Editor.Editor editor) { }

    public void RenderSelected()
    {
        Raylib.DrawRectangleLinesEx(this.GetRectangle().Inflate(2), 2, Color.ORANGE);
    }

    public void Move(Vector2 delta)
    {
        this.Position += delta;
    }
}