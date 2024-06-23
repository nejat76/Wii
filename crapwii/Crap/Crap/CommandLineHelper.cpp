// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Runtime.InteropServices;

namespace Dnp
{
    public abstract class CommandLineHandler
    {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
         [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            string executableName;
            return CommandLineToArgs(commandLine, out executableName);
        }

        public static string[] CommandLineToArgs(string commandLine, out string executableName)
        {
            int argCount;
            IntPtr result;
            string arg;
            IntPtr pStr;
            result = CommandLineToArgvW(commandLine, out argCount);


            if (result == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception();
            }
            else
            {
                try
                {
                    // Jump to location 0*IntPtr.Size (in other words 0).
                    pStr = Marshal.ReadIntPtr(result, 0 * IntPtr.Size);
                    executableName = Marshal.PtrToStringUni(pStr);

                    // Ignore the first parameter because it is the application
                    // name which is not usually part of args in Managed code.
                    string[] args = new string[argCount-1];
                    for (int i = 0; i < args.Length; i++)
                    {
                        pStr = Marshal.ReadIntPtr(result, (i+1) * IntPtr.Size);
                        arg = Marshal.PtrToStringUni(pStr);
                        args[i] = arg;
                    }

                    return args;
                }
                finally
                {
                    Marshal.FreeHGlobal(result);
                }
            }
        }

        // ...
    }
}


