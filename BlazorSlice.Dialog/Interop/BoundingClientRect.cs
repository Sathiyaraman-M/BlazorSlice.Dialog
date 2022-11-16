namespace BlazorSlice.Dialog.Interop;

public class BoundingClientRect
{
    public double Top { get; set; }
    public double Left { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public double WindowHeight { get; set; }

    public double WindowWidth { get; set; }

    public double ScrollX { get; set; }

    public double ScrollY { get; set; }

    // computed properties, read only
    public double X => Left;
    public double Y => Top;
    public double Bottom => Top + Height;
    public double Right => Left + Width;

    public double AbsoluteLeft => Left + ScrollX;

    public double AbsoluteTop => Top + ScrollY;

    public double AbsoluteRight => Right + ScrollX;

    public double AbsoluteBottom => Bottom + ScrollY;

    //check if the rect is outside of the viewport
    public bool IsOutsideBottom => Bottom > WindowHeight;

    public bool IsOutsideLeft => Left < 0;

    public bool IsOutsideTop => Top < 0;

    public bool IsOutsideRight => Right > WindowWidth;

    public BoundingClientRect Clone()
    {
        return new BoundingClientRect
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            WindowHeight = WindowHeight,
            WindowWidth = WindowWidth,
            ScrollX = ScrollX,
            ScrollY = ScrollY
        };
    }
}