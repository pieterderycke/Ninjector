using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegistryVirtualization
{
    public class MemoryWriter
    {
        private static readonly IntPtr exitThread;
        private static readonly IntPtr regOpenKey;
        private static readonly IntPtr regOverridePredefKey;
        private static readonly IntPtr loadLibrary;

        private readonly IntPtr targetStartAddress;
        private readonly byte[] buffer;
        private int bufferIndex;

        private bool canWriteData;
        private IntPtr codeStartTargetAddress;

        static MemoryWriter()
        {
            IntPtr kernel32 = Win32.LoadLibrary("kernel32.dll");
            IntPtr advapi32 = Win32.LoadLibrary("Advapi32.dll");

            exitThread = Win32.GetProcAddress(kernel32, "ExitThread");
            loadLibrary = Win32.GetProcAddress(kernel32, "LoadLibraryW");
            regOverridePredefKey = Win32.GetProcAddress(advapi32, "RegOverridePredefKey");
            regOpenKey = Win32.GetProcAddress(advapi32, "RegOpenKeyW");
        }

        public MemoryWriter(IntPtr targetStartAddress, int size)
        {
            this.targetStartAddress = targetStartAddress;
            this.buffer = new byte[size];
            this.bufferIndex = 0;
            this.canWriteData = true;
        }

        public byte[] Buffer
        {
            get { return buffer; }
        }

        public int Size
        {
            get { return buffer.Length; }
        }

        public IntPtr CodeStartTargetAddress
        {
            get { return codeStartTargetAddress; }
        }

        public IntPtr Alloc(int size)
        {
            VerifyDataWrite(size);            

            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += size;
            return address;
        }

        public IntPtr WriteValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            VerifyDataWrite((value.Length * 2) + 2); // value will be writen in UTF-16 format

            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += buffer.CopyString(bufferIndex, value);
            return address;
        }

        public IntPtr WriteValue(uint value)
        {
            VerifyDataWrite(sizeof(uint));

            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += buffer.CopyInt32(bufferIndex, value);
            return address;
        }

        public IntPtr WriteValue(int value)
        {
            VerifyDataWrite(sizeof(int));

            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += buffer.CopyInt32(bufferIndex, value);
            return address;
        }

        public void CallRegOpenKey(uint hkey, IntPtr subKey, IntPtr result)
        {
            MarkCodeStart();

            buffer[bufferIndex++] = 0x68; // PUSH
            bufferIndex += buffer.CopyInt32(bufferIndex, result.ToInt32());

            buffer[bufferIndex++] = 0x68; // PUSH
            bufferIndex += buffer.CopyInt32(bufferIndex, subKey.ToInt32());

            buffer[bufferIndex++] = 0x68; // PUSH
            bufferIndex += buffer.CopyInt32(bufferIndex, 0x80000001); //HKEY_CURRENT_USER

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, regOpenKey.ToInt32()); // Address of RegOpenKey

            buffer[bufferIndex++] = 0xFF; // CALL
            buffer[bufferIndex++] = 0xD0; // EAX
        }

        public void CallRegOverridePredefKey(IntPtr hKey, IntPtr newHkey)
        {
            MarkCodeStart();

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, newHkey.ToInt32()); // Address of RegOpenKey
            buffer[bufferIndex++] = 0x50; // PUSH EAX

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, hKey.ToInt32()); // Address of RegOpenKey
            buffer[bufferIndex++] = 0x50; // PUSH EAX

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, regOverridePredefKey.ToInt32()); // Address of RegOverridePredefKey

            buffer[bufferIndex++] = 0xFF; // CALL
            buffer[bufferIndex++] = 0xD0; // EAX
        }

        public void CallExitThread()
        {
            MarkCodeStart();

            buffer[bufferIndex++] = 0x6A; // PUSH
            buffer[bufferIndex++] = 0x00; // Constant 0

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, exitThread.ToInt32()); // Address of ExitThread

            buffer[bufferIndex++] = 0xFF; // CALL
            buffer[bufferIndex++] = 0xD0; // EAX
        }

        public void CallLoadLibrary(IntPtr lipFileName)
        {
            MarkCodeStart();

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, lipFileName.ToInt32()); // Address of RegOpenKey
            buffer[bufferIndex++] = 0x50; // PUSH EAX

            buffer[bufferIndex++] = 0xB8; // MOV EAX
            bufferIndex += buffer.CopyInt32(bufferIndex, loadLibrary.ToInt32()); // Address of LoadLibrary

            buffer[bufferIndex++] = 0xFF; // CALL
            buffer[bufferIndex++] = 0xD0; // EAX
        }

        private void VerifyDataWrite(int size)
        {
            if(!canWriteData)
                throw new Exception("Unable to write data, because execution code has already been writen.");

            if(buffer.Length < bufferIndex + size)
                throw new Exception("Not sufficient free space is available in the buffer to write the data.");
        }

        private void MarkCodeStart()
        {
            if (canWriteData)
            {
                this.canWriteData = false;
                this.codeStartTargetAddress = IntPtr.Add(targetStartAddress, bufferIndex);
            }
        }
    }
}