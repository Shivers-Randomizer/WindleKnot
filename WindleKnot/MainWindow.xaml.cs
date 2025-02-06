using System;
using System.IO;
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
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;




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

        List<Tuple<ushort, uint>> ViewMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> pictureMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> scriptMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> vocabMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> fontMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> patchMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> paletteMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> messageMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> mapMap = new List<Tuple<ushort, uint>>();
        List<Tuple<ushort, uint>> heapMap = new List<Tuple<ushort, uint>>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string RESMAPFilePath;
            string RESSCIFilePath;
            byte[] mapData = null;
            byte[] resourceData = null;

            //Get filepath to Shivers Files
            if (ShiversFilePaths(out RESMAPFilePath, out RESSCIFilePath))
            {
                // Read in data
                mapData = File.ReadAllBytes(RESMAPFilePath);
                resourceData = File.ReadAllBytes(RESSCIFilePath);

                // Parse Resource Map
                ResourceMap resourceMap = new ResourceMap();
                resourceMap.MapResources(mapData);

                Picture picture = new Picture();

                //Change third parameter to what ever image index you want.
                //CanvasPicture.Source = picture.DrawPicture(resourceData, resourceMap, 22);
                CanvasPicture.Source = picture.DrawPicture(resourceData, resourceMap, 20);
            }
        }

        


        private bool ShiversFilePaths(out string RESMAP, out string RESSCI)
        {

            //Show dialog to select filepath for Shivers files
            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowDialog();

            string folderPath = folderDialog.SelectedPath;

            RESMAP = System.IO.Path.Combine(folderPath, "RESMAP.000");
            RESSCI = System.IO.Path.Combine(folderPath, "RESSCI.000");

            // Check if both files exist
            if (!File.Exists(RESMAP) || !File.Exists(RESSCI))
            {
                string missingFiles = "";
                if (!File.Exists(RESMAP)) missingFiles += "RESMAP.000\n";
                if (!File.Exists(RESSCI)) missingFiles += "RESSCI.000\n";

                System.Windows.MessageBox.Show(
                    "The following required file(s) are missing:\n" + missingFiles,
                    "Missing Files",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }


    }
}

