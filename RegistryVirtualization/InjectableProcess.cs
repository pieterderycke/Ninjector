using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RegistryVirtualization;

namespace Ninjector
{
    public class InjectableProcess
    {
        private Win32.PROCESS_INFORMATION processInfo;

        public InjectableProcess(string applicationPath, int creationFlags)
        {
            Win32.STARTUPINFO startupInfo = new Win32.STARTUPINFO();

            Win32.CreateProcess(applicationPath, null, IntPtr.Zero, IntPtr.Zero, false, Win32.NORMAL_PRIORITY_CLASS | Win32.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref startupInfo, out processInfo);
        }

        public MemoryWriter CreateMemoryWriter(int size)
        {
            IntPtr processesStartAddress = Win32.VirtualAllocEx(processInfo.hProcess, IntPtr.Zero, (uint)size,
                Win32.AllocationType.Commit | Win32.AllocationType.Reserve, Win32.MemoryProtection.ExecuteReadWrite);

            //lastWin32Error = Marshal.GetLastWin32Error();

            return new MemoryWriter(processesStartAddress, size);
        }

        // TODO -> should return Task
        public void CreateRemoteThread(MemoryWriter writer)
        {
            UIntPtr bytesWriten;
            Win32.WriteProcessMemory(processInfo.hProcess, writer.TargetStartAddress, writer.Buffer,
                (uint)writer.Size, out bytesWriten);

            //lastWin32Error = Marshal.GetLastWin32Error();

            Win32.FlushInstructionCache(processInfo.hProcess, writer.TargetStartAddress, new UIntPtr((uint)writer.Size));

            //lastWin32Error = Marshal.GetLastWin32Error();

            IntPtr hThread = Win32.CreateRemoteThread(processInfo.hProcess, IntPtr.Zero, 0, writer.CodeStartTargetAddress,
                IntPtr.Zero, 0, IntPtr.Zero);

            //lastWin32Error = Marshal.GetLastWin32Error();

            Win32.WaitForSingleObject(hThread, Win32.INFINITE);

            // Free the memory in the process that we allocated
            Win32.VirtualFreeEx(processInfo.hProcess, writer.TargetStartAddress, 0, Win32.FreeType.Release);
        }

        public void Resume()
        {
            Win32.ResumeThread(processInfo.hThread);
        }
    }
}
