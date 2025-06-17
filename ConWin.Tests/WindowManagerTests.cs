using Xunit;
using ConWin.Lib;
using System.Collections.Generic;
using System.Linq;
using System; // Required for ConsoleColor

// Helper to access private GetWindowsInDrawingOrder method for testing
// This is a common pattern for testing private methods, though not always recommended for strict unit testing.
// An alternative would be to test its effects through the public DrawAllWindows method,
// but that would require more complex mocking of Console.
public static class WindowManagerExtensions
{
    public static List<Window> TestGetWindowsInDrawingOrder(this WindowManager manager)
    {
        // Using reflection to access the private method
        var methodInfo = typeof(WindowManager).GetMethod("GetWindowsInDrawingOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (methodInfo == null)
        {
            throw new InvalidOperationException("GetWindowsInDrawingOrder method not found. Reflection failed.");
        }
        return (List<Window>)methodInfo.Invoke(manager, null);
    }
}

namespace ConWin.Tests
{
    public class WindowManagerTests
    {
        [Fact]
        public void ChildWindow_IsDrawn_AfterParent()
        {
            // Arrange
            var manager = new WindowManager();
            var parentWindow = new Window(0, 0, 10, 10, "Parent") { ZIndex = 0 };
            var childWindow = new Window(1, 1, 5, 5, "Child", BorderStyle.None, parentWindow) { ZIndex = 0 }; // ZIndex of child relative to siblings

            manager.AddWindow(parentWindow);
            // Child is added to parent's list, manager should discover it or it should be added explicitly
            // Current Window constructor adds child to parent.Children. Manager discovers through parent.

            // Act
            var drawOrder = manager.TestGetWindowsInDrawingOrder();

            // Assert
            Assert.Equal(2, drawOrder.Count);
            Assert.Same(parentWindow, drawOrder[0]);
            Assert.Same(childWindow, drawOrder[1]);
        }

        [Fact]
        public void ChildrenWindows_AreDrawn_AfterParent_AndRespectTheirZIndex()
        {
            // Arrange
            var manager = new WindowManager();
            var parentWindow = new Window(0, 0, 20, 20, "Parent") { ZIndex = 0 };
            var child1 = new Window(1, 1, 5, 5, "Child1", BorderStyle.None, parentWindow) { ZIndex = 1 };
            var child2 = new Window(2, 2, 5, 5, "Child2", BorderStyle.None, parentWindow) { ZIndex = 0 }; // Lower Z-index than child1

            manager.AddWindow(parentWindow);

            // Act
            var drawOrder = manager.TestGetWindowsInDrawingOrder();

            // Assert
            Assert.Equal(3, drawOrder.Count);
            Assert.Same(parentWindow, drawOrder[0]);
            // Children should be ordered by their ZIndex
            Assert.Same(child2, drawOrder[1]); // Child2 has ZIndex 0
            Assert.Same(child1, drawOrder[2]); // Child1 has ZIndex 1
        }

        [Fact]
        public void NestedChildWindows_AreDrawn_InCorrectOrder()
        {
            // Arrange
            var manager = new WindowManager();
            var grandParent = new Window(0, 0, 30, 30, "GrandParent") { ZIndex = 0 };
            var parent = new Window(1, 1, 20, 20, "Parent", BorderStyle.None, grandParent) { ZIndex = 0 };
            var child = new Window(2, 2, 10, 10, "Child", BorderStyle.None, parent) { ZIndex = 0 };

            manager.AddWindow(grandParent);

            // Act
            var drawOrder = manager.TestGetWindowsInDrawingOrder();

            // Assert
            Assert.Equal(3, drawOrder.Count);
            Assert.Same(grandParent, drawOrder[0]);
            Assert.Same(parent, drawOrder[1]);
            Assert.Same(child, drawOrder[2]);
        }

