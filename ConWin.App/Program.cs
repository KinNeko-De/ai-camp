using System;
using ConWin.Lib;

namespace ConWin.App;

class Program
{
    static void Main(string[] args)
    {
        Console.CursorVisible = false;
        var manager = new WindowManager();

        // Window 1: Top-level, background
        var window1 = new Window(1, 1, 78, 23, "Main Window (Z=0)", BorderStyle.Double)
        {
            BackgroundColor = ConsoleColor.DarkGray,
            ForegroundColor = ConsoleColor.White,
            ZIndex = 0
        };
        manager.AddWindow(window1);

        // Window 2: Top-level, on top of Window 1
        var window2 = new Window(10, 3, 50, 15, "Second Window (Z=1)", BorderStyle.Single)
        {
            BackgroundColor = ConsoleColor.Blue,
            ForegroundColor = ConsoleColor.White,
            ZIndex = 1
        };
        manager.AddWindow(window2);

        // Window 3: Child of Window 2
        var childWindow1 = new Window(5, 2, 30, 8, "Child of Second (Z=0)", BorderStyle.Single, window2)
        {
            BackgroundColor = ConsoleColor.Green,
            ForegroundColor = ConsoleColor.Black,
            ZIndex = 0 // Z-index relative to its siblings, but always on top of parent
        };
        // manager.AddWindow(childWindow1); // Automatically added via parent

        // Window 4: Another top-level window, potentially obscuring others
        var window3 = new Window(20, 8, 35, 10, "Top Window (Z=2)", BorderStyle.Double)
        {
            BackgroundColor = ConsoleColor.Magenta,
            ForegroundColor = ConsoleColor.Yellow,
            ZIndex = 2
        };
        manager.AddWindow(window3);
        
        // Window 5: A small window that might be fully obscured
        var obscuredWindow = new Window(22, 9, 10, 5, "Obscured? (Z=0)", BorderStyle.Single)
        {
            BackgroundColor = ConsoleColor.Red,
            ForegroundColor = ConsoleColor.White,
            ZIndex = 0 // Same Z-index as window1, but added later, draw order depends on list stability or explicit Z
        };
        manager.AddWindow(obscuredWindow);


        manager.DrawAllWindows();

        Console.SetCursorPosition(0, Console.BufferHeight > 0 ? Console.BufferHeight - 1 : 0); // Set cursor to the last valid line
        Console.ResetColor();
        Console.WriteLine("Demo complete. Press any key to exit...");
        Console.ReadKey();
    }
}
