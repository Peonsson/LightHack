using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LightHack
{
    class Program
    {

        [DllImport("kernel32")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32")]
        public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);

        public static byte[] ReadBytes(IntPtr Handle, Int64 Address, uint BytesToRead)
        {
            IntPtr bytesRead;
            byte[] buffer = new byte[BytesToRead];
            ReadProcessMemory(Handle, new IntPtr(Address), buffer, BytesToRead, out bytesRead);
            return buffer;
        }
        public static int ReadInt32(Int64 Address, IntPtr Handle)
        {
            return BitConverter.ToInt32(ReadBytes(Handle, Address, 4), 0);
        }

        public static string ReadString(long Address, IntPtr Handle, uint length = 32)
        {
            return ASCIIEncoding.Default.GetString(ReadBytes(Handle, Address, length)).Split('\0')[0];
        }

        public static void WriteMemory(IntPtr Address, byte[] buffer, out int bytesWritten, IntPtr Handle)
        {
            IntPtr pBytesWritten = IntPtr.Zero;
            WriteProcessMemory(Handle, Address, buffer, (uint)buffer.Length, out pBytesWritten);
            bytesWritten = pBytesWritten.ToInt32();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("classictibia light-hack..");

            UInt32 Adr_BListBegin = 0x1C68B4;
            UInt32 Adr_BL_Light_Offset = 0x70;
            UInt32 Adr_BL_Brightness_Offset = 0x74;
            int bytesOut;

            while (true)
            {
                Process[] processes = Process.GetProcessesByName("classictibia");
                foreach (Process proc in processes)
                {
                    int counter = 0;
                    UInt32 base1 = 0;
                    while (true)
                    {
                        if (ReadString(proc.MainModule.BaseAddress.ToInt32() + Adr_BListBegin + (0x9C * counter), proc.Handle).Equals("Friend") || ReadString(proc.MainModule.BaseAddress.ToInt32() + Adr_BListBegin + (0x9C * counter), proc.Handle).Equals("Peon"))
                        {
                            base1 = (uint)proc.MainModule.BaseAddress.ToInt32() + Adr_BListBegin + (0x9C * (uint)counter);
                            break;
                        }
                        counter++;
                        if (counter > 250)
                        {
                            Console.WriteLine("An unexpected error has occured.");
                            Console.ReadLine();
                            return;
                        }
                    }

                    if (ReadInt32(base1 + Adr_BL_Light_Offset, proc.Handle) < 12)
                    {
                        WriteMemory((IntPtr)(base1 + Adr_BL_Light_Offset), BitConverter.GetBytes(12), out bytesOut, proc.Handle);
                    }
                    if (ReadInt32(base1 + Adr_BL_Brightness_Offset, proc.Handle) < 215)
                    {
                        WriteMemory((IntPtr)(base1 + Adr_BL_Brightness_Offset), BitConverter.GetBytes(215), out bytesOut, proc.Handle);
                    }
                }
                Thread.Sleep(25);
            }
        }
    }
}
