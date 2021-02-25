using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Display
{
    abstract class BaseWindow
    {
        private Vector2 _windowSize;
        /// <summary>
        /// Window size prop. Automatically changes the window size when changed.
        /// </summary>
        public Vector2 WindowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                _windowSize = value;
                Raylib.SetWindowSize((int)value.X, (int)value.Y);
            }
        }

        private string _title;
        /// <summary>
        /// Changes window title instantly as you change the value of this property.
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                Raylib.SetWindowTitle(value);
            }
        }

        public BaseWindow() : this(new Vector2(800, 600), "Base Window")
        {

        }

        public BaseWindow(Vector2 windowSize, string title)
        {
            this._windowSize = windowSize;
            this._title = title;
        }

        public void Run()
        {
            Initialize();

            Raylib.InitWindow((int)WindowSize.X, (int)WindowSize.Y, Title);

            LoadContent();

            while (!Raylib.WindowShouldClose())
            {
                Update();
                Render();
            }

            Raylib.CloseWindow();
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Render();
    }
}
