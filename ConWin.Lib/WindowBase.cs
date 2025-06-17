using System;

namespace ConWin.Lib
{
    public abstract class WindowBase
    {
        public Position Position { get; set; }
        public Size Size { get; set; }
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public int ZIndex { get; set; } = 0;

        protected WindowBase(Position position, Size size)
        {
            Position = position;
            Size = size;
        }

        protected WindowBase(int x, int y, int width, int height)
        {
            Position = new Position(x, y);
            Size = new Size(width, height);
        }

        public virtual void Draw()
        {
            // Default implementation: Draw a simple rectangle
            Console.SetCursorPosition(Position.X, Position.Y);
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;

            // Draw top border
            Console.Write("┌");
            for (int i = 0; i < Size.Width - 2; i++)
            {
                Console.Write("─");
            }
            Console.WriteLine("┐");

            // Draw middle part
            for (int i = 0; i < Size.Height - 2; i++)
            {
                Console.SetCursorPosition(Position.X, Position.Y + 1 + i);
                Console.Write("│");
                for (int j = 0; j < Size.Width - 2; j++)
                {
                    Console.Write(" "); // Fill with background color
                }
                Console.WriteLine("│");
            }

            // Draw bottom border
            Console.SetCursorPosition(Position.X, Position.Y + Size.Height - 1);
            Console.Write("└");
            for (int i = 0; i < Size.Width - 2; i++)
            {
                Console.Write("─");
            }
            Console.WriteLine("┘");

            Console.ResetColor();
        }
    }
}
