using System;

namespace WindleKnot
{
    public class LZS
    {
        public LZS()
        {
        }

        private const uint WINDOW_SIZE = 2048;
        private const uint PREFIX_SIZE = 2;
        private const uint HASHCHAIN_SIZE = WINDOW_SIZE * 2;
        private const uint HASHCHAIN_MASK = WINDOW_SIZE * 2 - 1;
        private const uint MIN_MATCH_LEN = 2;
        private const uint MAX_HIT_COUNT = WINDOW_SIZE;

        public byte[] Compress(byte[] uncompressedData, uint dataLen, bool optimal, out uint outSize)
        {
            BitStream bs = new BitStream(uncompressedData);
            if (bs == null)
            {
                outSize = 0;
                return null;
            }

            byte[] newBuffer = new byte[dataLen * 9 / 8 + 2];

            if (newBuffer == null)
            {
                outSize = 0;
                return null;
            }

            bs.Buffer = newBuffer;

            HashChain hc = new HashChain();

            //initialize with the first byte, which will always be a literal
            bs.WriteBits(uncompressedData[0], 9);

            for (uint i = 1; i < dataLen; i++)
            {
                uint matchPos = uint.MaxValue;
                uint matchLen = 0;

                if (!optimal)
                {
                    matchLen = HashFindMatch(ref hc, uncompressedData, i, dataLen, out matchPos);
                }
                else
                {
                    matchLen = SimpleFindMatch(uncompressedData, i, dataLen, out matchPos);
                }

                if (matchLen >= MIN_MATCH_LEN && i < dataLen)
                {
                    uint offset = i - matchPos;

                    if (offset < 128)
                        bs.WriteBits(0x180 | offset, 9);
                    else if (offset < 2048)
                        bs.WriteBits(0x1000 | offset, 13);
                    else
                    {
                        outSize = 0;
                        throw new IndexOutOfRangeException($"Illegal calculated offset of {offset:X}. (talk to the dev/maint about this one)");
                    }

                    // Write the match length
                    if (matchLen < 5)
                        bs.WriteBits(matchLen - 2, 2);
                    else if (matchLen < 8)
                        bs.WriteBits(0xC | (matchLen - 5), 4);
                    else
                    {
                        bs.WriteBits(0xF, 4);
                        uint tml = matchLen - 8;
                        while (tml > 14)
                        {
                            bs.WriteBits(0xF, 4);
                            tml -= 15;
                        }
                        bs.WriteBits(tml, 4);
                    }

                    while (--matchLen > 0)
                        Hash_Insert(ref hc, uncompressedData, ++i);
                }
                else
                {
                    bs.WriteBits(uncompressedData[i], 9);
                }
            }

            bs.WriteBits(0x180, 9);
            outSize = bs.BytePos;
            return bs.Buffer;

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



        public class HashChain
        {
            public uint[] HeadTable;
            public uint[] Chain;

            public HashChain()
            {
                HeadTable = new uint[HASHCHAIN_SIZE];
                Chain = new uint[HASHCHAIN_SIZE];

                Array.Fill(HeadTable, uint.MaxValue);
                Array.Fill(Chain, uint.MaxValue);
            }
        }
        private uint HashPrefix(byte[] buffer, uint pos)
        {
            if (pos >= buffer.Length) return 0;

            switch (PREFIX_SIZE)
            {
                case 1:
                    return Hash16(buffer[pos]);

                case 2:
                    if (pos + 1 >= buffer.Length) return 0;
                    return Hash16((ushort)(buffer[pos] | (buffer[pos + 1] << 8)));

                case 3:
                    if (pos + 2 >= buffer.Length) return 0;
                    return Hash32((uint)(buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16)));

                default:
                    return 0;
            }
        }

        private ushort Hash16(ushort x)
        {
            x ^= (ushort)(x >> 8);
            x *= 0x88b5;
            x ^= (ushort)(x >> 7);
            x *= 0xdb2d;
            x ^= (ushort)(x >> 9);
            return x;
        }

        private uint Hash32(uint x)
        {
            x ^= x >> 16;
            x *= 0x7feb352d;
            x ^= x >> 15;
            x *= 0x846ca68b;
            x ^= x >> 16;
            return x;
        }

        public void Hash_Insert(ref HashChain hc, byte[] buffer, uint pos)
        {
            uint key = HashPrefix(buffer, pos) & HASHCHAIN_MASK;

            hc.Chain[pos % HASHCHAIN_SIZE] = hc.HeadTable[key];
            hc.HeadTable[key] = pos;
        }
        private uint SimpleFindMatch(byte[] buffer, uint pos, uint bufferEnd, out uint matchPos)
        {
            uint bestLen = 0;
            matchPos = 0;

            uint minPos = (uint)Math.Max(0, (int)pos - (int)WINDOW_SIZE);

            for (uint i = pos - 1; i > minPos; i--)
            {
                uint matchLen = MatchLen(buffer, i, pos, bufferEnd);

                if (matchLen > bestLen)
                {
                    bestLen = matchLen;
                    matchPos = i;
                }
            }

            return bestLen;
        }

        private uint HashFindMatch(ref HashChain hc, byte[] buffer, uint pos, uint bufferEnd, out uint matchPos)
        {
            uint bestLen = 0;
            matchPos = 0;

            uint key = HashPrefix(buffer, pos) & HASHCHAIN_MASK;
            uint next = hc.HeadTable[key];

            uint minPos = (uint)Math.Max(0, (int)pos - (int)WINDOW_SIZE);
            uint hits = 0;

            while (next > minPos && ++hits < MAX_HIT_COUNT)
            {
                uint matchLen = MatchLen(buffer, pos, next, bufferEnd);

                if (matchLen > bestLen)
                {
                    bestLen = matchLen;
                    matchPos = next;
                }

                next = hc.Chain[next % HASHCHAIN_SIZE];
            }

            hc.Chain[pos % HASHCHAIN_SIZE] = hc.HeadTable[key];
            hc.HeadTable[key] = pos;

            return bestLen;
        }

        private uint MatchLen(byte[] buffer, uint pos1, uint pos2, uint bufferEnd)
        {
            uint length = 0;

            while (pos1 < bufferEnd && pos2 < bufferEnd && buffer[pos1] == buffer[pos2])
            {
                length++;
                pos1++;
                pos2++;
            }

            return length;
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
