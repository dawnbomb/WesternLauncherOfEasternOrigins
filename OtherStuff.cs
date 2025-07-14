using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WesternLauncherOfEasternOrigins
{
    internal class OtherStuff
    {
    }

    //This is all shit chat GPT told me to put somewhere


    public static class GamePad //used to check if gamepad is plugged in before launching a game.
    {
        [DllImport("xinput1_4.dll")]
        private static extern int XInputGetState(int dwUserIndex, ref XINPUT_STATE pState);

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public int dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        public static bool IsControllerConnected(int playerIndex)
        {
            XINPUT_STATE state = new XINPUT_STATE();
            int result = XInputGetState(playerIndex, ref state);
            return result == 0;
        }
    }

    public class NativeMethods //used to automate applying th practise tool.
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint Type;
            public InputUnion Data;
            [StructLayout(LayoutKind.Explicit)]
            public struct InputUnion
            {
                [FieldOffset(0)]
                public MOUSEINPUT Mouse;
                [FieldOffset(0)]
                public KEYBDINPUT Keyboard;
                [FieldOffset(0)]
                public HARDWAREINPUT Hardware;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        public const uint INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_KEYUP = 2;

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }


    public class WindowHandler //used to minimize the touhou community patcher
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        const int SW_MINIMIZE = 6;
        private const uint WM_CLOSE = 0x0010;

        public static void MinimizeWindow(string windowTitle)
        {
            // Find the window by its title
            IntPtr hWnd = FindWindow(null, windowTitle);
            if (hWnd != IntPtr.Zero)
            {
                // Minimize the window
                ShowWindow(hWnd, SW_MINIMIZE);
            }
            else
            {
                Console.WriteLine("Window not found!");
            }
        }

        public static void CloseWindow(string title)
        {
            IntPtr hWnd = FindWindow(null, title);
            if (hWnd != IntPtr.Zero)
            {
                SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
