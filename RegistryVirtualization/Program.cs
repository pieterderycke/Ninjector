using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace RegistryVirtualization
{
    class Program
    {


        private static IntPtr exitThread;
        private static IntPtr regOpenKey;

        static void Main(string[] args)
        {
            string application = @"C:\Program Files (x86)\Java\jre7\bin\java.exe";
            int lastWin32Error;

            string registryKey = "test1";
            string overrideRegistryKey = "test1";

            Win32.PROCESS_INFORMATION processInfo;
            Win32.STARTUPINFO startupInfo = new Win32.STARTUPINFO();

            Win32.CreateProcess(application, null, IntPtr.Zero, IntPtr.Zero, false, Win32.NORMAL_PRIORITY_CLASS | Win32.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref startupInfo, out processInfo);

            lastWin32Error = Marshal.GetLastWin32Error();

            IntPtr hProcess = processInfo.hProcess;

            IntPtr processesStartAddress = Win32.VirtualAllocEx(hProcess, IntPtr.Zero, 1024, 
                Win32.AllocationType.Commit | Win32.AllocationType.Reserve, Win32.MemoryProtection.ExecuteReadWrite);

            lastWin32Error = Marshal.GetLastWin32Error();

            MemoryWriter writer = new MemoryWriter(processesStartAddress, 1024);

            IntPtr registryKeyAddress = writer.WriteValue(registryKey);
            IntPtr overrideRegistryKeyAddress = writer.WriteValue(overrideRegistryKey);
            IntPtr registryHkeyAddress = writer.Alloc(4);

            writer.CallRegOpenKey(0x80000001, registryKeyAddress, registryHkeyAddress); //HKEY_CURRENT_USER
            writer.CallExitThread();

            // Change page protection so we can write executable code
            //VirtualProtectEx(hProcess, codecaveAddress, workspaceIndex, MemoryProtection.ExecuteReadWrite, &oldProtect);

            UIntPtr bytesWriten;
            Win32.WriteProcessMemory(hProcess, processesStartAddress, writer.Buffer, 
                (uint)writer.Size, out bytesWriten);

            lastWin32Error = Marshal.GetLastWin32Error();

            Win32.FlushInstructionCache(hProcess, processesStartAddress, new UIntPtr((uint)writer.Size));

            lastWin32Error = Marshal.GetLastWin32Error();

            IntPtr hThread = Win32.CreateRemoteThread(hProcess, IntPtr.Zero, 0, writer.CodeStartTargetAddress, 
                IntPtr.Zero, 0, IntPtr.Zero);

            //lastWin32Error = Marshal.GetLastWin32Error();

            Win32.WaitForSingleObject(hThread, Win32.INFINITE);

            // Free the memory in the process that we allocated
            Win32.VirtualFreeEx(hProcess, processesStartAddress, 0, Win32.FreeType.Release);

            // Resume process
            Win32.ResumeThread(processInfo.hThread);
        }

        private static uint WriteRegOpenKey(byte[] buffer, uint bufferIndex, uint hkey, IntPtr subKey, IntPtr result)
        {
            buffer[bufferIndex++] = 0x68; // PUSH
            bufferIndex += (uint)buffer.CopyInt32((int)bufferIndex, result.ToInt32());

            buffer[bufferIndex++] = 0x68; // PUSH
            bufferIndex += (uint)buffer.CopyInt32((int)bufferIndex, subKey.ToInt32());

            buffer[bufferIndex++] = 0x68; // PUSH
            bufferIndex += (uint)buffer.CopyInt32((int)bufferIndex, 0x80000001); //HKEY_CURRENT_USER

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += (uint)buffer.CopyInt32((int)bufferIndex, regOpenKey.ToInt32()); // Address of RegOpenKey

            buffer[bufferIndex++] = 0xFF; // CALL
            buffer[bufferIndex++] = 0xD0; // EAX

            return bufferIndex;
        }

        private static uint WriteExitThread(byte[] buffer, uint bufferIndex)
        {
            buffer[bufferIndex++] = 0x6A; // PUSH
            buffer[bufferIndex++] = 0x00; // Constant 0

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += (uint)buffer.CopyInt32((int)bufferIndex, exitThread.ToInt32()); // Address of ExitThread

            buffer[bufferIndex++] = 0xFF; // CALL
            buffer[bufferIndex++] = 0xD0; // EAX

            return bufferIndex;
        }
    }
}
