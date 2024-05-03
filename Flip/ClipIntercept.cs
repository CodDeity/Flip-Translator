using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Flip
{
    public static class ClipIntercept
    {
        [DllImport("FlipIntercept.dll")]
        private static extern void RunClipboardLoop(IntPtr CallbackPointer);
        public delegate bool ClipInterceptDelegate(string data);
        public static event EventHandler<string> OnClipboardDataChange = null;
        private static CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private static List<char>? FilteredChars = null;
        private static bool working = false;
        private static bool started = false;
        public static bool Intercepting { get { return working; } }
        private static bool ClipInterceptCallback(string data)
        {
            if(OnClipboardDataChange != null && working)
            {
                if (FilteredChars != null)
                {
                    foreach (char c in FilteredChars)
                    {
                        if (data.Contains(c))
                            return true;
                    }
                }
                OnClipboardDataChange(null, data);
            }
            return true;
        }
        public static void StartClipIntercept(List<char>? Filtered)
        {
            if (!working && !started)
            {
                FilteredChars = Filtered;
                cancellationToken = new CancellationTokenSource();
                ClipInterceptDelegate c0 = ClipInterceptCallback;
                Task.Run(() => RunClipboardLoop(Marshal.GetFunctionPointerForDelegate(c0)), cancellationToken.Token);
                started = true;
                working = true;
            }
            if(!working && started)
                working = true;
        }
        public static void StopClipIntercept()
        {
            working = false;
        }
        
    }
}
