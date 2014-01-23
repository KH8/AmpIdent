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
        private Boolean _compute;
        private SoundPlayer _sp;
        private ARMAX armax;
        private DenseMatrix _leftChannel;
        private DenseMatrix _rightChannel;
        private Ploter _ploter;


        public MainWindow()
        {
            InitializeComponent();
            string newLine = Environment.NewLine;
            Random random = new Random();
            _sp = new SoundPlayer();
            _ploter = new Ploter();

            //Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";
            
            _path = "";
            _loadingPercentage = 0;
            armax = new ARMAX();

            Thread _thread1 = new Thread(new ThreadStart(Update));
            Thread _thread2 = new Thread(new ThreadStart(Compute));

            _thread1.SetApartmentState(ApartmentState.STA);
            _thread1.IsBackground = false;
            _thread1.Start();
            _thread2.SetApartmentState(ApartmentState.STA);
            _thread2.IsBackground = false;
            _thread2.Start();

            DataContext = _ploter.MainViewModel;
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

            _leftChannel = new DenseMatrix(samples, 1, 0.0);
            _rightChannel = new DenseMatrix(samples, 1, 0.0);

            for (int i = 0; i < samples - 1; i++)
            {
                int number = wav[pos] + 256 * wav[pos + 1];
                if (number > 32767) number -= 65534;
                _leftChannel[i, 0] = number;

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
                    _rightChannel[i, 0] = number;

                    pos += 2 * NumChannels;

                    _loadingPercentage = i * 100 / samples;
                }
            }

            _compute = true;
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
                System.Threading.Thread.Sleep(5);
            }
        }

        private void Compute()
        {
            while (true)
            {
                if (_compute)
                {
                    armax.NumberOfIterations = 1;
                    armax.Compute(_leftChannel, _rightChannel, 0);
                    _ploter.PlottingResolution = 100;
                    _ploter.Plot(_leftChannel, 1);
                    _ploter.Plot(_rightChannel, 2);
                    _ploter.Plot(armax.YK, 3);

                    _compute = false;
                }
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
