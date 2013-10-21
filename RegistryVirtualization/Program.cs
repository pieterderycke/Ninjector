using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ninjector;

namespace Ninjector
{
    class Program
    {
        private const string TestApplication = 
            @"C:\Users\Pieter\Documents\Visual Studio 2012\Projects\ReadRegKey\ReadRegKey\bin\x86\Debug\ReadRegKey.exe";

        static void Main(string[] args)
        {
            int lastWin32Error;

            InjectableProcess process = new InjectableProcess(TestApplication, Win32.NORMAL_PRIORITY_CLASS | Win32.CREATE_SUSPENDED);
            MemoryWriter writer = process.CreateMemoryWriter(1024);

            IntPtr advapi32 = writer.WriteValue("Advapi32.dll");
            IntPtr registryKeyAddress = writer.WriteValue(@"Pieter\Test");
            IntPtr registryHkeyAddress = writer.Alloc(4);

            writer.CallLoadLibrary(advapi32);
            writer.CallRegOpenKey(0x80000001, registryKeyAddress, registryHkeyAddress); //HKEY_CURRENT_USER
            writer.CallRegOverridePredefKey(0x80000001, registryHkeyAddress);
            writer.CallRegCloseKey(registryHkeyAddress);
            writer.CallExitThread();

            // Change page protection so we can write executable code
            //VirtualProtectEx(hProcess, codecaveAddress, workspaceIndex, MemoryProtection.ExecuteReadWrite, &oldProtect);

            Task task = process.CreateRemoteThread(writer);
            task.Wait();

            process.Resume();
        }
    }
}
