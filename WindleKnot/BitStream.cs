using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindleKnot
{
    internal class BitStream
    {
        public byte[] Buffer;  // The byte array containing the data
        public uint BytePos;   // The current byte position
        public uint BitPos;    // The current bit position within the byte

        // Constructor initializes the BitStream with a buffer
        public BitStream(byte[] buffer)
        {
            Buffer = buffer;
            BytePos = 0;
            BitPos = 0;
        }

        // Read the specified number of bits from the bitstream (MSB order)
        public uint ReadBits(uint bitCount)
        {
            uint result = 0;
            uint curAmt = 0;

            while (curAmt < bitCount)
            {
                // How many bits we can extract from the current byte
                uint remainder = bitCount - curAmt;
                uint byteSpace = 8 - BitPos;

                if (remainder < byteSpace)
                {
                    // We can read all remaining bits from the current byte
                    result <<= (int)remainder;
                    uint remMask = (1U << (int)byteSpace) - 1;
                    result |= (uint)(Buffer[BytePos] & remMask) >> (int)(byteSpace - remainder);
                    curAmt = bitCount;
                    BitPos += remainder;
                }
                else
                {
                    // We need to cross a byte boundary
                    result <<= (int)byteSpace;
                    uint remMask = (1U << (int)byteSpace) - 1;
                    result |= (uint)(Buffer[BytePos++] & remMask);
                    curAmt += byteSpace;
                    BitPos = 0;
                }
            }

            return result;
        }

        // Write the specified number of bits to the bitstream (MSB order)
        public void WriteBits(uint value, uint bitCount)
        {
            uint curAmt = 0;

            while (curAmt < bitCount)
            {
                uint remainder = bitCount - curAmt;
                uint byteSpace = 8 - BitPos;

                if (remainder < byteSpace)
                {
                    // Write the value into the current byte
                    uint valueMask = (1U << (int)remainder) - 1;
                    Buffer[BytePos] |= (byte)((value & valueMask) << (int)(byteSpace - remainder));
                    curAmt = bitCount;
                    BitPos += remainder;
                }
                else
                {
                    // Fill the byte and move to the next byte
                    uint valueMask = ((1U << (int)byteSpace) - 1) << (int)(remainder - byteSpace);
                    Buffer[BytePos++] |= (byte)((value & valueMask) >> (int)(remainder - byteSpace));
                    curAmt += byteSpace;
                    BitPos = 0;
                }
            }
        }
    }
}