        [Fact]
        public void TopLevelWindows_AreDrawn_ByZIndex_WithChildrenFollowingParents()
        {
            // Arrange
            var manager = new WindowManager();
            var top1_Z0 = new Window(0, 0, 10, 10, "Top1_Z0") { ZIndex = 0 };
            var top2_Z1 = new Window(5, 5, 10, 10, "Top2_Z1") { ZIndex = 1 };
            var child_of_Top1 = new Window(1, 1, 5, 5, "Child_Top1", BorderStyle.None, top1_Z0) { ZIndex = 0 };
            var top3_Z0_addedLater = new Window(3,3, 10,10, "Top3_Z0_Late") { ZIndex = 0};


            manager.AddWindow(top1_Z0); // Z=0
            manager.AddWindow(top2_Z1); // Z=1
            manager.AddWindow(top3_Z0_addedLater); // Z=0, added after top2_Z1

            // Act
            var drawOrder = manager.TestGetWindowsInDrawingOrder();

            // Assert
            // Expected order: top1_Z0, child_of_Top1, top3_Z0_addedLater, top2_Z1
            // (Order of same Z-index top-level windows might depend on AddWindow call order if not otherwise specified,
            //  the current implementation of OrderBy(w => w.ZIndex) is stable for elements with same key)
            Assert.Equal(4, drawOrder.Count);
            Assert.True(drawOrder.IndexOf(top1_Z0) < drawOrder.IndexOf(child_of_Top1));
            Assert.True(drawOrder.IndexOf(child_of_Top1) < drawOrder.IndexOf(top2_Z1));
            
            // Check Z-ordering for top-level windows
            var topLevelDrawn = drawOrder.Where(w => w.Parent == null).ToList();
            Assert.Same(top1_Z0, topLevelDrawn[0]);
            Assert.Same(top3_Z0_addedLater, topLevelDrawn[1]); // Assuming stable sort for same Z-index
            Assert.Same(top2_Z1, topLevelDrawn[2]);
        }
    }

    public class WindowVisibilityTests
    {
        [Fact]
        public void Window_IsVisible_WhenNotObscured()
        {
            // Arrange
            var window = new Window(0, 0, 10, 10, "Visible");
            var allWindows = new List<Window> { window };

            // Act
            var isVisible = window.IsVisible(allWindows);

            // Assert
            Assert.True(isVisible);
        }

        [Fact]
        public void Window_IsNotVisible_WhenFullyObscured_ByHigherZIndexWindow()
        {
            // Arrange
            var obscuredWindow = new Window(0, 0, 10, 10, "Obscured") { ZIndex = 0 };
            var obscuringWindow = new Window(0, 0, 10, 10, "Obscuring") { ZIndex = 1 }; // Same pos/size, higher Z
            var allWindows = new List<Window> { obscuredWindow, obscuringWindow };

            // Act
            var isVisible = obscuredWindow.IsVisible(allWindows);

            // Assert
            Assert.False(isVisible);
        }

        [Fact]
        public void Window_IsVisible_WhenFullyObscured_ByLowerZIndexWindow()
        {
            // Arrange
            var visibleWindow = new Window(0, 0, 10, 10, "Visible") { ZIndex = 1 };
            var obscuringWindow = new Window(0, 0, 10, 10, "ObscuringButLowerZ") { ZIndex = 0 }; // Same pos/size, lower Z
            var allWindows = new List<Window> { visibleWindow, obscuringWindow };

            // Act
            var isVisible = visibleWindow.IsVisible(allWindows);

            // Assert
            Assert.True(isVisible);
        }

        [Fact]
        public void Window_IsVisible_WhenPartiallyObscured_ByHigherZIndexWindow()
        {
            // Arrange
            var partiallyObscuredWindow = new Window(0, 0, 10, 10, "Partial") { ZIndex = 0 };
            var obscuringWindow = new Window(5, 5, 10, 10, "Obscuring") { ZIndex = 1 }; // Partially overlaps, higher Z
            var allWindows = new List<Window> { partiallyObscuredWindow, obscuringWindow };

            // Act
            var isVisible = partiallyObscuredWindow.IsVisible(allWindows);

            // Assert
            Assert.True(isVisible); // Not FULLY obscured
        }
        
        [Fact]
        public void Window_IsVisible_WhenObscuringWindow_IsNextToIt_HigherZIndex()
        {
            // Arrange
            var window = new Window(0, 0, 10, 10, "Window") { ZIndex = 0 };
            var otherWindow = new Window(10, 0, 10, 10, "OtherWindow") { ZIndex = 1 }; // Next to it, no overlap
            var allWindows = new List<Window> { window, otherWindow };

            // Act
            var isVisible = window.IsVisible(allWindows);

            // Assert
            Assert.True(isVisible);
        }

        [Fact]
        public void ChildWindow_IsNotVisible_WhenFullyObscured_ByAnotherTopLevelWindow_WithHigherZIndex()
        {
            // Arrange
            var parent = new Window(0, 0, 20, 20, "Parent") { ZIndex = 0 };
            var child = new Window(1, 1, 5, 5, "Child", BorderStyle.None, parent) { ZIndex = 0 }; // Child's Z is relative to siblings
            var obscuringTopWindow = new Window(1, 1, 5, 5, "Obscurer") { ZIndex = 1 }; // Covers child, higher Z than parent (and thus child effectively)
            
            var allWindows = new List<Window> { parent, child, obscuringTopWindow };
            // Note: The WindowManager would normally manage the list passed to IsVisible.
            // For direct testing of IsVisible, we construct 'allWindows' manually.

            // Act
            var isChildVisible = child.IsVisible(allWindows);

            // Assert
            Assert.False(isChildVisible);
        }

