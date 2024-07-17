using legallead.reader.service.utility;
using System.Runtime.InteropServices;
using System.Timers;

namespace legallead.reader.service.services
{
    internal partial class MainWindowService : IMainWindowService
    {
        [LibraryImport("User32")]
        private static partial int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
            "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time",
            Justification = "<Pending>")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        private static readonly object lockme = new();
        private readonly System.Timers.Timer aTimer;
        private bool isWindowVisible;
        private readonly bool showAppWindow;

        public MainWindowService(IConfiguration? configuration)
        {
            isWindowVisible = true;
            if (configuration == null)
            {
                showAppWindow = false;
            }
            else
            {
                string environ = ServiceExtensions.GetConfigOrDefault(configuration, "ShowAppWindow", "false");
                _ = bool.TryParse(environ, out var isActive);
                showAppWindow = isActive;
            }
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(500);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            lock (lockme)
            {
                if (!isWindowVisible) { return; }
                isWindowVisible = !HideSelections(!showAppWindow);
            }
        }

        private static bool HideSelections(bool hideApplication = true)
        {
            const int sw_hide = 0;
            if (!hideApplication) { return false; }
            var handle = GetConsoleWindow().ToInt32();
            _ = ShowWindow(handle, sw_hide);
            return true;
        }

        public bool IsMainVisible => isWindowVisible;
    }
}