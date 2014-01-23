using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Media;
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
        private Boolean _playing;
        private SoundPlayer _sp;
        private ARMAX armax;

        public MainWindow()
        {
            InitializeComponent();
            string newLine = Environment.NewLine;
            Random random = new Random();
            _sp = new SoundPlayer();

            //Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";
            
            _path = "";
            _loadingPercentage = 0;
            armax = new ARMAX();

            Thread _thread = new Thread(new ThreadStart(Update));

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = false;
            _thread.Start();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            // first we need to read our wav file, so we can get our info:
            byte[] wav = File.ReadAllBytes(_path);

            // then we are going to get our file's info
            var NumChannels = wav[22];
            var SampleRate = wav[24] + 256 * wav[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            int samples = (wav.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (NumChannels == 2) samples /= 2;

            int pos = 44; // start of data chunk
            _loadingPercentage = 0;

            DenseMatrix leftChannel = new DenseMatrix(samples, 1, 0.0);
            DenseMatrix rightChannel = new DenseMatrix(samples, 1, 0.0);

            for (int i = 0; i < samples - 1; i++)
            {
                int number = wav[pos] + 256 * wav[pos + 1];
                if (number > 32767) number -= 65534;
                leftChannel[i, 0] = number;

                pos += 2 * NumChannels;

                _loadingPercentage = i * 100 / samples;
            }
            if (NumChannels == 2)
            {
                pos = 44;
                for (int i = 0; i < samples - 3; i++)
                {
                    int number = wav[pos + 2] + 256 * wav[pos + 3];
                    if (number > 32767) number -= 65534;
                    rightChannel[i, 0] = number;

                    pos += 2 * NumChannels;

                    _loadingPercentage = i * 100 / samples;
                }
            }


            Ploter ploter = new Ploter();
            ploter.PlottingResolution = 100;
            ploter.Plot(leftChannel, 1);
            ploter.Plot(rightChannel, 2);

            armax.NumberOfIterations = 1;
            armax.Compute(leftChannel, rightChannel, 200000);
            ploter.Plot(armax.YK, 3);

            DataContext = ploter.MainViewModel;
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (!_playing)
            {
                _sp.Stop();
                _sp.SoundLocation = _path;
                _sp.LoadAsync();
                _sp.Play();
                _playing = true;
                button.Content = "Stop";
            }
            else
            {
                _sp.Stop();
                _playing = false;
                button.Content = "Play";
            }
        }

        private void PathBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            TextBox _pathBox = (TextBox)sender;
            _path = _pathBox.Text.ToString();
        }

        private void Update()
        {
            while (true)
            {
                Loading.Dispatcher.BeginInvoke((new Action(delegate()
                {
                    Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";
                })));
                Console.WriteLine(armax.EstimationPercentage.ToString());
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
