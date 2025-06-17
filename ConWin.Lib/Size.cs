namespace ConWin.Lib;

public record Size(int Width, int Height)
{
    /// <summary>
    /// Gets a size with zero width and height.
    /// </summary>
    public static Size Zero => new(0, 0);
}