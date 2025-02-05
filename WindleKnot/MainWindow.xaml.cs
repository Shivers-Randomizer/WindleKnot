using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindleKnot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            byte[] uncompressedData = new byte[]
                 {0x41, 0x42, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x43, 0x41, 0x42, 0x41, 0x42, 0x41, 0x42, 0x41};


            uint unCompressedDataLen = (uint)uncompressedData.Length;
            uint expectedSize = 16;
            uint outSize;

            LZS lzs = new LZS();

            byte[] CompressedData = lzs.Compress(uncompressedData, unCompressedDataLen, false, out outSize);

            uint compressedDataLen = (uint)CompressedData.Length;

            byte[] LZSdecompressedDataTest = lzs.Decompress(CompressedData, compressedDataLen, expectedSize, out outSize);
            // Expected result: 0x41 0x42 0x41 0x41 0x41 0x41 0x41 0x41 0x43 0x41 0x42 0x41 0x42 0x41 0x42 0x41
            // Result: Success
        }
    }
}

