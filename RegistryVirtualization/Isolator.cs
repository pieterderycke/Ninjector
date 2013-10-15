using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegistryVirtualization
{
    public class Isolator
    {
        private IntPtr exitThread;
        private IntPtr regOpenKey;
        private IntPtr regOverridePredefKey;

        public void Start()
        {
            LoadAddresses();

            byte[] buffer = new byte[1024];
            uint bufferIndex = 0;


        }

        private void LoadAddresses()
        {
            IntPtr kernel32 = Win32.LoadLibrary("kernel32.dll");
            IntPtr advapi32 = Win32.LoadLibrary("Advapi32.dll");

            exitThread = Win32.GetProcAddress(kernel32, "ExitThread");
            regOverridePredefKey = Win32.GetProcAddress(advapi32, "RegOverridePredefKey");
            regOpenKey = Win32.GetProcAddress(advapi32, "RegOpenKeyW");
        }
    }
}
