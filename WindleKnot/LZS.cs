using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindleKnot
{
    public class LZS
    {
        public LZS()
        {
        }

        public byte[] Decompress(byte[] compressedData, uint dataLen, uint expectedSize, out uint outSize)
        {
            BitStream bs = new BitStream(compressedData);

            if (bs == null)
            {
                outSize = 0;
                return null;
            }

            byte[] outBytes = new byte[expectedSize];
            if (outBytes == null)
            {
                outSize = 0;
                return null;
            }

            uint outPos = 0;

            while (true)
            {
                // Error Handling
                if (bs.BytePos > dataLen)
                {
                    // Failed to find end token
                    outSize = 0;
                    return null;
                }

                //Token parsing
                if (bs.ReadBits(1) == 1)
                {
                    // Compression handling
                    ushort offset = 0;

                    if (bs.ReadBits(1) == 1)
                    {
                        // Seven bit offset
                        offset = (ushort)bs.ReadBits(7);
                        if (offset == 0) // offset of 0 len 7 indicates EOF
                            break;
                    }
                    else
                    {
                        // Eleven bit offset
                        offset = (ushort)bs.ReadBits(11);
                    }

                    // Get length of compressed byte stream
                    uint clen = GetCompLen(bs);

                    // Finish with copy to output
                    if (CopyComp(outBytes, ref outPos, offset, clen) == -1)
                    {
                        outSize = 0;
                        return null;
                    }
                }
                else
                {
                    // Write literal to output
                    byte literal = (byte)bs.ReadBits(8);
                    outBytes[outPos++] = literal;
                }
            }

            outSize = outPos;
            return outBytes;
        }

        private uint GetCompLen(BitStream bs)
        {
            uint clen;
            ushort nibble;
            switch (bs.ReadBits(2))
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 4;
                default:
                    switch (bs.ReadBits(2))
                    {
                        case 0:
                            return 5;
                        case 1:
                            return 6;
                        case 2:
                            return 7;
                        default:
                            clen = 8;
                            do
                            {
                                nibble = (ushort)bs.ReadBits(4);
                                clen += nibble;
                            } while (nibble == 0xf);
                            return clen;
                    }
            }
        }

        private static int CopyComp(byte[] data, ref uint dataPos, ushort offset, uint clen)
        {
            // Check if the offset is valid
            uint tempPos = dataPos;
            if (tempPos < offset)
            {
                return -1;
            }

            // Copy compressed bytes to output
            while (clen-- > 0)
            {
                data[tempPos] = data[tempPos - offset];
                tempPos++;
            }

            dataPos = tempPos;
            return 0;
        }
    }
    
}
