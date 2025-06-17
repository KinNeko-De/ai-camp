using System;
using System.Collections.Generic;
using System.Linq;

namespace ConWin.Lib;

public class WindowManager
{
    private readonly List<Window> _windows = new List<Window>();

    public void AddWindow(Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }
        _windows.Add(window);
    }

    public void RemoveWindow(Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }
        _windows.Remove(window);
        // Also remove from parent's children list if it's a child window
        window.Parent?.Children.Remove(window);
    }

    public void DrawAllWindows()
    {
        Console.Clear(); // Clear screen before drawing all windows

        var windowsToDraw = GetWindowsInDrawingOrder();

        foreach (var window in windowsToDraw)
        {
            if (window.IsVisible(_windows))
            {
                window.Draw();
            }
        }
        Console.ResetColor(); 
    }

    private List<Window> GetWindowsInDrawingOrder()
    {
        var sortedWindows = new List<Window>();
        var topLevelWindows = _windows.Where(w => w.Parent == null).OrderBy(w => w.ZIndex).ToList();

        foreach (var topLevelWindow in topLevelWindows)
        {
            AddWindowAndItsChildrenToSortedList(topLevelWindow, sortedWindows);
        }

        return sortedWindows;
    }

    private void AddWindowAndItsChildrenToSortedList(Window window, List<Window> sortedList)
    {
        sortedList.Add(window);
        // Children are drawn on top of their parent, respecting their own Z-index relative to siblings if needed in future.
        // For now, simple recursive add is fine as children are always on top of parent.
        var childrenSorted = window.Children.OrderBy(c => c.ZIndex).ToList(); 
        foreach (var child in childrenSorted)
        {
            AddWindowAndItsChildrenToSortedList(child, sortedList);
        }
    }
}
