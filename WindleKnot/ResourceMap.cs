using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindleKnot
{
    public class ResourceMap
    {
        public ResourceMap()
        {

        }
        public List<Tuple<ushort, uint>> ViewMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> pictureMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> scriptMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> vocabMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> fontMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> patchMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> paletteMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> messageMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> mapMap = new List<Tuple<ushort, uint>>();
        public List<Tuple<ushort, uint>> heapMap = new List<Tuple<ushort, uint>>();

        public void MapResources(byte[] mapData)
        {
            // Read header. The header is 3 byte chunks. The first byte is the resource type. Byte 2 and 3 are the startLocation.
            int i = 0;
            List<Tuple<byte, ushort>> headerChunks = new List<Tuple<byte, ushort>>();


            while (true)
            {

                if (mapData[i] == 255) // End of header byte
                {
                    break;
                }

                byte resourceType = mapData[i];
                ushort startLocation = BitConverter.ToUInt16(mapData, i + 1);
                headerChunks.Add(new Tuple<byte, ushort>(resourceType, startLocation));

                i += 3;
            }

            for (int j = 0; j < headerChunks.Count; j++)
            {
                switch (headerChunks[j].Item1)
                {
                    case 0:
                        ViewMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 1:
                        pictureMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 2:
                        scriptMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 6:
                        vocabMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 7:
                        fontMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 9:
                        patchMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 11:
                        paletteMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 15:
                        messageMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 16:
                        mapMap = ExtractMap(mapData, headerChunks[j].Item2, headerChunks[j + 1].Item2);
                        break;
                    case 17:
                        heapMap = ExtractMap(mapData, headerChunks[j].Item2, mapData.Length);
                        break;

                }
            }
        }

        private List<Tuple<ushort, uint>> ExtractMap(byte[] mapData, int entry, int exit)
        {
            List<Tuple<ushort, uint>> resourceMap = new List<Tuple<ushort, uint>>();

            for (int i = entry; i < exit; i += 6)
            {
                // Extract resource (2 bytes) and address (4 bytes) from resmap
                ushort resource = BitConverter.ToUInt16(mapData, i); // Little-endian for 2 bytes
                uint address = BitConverter.ToUInt32(mapData, i + 2); // Little-endian for 4 bytes
                resourceMap.Add(Tuple.Create(resource, address));
            }

            return resourceMap;
        }
    }


}
