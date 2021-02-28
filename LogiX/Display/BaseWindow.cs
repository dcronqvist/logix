using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Display
{
    abstract class BaseWindow
    {
        public Vector2 WindowSize { get; set; }
        public string Title { get; set; }

        public BaseWindow() : this(new Vector2(800, 600), "Base Window")
        {

        }

        public BaseWindow(Vector2 windowSize, string title)
        {
            this.WindowSize = windowSize;
            this.Title = title;
        }

        /// <summary>
        /// Sets the size of the window to the specific Vector.
        /// </summary>
        /// <param name="size">Vector2 of the window size.</param>
        /// <param name="immediately">If true, then the window will be updated immediately. Should be set to false if ever run before LoadContent().</param>
        public void SetWindowSize(Vector2 size, bool immediately)
        {
            this.WindowSize = size;
            if (immediately)
                Raylib.SetWindowSize((int)size.X, (int)size.Y);
        }

        public void SetWindowTitle(string title, bool immediately)
        {
            this.Title = title;
            if (immediately)
                Raylib.SetWindowTitle(title);
        }

        public void Run()
        {
            Initialize();

            Raylib.InitWindow((int)WindowSize.X, (int)WindowSize.Y, Title);
            Raylib.SetExitKey((KeyboardKey)(-1));

            LoadContent();

            while (!Raylib.WindowShouldClose())
            {
                Update();
                Render();
            }

            Unload();
            Raylib.CloseWindow();
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Render();
        public abstract void Unload();
    }
}
