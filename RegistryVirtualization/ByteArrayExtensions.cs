using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegistryVirtualization
{
    public static class ByteArrayExtensions
    {
        public static int CopyString(this byte[] buffer, int offset, string value)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);

            Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
            buffer[offset + bytes.Length] = 0x00;
            buffer[offset + bytes.Length + 1] = 0x00;

            return bytes.Length + 2;
        }

        public static int CopyInt32(this byte[] buffer, int offset, int value)
        {
            //value.CopyToByteArray(buffer, offset);

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            // check if there is enough space for all the 4 bytes we will copy
            if (buffer.Length < offset + 4)
                throw new ArgumentException("Not enough room in the destination array");

            buffer[offset + 3] = (byte)(value >> 24); // fourth byte
            buffer[offset + 2] = (byte)(value >> 16); // third byte
            buffer[offset + 1] = (byte)(value >> 8); // second byte
            buffer[offset] = (byte)value; // last byte is already in proper position

            return 4;
        }

        public static int CopyInt32(this byte[] buffer, int offset, uint value)
        {
            //value.CopyToByteArray(buffer, offset);

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            // check if there is enough space for all the 4 bytes we will copy
            if (buffer.Length < offset + 4)
                throw new ArgumentException("Not enough room in the destination array");

            buffer[offset + 3] = (byte)(value >> 24); // fourth byte
            buffer[offset + 2] = (byte)(value >> 16); // third byte
            buffer[offset + 1] = (byte)(value >> 8); // second byte
            buffer[offset] = (byte)value; // last byte is already in proper position

            return 4;
        }
    }
}
