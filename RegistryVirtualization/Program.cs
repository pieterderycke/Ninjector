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

        static void Main(string[] args)
        {
            //string application = @"C:\Program Files (x86)\Java\jre7\bin\java.exe";
            string application = @"C:\Users\Pieter\Documents\Visual Studio 2012\Projects\ReadRegKey\ReadRegKey\bin\x86\Debug\ReadRegKey.exe";
            int lastWin32Error;

            string registryKey = @"Pieter\Test";
            //string overrideRegistryKey = @"Pieter\Test";

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
            IntPtr registryHkeyAddress = writer.Alloc(4);

            //IntPtr overrideKeyAddress = writer.WriteValue(overrideRegistryKey);
            //IntPtr overrideHKeyAddress = writer.Alloc(4);

            IntPtr advapi32 = writer.WriteValue("Advapi32.dll");

            writer.CallLoadLibrary(advapi32);
            writer.CallRegOpenKey(0x80000001, registryKeyAddress, registryHkeyAddress); //HKEY_CURRENT_USER
            //writer.CallGetLastError();
            //writer.CallRegOpenKey(0x80000001, overrideKeyAddress, overrideHKeyAddress); //HKEY_CURRENT_USER
            //writer.CallGetLastError();
            //writer.CallRegOverridePredefKey(0x80000001, registryHkeyAddress);
            //writer.CallGetLastError();
            //writer.CallRegCloseKey(registryHkeyAddress);
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
    }
}
