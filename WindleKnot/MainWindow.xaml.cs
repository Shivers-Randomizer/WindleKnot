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
            byte[] compressedData = new byte[]
            {
                 0x20, 0x90, 0x88, 0x38, 0x1C, 0x21, 0xE2, 0x5C, 0x15, 0x80
            };

            uint dataLen = (uint)compressedData.Length;
            uint expectedSize = 16;
            uint outSize;

            LZS lzs = new LZS();

            byte[] decompressedData = lzs.Decompress(compressedData, dataLen, expectedSize, out outSize);
            // Expected result: 0x41 0x42 0x41 0x41 0x41 0x41 0x41 0x41 0x43 0x41 0x42 0x41 0x42 0x41 0x42 0x41
            // Result: Success
        }
    }
}

