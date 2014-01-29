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
        private string _path1;
        private string _path2;
        private int _loadingPercentage;
        private Boolean _playing1;
        private Boolean _playing2;
        private Boolean _1loaded;
        private Boolean _2loaded;

        private Boolean _compute;
        private SoundPlayer _sp;
        private ARMAX armax;
        private DenseMatrix _leftChannel1;
        private DenseMatrix _rightChannel1;
        private DenseMatrix _leftChannel2;
        private DenseMatrix _rightChannel2;
        private DenseMatrix _outputChannel;
        private Ploter _ploter;
        private byte[] _wav1;
        private byte[] _wav2;
        private int _samples;


        public MainWindow()
        {
            InitializeComponent();
            string newLine = Environment.NewLine;
            Random random = new Random();
            _sp = new SoundPlayer();
            _ploter = new Ploter();

            //Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";
            
            _path1 = "";
            _path2 = "";
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

        private void Load1(object sender, RoutedEventArgs e)
        {
            // first we need to read our wav file, so we can get our info:
            _wav1 = File.ReadAllBytes(_path1);

            // then we are going to get our file's info
            var NumChannels = _wav1[22];
            var SampleRate = _wav1[24] + 256 * _wav1[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            _samples = (_wav1.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (NumChannels == 2) _samples /= 2;

            int pos = 44; // start of data chunk
            _loadingPercentage = 0;

            _leftChannel1 = new DenseMatrix(_samples, 1, 0.0);
            _rightChannel1 = new DenseMatrix(_samples, 1, 0.0);

            for (int i = 0; i < _samples - 1; i++)
            {
                int number = _wav1[pos] + 256 * _wav1[pos + 1];
                if (number > 32767) number -= 65534;
                _leftChannel1[i, 0] = number;

                pos += 2 * NumChannels;

                _loadingPercentage = i * 100 / _samples;
            }
            if (NumChannels == 2)
            {
                pos = 44;
                for (int i = 0; i < _samples - 3; i++)
                {
                    int number = _wav1[pos + 2] + 256 * _wav1[pos + 3];
                    if (number > 32767) number -= 65534;
                    _rightChannel1[i, 0] = number;

                    pos += 2 * NumChannels;

                    _loadingPercentage = i * 100 / _samples;
                }
            }

            _1loaded = true;

            _ploter.PlottingResolution = 100;
            _ploter.Clear();
            _ploter.Plot(_rightChannel1, 1);
            _ploter.Plot(_leftChannel1, 2);
        }

        private void Load2(object sender, RoutedEventArgs e)
        {
            // first we need to read our wav file, so we can get our info:
            _wav2 = File.ReadAllBytes(_path2);

            // then we are going to get our file's info
            var NumChannels = _wav2[22];
            var SampleRate = _wav2[24] + 256 * _wav2[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            _samples = (_wav2.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (NumChannels == 2) _samples /= 2;

            int pos = 44; // start of data chunk
            _loadingPercentage = 0;

            _leftChannel2 = new DenseMatrix(_samples, 1, 0.0);
            _rightChannel2 = new DenseMatrix(_samples, 1, 0.0);

            for (int i = 0; i < _samples - 1; i++)
            {
                int number = _wav2[pos] + 256 * _wav2[pos + 1];
                if (number > 32767) number -= 65534;
                _leftChannel2[i, 0] = number;

                pos += 2 * NumChannels;

                _loadingPercentage = i * 100 / _samples;
            }
            if (NumChannels == 2)
            {
                pos = 44;
                for (int i = 0; i < _samples - 3; i++)
                {
                    int number = _wav2[pos + 2] + 256 * _wav2[pos + 3];
                    if (number > 32767) number -= 65534;
                    _rightChannel2[i, 0] = number;

                    pos += 2 * NumChannels;

                    _loadingPercentage = i * 100 / _samples;
                }
            }

            _2loaded = true;

            _ploter.PlottingResolution = 100;
            _ploter.Clear();
            _ploter.Plot(_rightChannel2, 1);
            _ploter.Plot(_leftChannel2, 2);

            if (!(Boolean)Graph.IsChecked && _1loaded)
            {
                _compute = true;
            }
        }

        private void Play1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (!_playing1)
            {
                _sp.Stop();
                _sp.SoundLocation = _path1;
                _sp.LoadAsync();
                _sp.Play();
                _playing1 = true;
                button.Content = "Stop";
            }
            else
            {
                _sp.Stop();
                _playing1 = false;
                button.Content = "Play";
            }
        }

        private void Play2(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (!_playing2)
            {
                _sp.Stop();
                _sp.SoundLocation = _path2;
                _sp.LoadAsync();
                _sp.Play();
                _playing2 = true;
                button.Content = "Stop";
            }
            else
            {
                _sp.Stop();
                _playing2 = false;
                button.Content = "Play";
            }
        }

        private void PathBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            TextBox _pathBox = (TextBox)sender;
            _path1 = _pathBox.Text.ToString();
        }

        private void PathBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            TextBox _pathBox = (TextBox)sender;
            _path2 = _pathBox.Text.ToString();
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
                    armax.NAParameter = 50;
                    armax.NBParameter = 100;
                    armax.NDParameter = 10;
                    armax.NKParameter = 0;
                    armax.ModelShift = 1;
                    armax.StartingPoint = 100;

                    armax.NumberOfIterations = 1;
                    armax.Compute(_leftChannel1, _leftChannel2, 30000);

                    _ploter.PlottingResolution = 100;
                    _ploter.Clear();
                    _ploter.Plot(armax.YK, 3);

                    _compute = false;

                    Console.WriteLine("Model computation: DONE!");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void Output(object sender, RoutedEventArgs e)
        {
            _outputChannel = new DenseMatrix(_samples, 1, 0.0);
            _outputChannel = armax.Model(_leftChannel1);
            Console.WriteLine("Output computation: DONE!");

            _ploter.PlottingResolution = 100;
            _ploter.Clear();
            _ploter.Plot(_outputChannel, 4);
            DataContext = _ploter.MainViewModel;

            byte[] wavOutput = _wav1;

            int pos2 = 44;
            for (int i = 0; i < _samples - 10; i++)
            {
                double value1 = _outputChannel[i, 0];
                int value2 = (int)value1;
                byte[] value = BitConverter.GetBytes(value2);

                wavOutput[pos2] = value[0];
                wavOutput[pos2 + 1] = value[1];
                wavOutput[pos2 + 2] = value[0];
                wavOutput[pos2 + 3] = value[1];

                pos2 += 4;
            }

            File.WriteAllBytes("C:\\Output\\output.wav", wavOutput);
            Console.WriteLine("File creation: DONE!");

        }
    }
}
