using System.Runtime.InteropServices;

namespace SynthriderzMapUpdateTool
{
    public class ConsoleSetup
    {
        const int MF_BYCOMMAND = 0x00000000;
        const int SC_SIZE = 0xF000;
        public const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll")]
        static extern int DeleteMenu(nint hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        static extern nint GetSystemMenu(nint hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        static extern nint GetConsoleWindow();

        public ConsoleSetup(string title)
        {
            Console.Title = title;

            nint handle = GetConsoleWindow();
            nint sysMenu = GetSystemMenu(handle, false);

            if (handle != nint.Zero)
            {
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            }

            // hide scrollbar
            Console.SetWindowSize(111, 35);
            Console.SetBufferSize(111, 35);
        }
    }
}
