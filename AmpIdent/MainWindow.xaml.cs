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
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel();

            /*
            _path = "";
            _loadingPercentage = 0;

            MainViewModel mainViewModel = new MainViewModel();
            string newLine = Environment.NewLine;
            Random random = new Random();

            Thread _thread = new Thread(new ThreadStart(Update));
            _thread.IsBackground = false;
            _thread.Start();
            //*/

            ///*
            //TEST MODEL
            var X1 = new DenseMatrix(500, 1, 0.0);
            var Y1 = new DenseMatrix(500, 1, 0.0);

            for (int i = 0; i <= 499; i++)
            {
                X1[i, 0] = -54 * Math.Sin(i / 10);// +random.Next(-10, 100);
                //X1[i, 0] = random.Next(-10, 100);
                //mainViewModel.AddPoint(1, new DataPoint(i, X1[i, 0]));

                if (i <= 10)
                {
                    Y1[i, 0] = X1[i, 0];
                    //mainViewModel.AddPoint(2, new DataPoint(i, Y1[i, 0]));
                }

                if (i > 10)
                {
                    Y1[i, 0] = 30 * Math.Cos(X1[i -10, 0]) - X1[i, 0];
                    //mainViewModel.AddPoint(2, new DataPoint(i, Y1[i, 0]));
                }
            }

            ARMAX modeller = new ARMAX();
            modeller.Compute(X1, Y1, 0);

            for (int i = 0; i <= modeller.YK.Values.Length - 1; i++)
            {
                //mainViewModel.AddPoint(3, new DataPoint(i, modeller.YK[i,0]));
            }


            ///*
            for (int i = 0; i <= 499; i++)
            {
                X1[i, 0] = 100*(i-3)*(i-400)*(i-230);
                mainViewModel.AddPoint(4, new DataPoint(i, X1[i, 0]));

                if (i <= 10)
                {
                    Y1[i, 0] = X1[i, 0];
                    mainViewModel.AddPoint(5, new DataPoint(i, Y1[i, 0]));
                }

                if (i > 10)
                {
                    Y1[i, 0] = 30 * Math.Sin(X1[i - 10, 0]) - X1[i, 0];
                    mainViewModel.AddPoint(5, new DataPoint(i, Y1[i, 0]));
                }
            }
            for (int i = 0; i <= 400 + modeller.StartingPoint - 1; i++)
            {
                var Fi_k_t = modeller.CalculateFi_k(modeller.NAParameter, modeller.NBParameter, modeller.NDParameter, modeller.NKParameter, i + modeller.StartingPoint, X1, Y1, modeller.V0);

                var Theta_k_Y_1 = modeller.Theta.Transpose() * Fi_k_t;
                mainViewModel.AddPoint(6, new DataPoint(i + modeller.StartingPoint, Theta_k_Y_1[0, 0]));
            }

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

            for (int i = 0; i < samples; i++)
            {
                int number = wav[pos] + 256 * wav[pos + 1];
                if (number > 32767) number -= 65534;
                mainViewModel.AddPoint(1, new DataPoint(i, number));

                _loadingPercentage = i * 100 / samples;
                pos += 2 * NumChannnels;
                Console.WriteLine(_loadingPercentage.ToString());
            }

            DataContext = mainViewModel;
        }

        private void PathBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            TextBox _pathBox = (TextBox)sender;
            _path = _pathBox.Text.ToString();
        }

        private void Update()
        {
        }
    }
}
