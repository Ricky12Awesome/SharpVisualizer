using System;
using System.Threading;
using ImGuiNET;
using NAudio.Dsp;
using NAudio.Wave;
using Raylib_cs;

namespace Raygui
{
    internal class Program
    {
        private static unsafe void Main(string[] args)
        {
            var api = new WasapiLoopbackCapture();
            var (width, height) = (1920, 1080);
            Raylib.InitWindow(width, height, "SharpVisualizer");
            Raylib.SetWindowMinSize(1280, 720);
            
            const int size = 8192;
            var data = stackalloc Complex[size + 9600];
            var rects = stackalloc Rectangle[size + 9600];
            var dataLock = new Mutex();
            
            api.DataAvailable += (_, eventArgs) =>
            {
                var len = eventArgs.Buffer.Length / sizeof(float);
            
                if (len > 9600)
                {
                    return;
                }
            
                lock (dataLock)
                {
                    fixed (byte* buffer = eventArgs.Buffer)
                    {
                        var buf = (float*) buffer;
            
                        for (var i = 0; i < len; i++)
                        {
                            data[i].X = buf[i];
                            data[i].Y = 0.0f;
                        }
            
                        FastFourierTransformUnsafe.FFT(false, (int) Math.Log2(size), data);
                    }
                }
            };
            
            api.RecordingStopped += (_, eventArgs) => { Console.WriteLine(eventArgs.Exception?.ToString()); };
            
            api.StartRecording();

            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F11))
                {
                    Raylib.SetWindowSize(Raylib.GetMonitorWidth(0), Raylib.GetMonitorHeight(0));
                    Raylib.ToggleFullscreen();

                    if (!Raylib.IsWindowFullscreen())
                    {
                        Raylib.SetWindowSize(width, height);
                    }
                }
                
                Raylib.BeginDrawing();

                lock (dataLock)
                {
                    Draw(data, rects, size);
                }

                Raylib.EndDrawing();
            }
            
            Raylib.CloseWindow();
            api.StopRecording();
        }

        public static unsafe void Draw(Complex* buf, Rectangle* rects, int len)
        {
            Raylib.ClearBackground(new Color(32, 32, 32, 255));

            var width = (float) Raylib.GetScreenWidth();
            var gap = width / (len + 1);

            for (var i = 0; i < len; i++)
            {
                var value = buf[i].X * 12.5f;
                var rect = rects[i];

                rect.x = gap * i;
                rect.y = Raylib.GetScreenHeight() - value;
                rect.width = gap;
                rect.height = value;

                Raylib.DrawRectangleRec(rect, Color.RED);
            }
        }
    }
}