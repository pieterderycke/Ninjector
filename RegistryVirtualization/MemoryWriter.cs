using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegistryVirtualization
{
    public class MemoryWriter
    {
        private readonly IntPtr targetStartAddress;
        private readonly byte[] buffer;
        private int bufferIndex;

        public MemoryWriter(IntPtr targetStartAddress, int size)
        {
            this.targetStartAddress = targetStartAddress;
            this.buffer = new byte[size];
            this.bufferIndex = 0;
        }

        public IntPtr WriteValue(string value)
        {
            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += buffer.CopyString(bufferIndex, value);
            return address;
        }

        public IntPtr WriteValue(uint value)
        {
            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += buffer.CopyInt32(bufferIndex, value);
            return address;
        }

        public IntPtr WriteValue(int value)
        {
            IntPtr address = IntPtr.Add(targetStartAddress, bufferIndex);
            bufferIndex += buffer.CopyInt32(bufferIndex, value);
            return address;
        }
    }
}