        [Fact]
        public void ChildWindow_IsVisible_WhenParentIsFullyObscured_ButChildIsNot_ByHigherZIndexWindow()
        {
            // Arrange
            var parent = new Window(0, 0, 10, 10, "Parent") { ZIndex = 0 };
            // Child is smaller and offset, potentially not covered by the window obscuring the parent
            var child = new Window(1, 1, 3, 3, "Child", BorderStyle.None, parent) { ZIndex = 1 }; // Higher Z than parent for draw order
            
            // This window fully obscures the parent, but not necessarily the child if child is drawn "on top"
            // and the obscuring window is between parent and child in Z-order.
            // However, IsVisible checks against ALL windows.
            var obscuringParentWindow = new Window(0, 0, 10, 10, "ParentObscurer") { ZIndex = 1 };

            // This window specifically targets obscuring the child
            var obscuringChildWindow = new Window(1, 1, 3, 3, "ChildObscurer") { ZIndex = 2 };


            var allWindowsOnlyParentObscured = new List<Window> { parent, child, obscuringParentWindow };
            var allWindowsChildObscured = new List<Window> { parent, child, obscuringChildWindow };
            var allWindowsBothObscuredByOne = new List<Window> { parent, child, new Window(0,0,10,10, "MegaObscurer") { ZIndex = 3}};


            // Act & Assert
            // Scenario 1: Parent is obscured, child is not (because child is drawn on top of parent's obscurer due to Z-index or draw order)
            // The IsVisible method for the child should still see 'obscuringParentWindow'.
            // If child's ZIndex is higher than obscuringParentWindow, it should be visible.
            // If child's ZIndex is lower or same, it would be obscured if coordinates match.
            // Let's assume child Z-index is 0 (relative to siblings), parent Z-index is 0.
            // obscuringParentWindow Z-index is 1.
            // Child is at (1,1) relative to screen, parent at (0,0).
            // obscuringParentWindow is at (0,0) size (10,10) Z=1. It covers parent. It also covers child's area.
            // So child should be NOT visible if its effective Z is less than or equal to obscuringParentWindow's Z.
            
            // Let's re-evaluate the scenario:
            // Parent (P) at (0,0) size (10,10) Z=0
            // Child (C) at (P.X+1, P.Y+1) = (1,1) size (3,3) Z=0 (relative to siblings, effectively drawn on top of P)
            // Obscurer (O) at (0,0) size (10,10) Z=1.
            // O covers P. O also covers C's coordinates. Since O.ZIndex > P.ZIndex and O.ZIndex > C.ZIndex (assuming C inherits P's base Z for comparison with other top-level windows),
            // both P and C should be not visible.

            var parentZ0 = new Window(0, 0, 10, 10, "P_Z0") { ZIndex = 0 };
            var childOfPZ0 = new Window(1, 1, 3, 3, "C_PZ0", BorderStyle.None, parentZ0) { ZIndex = 0 }; // Effective Z for comparison with O_Z1 is based on parent.
            var obscurerZ1 = new Window(0, 0, 10, 10, "O_Z1") { ZIndex = 1 }; // Covers both
            
            var windows = new List<Window> { parentZ0, childOfPZ0, obscurerZ1 };

            // Assert
            Assert.False(parentZ0.IsVisible(windows), "Parent should be obscured");
            Assert.False(childOfPZ0.IsVisible(windows), "Child should be obscured by a higher Z-index window that covers its area");

            // Scenario 2: A window that only obscures the child, not the parent.
            var parent2 = new Window(0,0,10,10, "P2") { ZIndex = 0 };
            var child2 = new Window(1,1,3,3, "C2", BorderStyle.None, parent2) { ZIndex = 0 };
            var childObscurer = new Window(1,1,3,3, "CObscurer") { ZIndex = 1}; // Covers only child
            var windows2 = new List<Window> { parent2, child2, childObscurer };

            Assert.True(parent2.IsVisible(windows2), "Parent2 should be visible");
            Assert.False(child2.IsVisible(windows2), "Child2 should be obscured");
        }
    }
}
