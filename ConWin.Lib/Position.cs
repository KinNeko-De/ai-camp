namespace ConWin.Lib;

/// <summary>
/// Represents a position in a 2D space with X and Y coordinates.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
public record Position(int X, int Y)
{
    public static Position Zero => new(0, 0);
}
