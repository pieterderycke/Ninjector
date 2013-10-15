using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegistryVirtualization
{
    public static class Int32Extensions
    {
        public static void CopyToByteArray(this int source, byte[] destination, uint offset)
        {
            if (destination == null)
                throw new ArgumentException("Destination array cannot be null");

            // check if there is enough space for all the 4 bytes we will copy
            if (destination.Length < offset + 4)
                throw new ArgumentException("Not enough room in the destination array");

            destination[offset+3] = (byte)(source >> 24); // fourth byte
            destination[offset+2] = (byte)(source >> 16); // third byte
            destination[offset+1] = (byte)(source >> 8 ); // second byte
            destination[offset] = (byte)source; // last byte is already in proper position
        }
    }
}
