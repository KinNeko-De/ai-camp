using System;
using System.Collections.Generic;

namespace ConWin.Lib;

public class Window : WindowBase
{
    public string? Title { get; set; }
    public BorderStyle BorderStyle { get; set; } = BorderStyle.None;
    public Window? Parent { get; set; }
    public List<Window> Children { get; } = new List<Window>();

    public Window(Position position, Size size, string? title = null, BorderStyle borderStyle = BorderStyle.None, Window? parent = null)
        : base(position, size)
    {
        Title = title;
        BorderStyle = borderStyle;
        Parent = parent;
        if (Parent != null)
        {
            Parent.Children.Add(this);
        }
    }

    public Window(int x, int y, int width, int height, string? title = null, BorderStyle borderStyle = BorderStyle.None, Window? parent = null)
        : base(x, y, width, height)
    {
        Title = title;
        BorderStyle = borderStyle;
        Parent = parent;
        if (Parent != null)
        {
            Parent.Children.Add(this);
        }
    }

    public override void Draw()
    {
        // Ensure drawing happens within console boundaries
        if (Position.X < 0 || Position.Y < 0 || 
            Position.X + Size.Width > Console.BufferWidth || 
            Position.Y + Size.Height > Console.BufferHeight)
        {
            // Optionally log or handle out-of-bounds drawing
            return; 
        }

        Console.BackgroundColor = BackgroundColor;
        Console.ForegroundColor = ForegroundColor;

        // Fill background
        for (int i = 0; i < Size.Height; i++)
        {
            Console.SetCursorPosition(Position.X, Position.Y + i);
            Console.Write(new string(' ', Size.Width));
        }

        if (BorderStyle != BorderStyle.None)
        {
            DrawBorder();
        }

        if (!string.IsNullOrEmpty(Title))
        {
            DrawTitle();
        }
    }

    private void DrawBorder()
    {
        char topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical;

        switch (BorderStyle)
        {
            case BorderStyle.Single:
                topLeft = '┌';
                topRight = '┐';
                bottomLeft = '└';
                bottomRight = '┘';
                horizontal = '─';
                vertical = '│';
                break;
            case BorderStyle.Double:
                topLeft = '╔';
                topRight = '╗';
                bottomLeft = '╚';
                bottomRight = '╝';
                horizontal = '═';
                vertical = '║';
                break;
            default:
                return; // No border
        }

        // Draw corners
        Console.SetCursorPosition(Position.X, Position.Y);
        Console.Write(topLeft);
        Console.SetCursorPosition(Position.X + Size.Width - 1, Position.Y);
        Console.Write(topRight);
        Console.SetCursorPosition(Position.X, Position.Y + Size.Height - 1);
        Console.Write(bottomLeft);
        Console.SetCursorPosition(Position.X + Size.Width - 1, Position.Y + Size.Height - 1);
        Console.Write(bottomRight);

        // Draw horizontal lines
        for (int i = 1; i < Size.Width - 1; i++)
        {
            Console.SetCursorPosition(Position.X + i, Position.Y);
            Console.Write(horizontal);
            Console.SetCursorPosition(Position.X + i, Position.Y + Size.Height - 1);
            Console.Write(horizontal);
        }

        // Draw vertical lines
        for (int i = 1; i < Size.Height - 1; i++)
        {
            Console.SetCursorPosition(Position.X, Position.Y + i);
            Console.Write(vertical);
            Console.SetCursorPosition(Position.X + Size.Width - 1, Position.Y + i);
            Console.Write(vertical);
        }
    }

    private void DrawTitle()
    {
        if (!string.IsNullOrEmpty(Title) && Size.Width > 4 && BorderStyle != BorderStyle.None)
        {
            Console.SetCursorPosition(Position.X + 2, Position.Y);
            // Ensure title doesn't exceed window width
            string displayTitle = Title.Length > Size.Width - 4 ? Title.Substring(0, Size.Width - 4) : Title;
            Console.Write(displayTitle);
        }
        else if (!string.IsNullOrEmpty(Title) && BorderStyle == BorderStyle.None && Size.Width > 0 && Size.Height > 0)
        {
            // Draw title at the top-left of the content area if no border
            Console.SetCursorPosition(Position.X, Position.Y);
            string displayTitle = Title.Length > Size.Width ? Title.Substring(0, Size.Width) : Title;
            Console.Write(displayTitle);
        }
    }

    public bool IsVisible(List<Window> allWindows)
    {
        // Check if this window is obscured by any other window that is drawn on top of it
        foreach (var otherWindow in allWindows)
        {
            if (otherWindow == this || otherWindow.ZIndex < this.ZIndex) continue;

            // Check for full occlusion
            if (otherWindow.Position.X <= this.Position.X &&
                otherWindow.Position.Y <= this.Position.Y &&
                otherWindow.Position.X + otherWindow.Size.Width >= this.Position.X + this.Size.Width &&
                otherWindow.Position.Y + otherWindow.Size.Height >= this.Position.Y + this.Size.Height)
            {
                return false; // Fully obscured
            }
        }
        return true;
    }
}
