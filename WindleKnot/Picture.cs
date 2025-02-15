using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WindleKnot
{
    internal class Picture
    {
        public Picture()
        {

        }

        LZS lzs = new LZS();

        public BitmapImage DrawPicture(byte[] resourceData, ResourceMap resourceMap, int imageIndex)
        {
            uint compressedSize = BitConverter.ToUInt32(resourceData, (int)resourceMap.pictureMap[imageIndex].Item2 + 3);
            uint uncompressedSize = BitConverter.ToUInt32(resourceData, (int)resourceMap.pictureMap[imageIndex].Item2 + 7);
            uint outSize;
            byte[] CompressedData = new byte[compressedSize];
            Array.Copy(resourceData, resourceMap.pictureMap[imageIndex].Item2 + 0x0D, CompressedData, 0, compressedSize);

            byte[] uncompressedData = lzs.Decompress(CompressedData, compressedSize, uncompressedSize, out outSize);

            (byte[][] matrixData, List<(byte A, byte R, byte G, byte B)> palette) = GetImageDataWithPalette(uncompressedData);

            return ConstructBitmapImage(matrixData, palette);
        }

        public static BitmapImage ConstructBitmapImage(byte[][] matrixData, List<(byte A, byte R, byte G, byte B)> palette)
        {
            int height = matrixData.Length;
            int width = matrixData[0].Length;

            // Create a new bitmap
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        // Get palette index
                        byte paletteIndex = matrixData[row][col];

                        // Get the RGB color from the palette
                        var (a, r, g, b) = palette[paletteIndex];


                        // Set the pixel color
                        bitmap.SetPixel(col, row, System.Drawing.Color.FromArgb(r, g, b));
                    }
                }

                BitmapImage bitmapImage = ConvertBitmapToBitmapImage(bitmap);

                return bitmapImage;
            }
        }

        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the System.Drawing.Bitmap to the memory stream in PNG format
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                // Create BitmapImage from MemoryStream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;  // Use StreamSource for the image data
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }



        static (byte[][], List<(byte A, byte R, byte G, byte B)>) GetImageDataWithPalette(byte[] data)
        {
            // Constants and header parsing
            int HEADER_LENGTHS = 0x44;
            int PALETTE_OFFSET = 0x63;
            int PALETTE_ITEM_SIZE = 3;

            int offset = BitConverter.ToInt32(data, 0x3A); // Little-endian offset
            int width = BitConverter.ToInt16(data, 0x0E); // Image width
            int height = BitConverter.ToInt16(data, 0x10); // Image height

            //Determine if using RGB or ARGB Format
            if (data[0x5E] == 0)
            {
                PALETTE_ITEM_SIZE = 4;
                PALETTE_OFFSET = 0x64;
            }

            // Extract palette data
            var palette = new List<(byte A, byte R, byte G, byte B)>();
            for (int i = PALETTE_OFFSET; i < offset + HEADER_LENGTHS; i += PALETTE_ITEM_SIZE)
            {
                byte a = 1;

                //Check if alpha ARGB format
                if (PALETTE_ITEM_SIZE == 4)
                {
                    a = data[i - 1];
                }

                byte r = data[i];
                byte g = data[i + 1];
                byte b = data[i + 2];
                palette.Add((a, r, g, b));
            }

            // Extract the image data
            int start = offset + HEADER_LENGTHS;
            byte[] img = new byte[data.Length - start];
            Array.Copy(data, start, img, 0, img.Length);

            byte[][] matrix = FlattenToMatrix(img, width);
            // Return as matrix or flat array
            return (matrix, palette);
        }

        private static byte[][] FlattenToMatrix(byte[] img, int width)
        {
            // Calculate the number of rows
            int rows = img.Length / width;
            byte[][] matrix = new byte[rows][];

            // Fill the matrix row by row
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new byte[width];
                Array.Copy(img, i * width, matrix[i], 0, width);
            }

            return matrix;
        }

    }
}
