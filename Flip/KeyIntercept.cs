using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Timers;

namespace Flip
{
    public static class KeyIntercept
    {
        [DllImport("FlipIntercept.dll")]
        private static extern void RunKeyIntercept(IntPtr CallbackPointer);
        public delegate bool KeyInterceptDelegate(int key);
        public static event EventHandler<KeyInterceptEventArgs> OnKeyPressed = null;
        private static int TargetKeyCode = -1;
        private static int PressedCount = 1;
        private static System.Timers.Timer PressTimer = new System.Timers.Timer(1000);

        static int KeyDownCount = 0;
        private static void PressTimer_Elapsed(object? sender, ElapsedEventArgs e) => KeyDownCount = 0;
        private static bool KeyInterceptCallBack(int Pressed)
        {
            if(Pressed == TargetKeyCode)
            {
                if(KeyDownCount < PressedCount)
                {
                    KeyDownCount++;
                    if(KeyDownCount == 1)
                    {
                        PressTimer.Start();
                    }
                }
                else
                {
                    PressTimer.Stop();
                    KeyDownCount = 0;
                    if(OnKeyPressed != null)
                        OnKeyPressed(null , new KeyInterceptEventArgs(Pressed));
                }
            }
            return true;
        }
        public static void StartIntercept(int PressedTime,int KeyCode)
        {
            KeyInterceptDelegate k0 = KeyInterceptCallBack;
            TargetKeyCode = KeyCode;
            PressedCount = PressedTime;
            if(TargetKeyCode == -1)
            {
                throw new Exception("targetKeyCode must be set");
            }
            RunKeyIntercept(Marshal.GetFunctionPointerForDelegate(k0));
            PressTimer.Elapsed += PressTimer_Elapsed;
        }
        
    }
    public class KeyInterceptEventArgs : EventArgs
    {
        public int Key { get; set; }
        public KeyInterceptEventArgs(int key)
        {
            Key = key;
        }
    }
}
