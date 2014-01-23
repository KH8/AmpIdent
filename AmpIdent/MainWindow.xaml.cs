using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra.Double;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AmpIdent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _path;
        private int _loadingPercentage;

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel();
            string newLine = Environment.NewLine;
            Random random = new Random();
            
            _path = "";
            _loadingPercentage = 0;

            DataContext = mainViewModel;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = new MainViewModel();

            // first we need to read our wav file, so we can get our info:
            byte[] wav = File.ReadAllBytes(_path);

            // then we are going to get our file's info
            var NumChannnels = wav[22];
            var SampleRate = wav[24] + 256 * wav[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            int samples = (wav.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (NumChannnels == 2) samples /= 2;

            int pos = 44; // start of data chunk
            _loadingPercentage = 0;

            for (int i = 0; i < samples / 1000; i++)
            {
                int number = wav[pos] + 256 * wav[pos + 1];
                if (number > 32767) number -= 65534;
                mainViewModel.AddPoint(1, new DataPoint(i, number));

                _loadingPercentage = i * 100 / samples;
                pos += 2000 * NumChannnels;
                Console.WriteLine(_loadingPercentage.ToString());
            }

            DataContext = mainViewModel;
        }

        private void PathBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            TextBox _pathBox = (TextBox)sender;
            _path = _pathBox.Text.ToString();
        }
    }
}
