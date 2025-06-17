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
            // Pass a new set for each top-level window to track visited nodes in the current branch
            AddWindowAndItsChildrenToSortedList(topLevelWindow, sortedWindows, new HashSet<Window>());
        }

        return sortedWindows;
    }

    private void AddWindowAndItsChildrenToSortedList(Window window, List<Window> sortedList, HashSet<Window> visitedInPath)
    {
        // Check for circular dependency
        if (visitedInPath.Contains(window))
        {
            // Cycle detected, do not process this window again in this path
            // Optionally, log this event or throw a specific exception if strict error handling is desired.
            return; 
        }

        visitedInPath.Add(window); // Mark as visited in current recursion path

        sortedList.Add(window);
        
        var childrenSorted = window.Children.OrderBy(c => c.ZIndex).ToList(); 
        foreach (var child in childrenSorted)
        {
            AddWindowAndItsChildrenToSortedList(child, sortedList, visitedInPath);
        }

        visitedInPath.Remove(window); // Unmark as visited when recursion for this node (and its children) is done
    }
}
