using System;
using System.IO;
using System.Threading;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _path1;
        private string _path2;
        private int _loadingPercentage1;
        private int _loadingPercentage2;
        private Boolean _playing1;
        private Boolean _playing2;
        private Boolean _1Loaded;
        private Boolean _2Loaded;
        private string _status;

        private Boolean _compute;
        private SoundPlayer _sp;
        private Armax armax;
        private DenseMatrix _leftChannel1;
        private DenseMatrix _rightChannel1;
        private DenseMatrix _leftChannel2;
        private DenseMatrix _rightChannel2;
        private DenseMatrix _outputChannel;
        private Ploter _ploter;
        private byte[] _wav1;
        private byte[] _wav2;
        private int _samples;

        private Thread _thread1;
        private Thread _thread2;

        public MainWindow()
        {
            InitializeComponent();
            var newLine = Environment.NewLine;
            var random = new Random();
            _sp = new SoundPlayer();
            _ploter = new Ploter();

            //Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";
            
            _path1 = "";
            _path2 = "";
            _loadingPercentage1 = 0;
            _loadingPercentage2 = 0;
            armax = new Armax();

            _thread1 = new Thread(new ThreadStart(Update));
            _thread2 = new Thread(new ThreadStart(Compute));

            _thread1.SetApartmentState(ApartmentState.STA);
            _thread1.IsBackground = false;
            _thread1.Start();
            _thread2.SetApartmentState(ApartmentState.STA);
            _thread2.IsBackground = false;
            _thread2.Start();

            _status = "Load files";

            DataContext = _ploter.MainViewModel;
        }

        private void Load1(object sender, RoutedEventArgs e)
        {
            // first we need to read our wav file, so we can get our info:
            _wav1 = File.ReadAllBytes(_path1);

            // then we are going to get our file's info
            var numChannels = _wav1[22];
            var sampleRate = _wav1[24] + 256 * _wav1[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            _samples = (_wav1.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (numChannels == 2) _samples /= 2;

            var pos = 44; // start of data chunk
            _loadingPercentage1 = 0;

            _leftChannel1 = new DenseMatrix(_samples, 1, 0.0);
            _rightChannel1 = new DenseMatrix(_samples, 1, 0.0);

            for (var i = 0; i < _samples - 1; i++)
            {
                var number = _wav1[pos] + 256 * _wav1[pos + 1];
                if (number > 32767) number -= 65534;
                _leftChannel1[i, 0] = number;

                pos += 2 * numChannels;

                _loadingPercentage1 = i * 50 / _samples;
            }
            if (numChannels == 2)
            {
                pos = 44;
                _loadingPercentage1 = 50;
                for (var i = 0; i < _samples - 3; i++)
                {
                    var number = _wav1[pos + 2] + 256 * _wav1[pos + 3];
                    if (number > 32767) number -= 65534;
                    _rightChannel1[i, 0] = number;

                    pos += 2 * numChannels;

                    _loadingPercentage1 = 50 + (i * 50 / _samples);
                }
            }

            _1Loaded = true;

            _ploter.PlottingResolution = 100;
            _ploter.Clear();
            _ploter.Plot(_rightChannel1, 1);
            _ploter.Plot(_leftChannel1, 2);
            _status = "File1 Loaded";
        }

        private void Load2(object sender, RoutedEventArgs e)
        {
            // first we need to read our wav file, so we can get our info:
            _wav2 = File.ReadAllBytes(_path2);

            // then we are going to get our file's info
            var numChannels = _wav2[22];
            var sampleRate = _wav2[24] + 256 * _wav2[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            _samples = (_wav2.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (numChannels == 2) _samples /= 2;

            var pos = 44; // start of data chunk
            _loadingPercentage2 = 0;

            _leftChannel2 = new DenseMatrix(_samples, 1, 0.0);
            _rightChannel2 = new DenseMatrix(_samples, 1, 0.0);

            for (var i = 0; i < _samples - 1; i++)
            {
                var number = _wav2[pos] + 256 * _wav2[pos + 1];
                if (number > 32767) number -= 65534;
                _leftChannel2[i, 0] = number;

                pos += 2 * numChannels;

                _loadingPercentage2 = i * 50 / _samples;
            }
            if (numChannels == 2)
            {
                pos = 44;
                _loadingPercentage2 = 50;
                for (var i = 0; i < _samples - 3; i++)
                {
                    var number = _wav2[pos + 2] + 256 * _wav2[pos + 3];
                    if (number > 32767) number -= 65534;
                    _rightChannel2[i, 0] = number;

                    pos += 2 * numChannels;

                    _loadingPercentage2 = 50 + (i * 50 / _samples);
                }
            }

            _2Loaded = true;

            _ploter.PlottingResolution = 100;
            _ploter.Clear();
            _ploter.Plot(_rightChannel2, 1);
            _ploter.Plot(_leftChannel2, 2);
            _status = "File2 Loaded";

            if (!(Boolean)Graph.IsChecked && _1Loaded)
            {
                _compute = true;
            }
        }

        private void Play1(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

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
            var button = (Button)sender;

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
            var pathBox = (TextBox)sender;
            _path1 = pathBox.Text.ToString();
        }

        private void PathBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _path2 = pathBox.Text.ToString();
        }

        private void Update()
        {
            var i = 0;
            while (true)
            {
                Loading_1.Dispatcher.BeginInvoke((new Action(delegate()
                {
                    Loading_1.Content = _loadingPercentage1.ToString() + "%";
                })));
                Loading_2.Dispatcher.BeginInvoke((new Action(delegate()
                {
                    Loading_2.Content = _loadingPercentage2.ToString() + "%";
                })));
                OutputBox.Dispatcher.BeginInvoke((new Action(delegate()
                {
                    if (_compute)
                    {
                        if (i % 4 == 0) _status = "Model computation ";
                        if (i % 4 == 1) _status = "Model computation .";
                        if (i % 4 == 2) _status = "Model computation ..";
                        if (i % 4 == 3) _status = "Model computation ...";
                    }
                    i++;
                    OutputBox.Text = _status;
                })));
                Thread.Sleep(1000);
            }
        }

        private void Compute()
        {
            while (true)
            {
                if (_compute)
                {
                    armax.NAParameter = 10;
                    armax.NBParameter = 10;
                    armax.NDParameter = 1;
                    armax.NKParameter = 0;
                    armax.ModelShift = 1;
                    armax.StartingPoint = 200;

                    armax.NumberOfIterations = 1;
                    armax.Compute(_leftChannel1, _leftChannel2, 0);

                    _ploter.PlottingResolution = 100;
                    _ploter.Clear();
                    _ploter.Plot(armax.YK, 3);

                    _compute = false;

                    Console.WriteLine(@"Model computation: DONE!");
                    _status = "Model computation: DONE!";
                }
                Thread.Sleep(1000);
            }
        }

        private void Output(object sender, RoutedEventArgs e)
        {
            _outputChannel = new DenseMatrix(_samples, 1, 0.0);
            _outputChannel = armax.Model(_leftChannel1);
            Console.WriteLine(@"Output computation: DONE!");
            _status = "Output computation: DONE!";

            _ploter.PlottingResolution = 100;
            _ploter.Clear();
            _ploter.Plot(_outputChannel, 4);
            DataContext = _ploter.MainViewModel;

            var wavOutput = _wav1;

            var pos2 = 44;
            for (var i = 0; i < _samples - 10; i++)
            {
                var value1 = _outputChannel[i, 0];
                var value2 = (int)value1;
                var value = BitConverter.GetBytes(value2);

                wavOutput[pos2] = value[0];
                wavOutput[pos2 + 1] = value[1];
                wavOutput[pos2 + 2] = value[0];
                wavOutput[pos2 + 3] = value[1];

                pos2 += 4;
            }
            File.WriteAllBytes("C:\\Output\\output.wav", wavOutput);
            Console.WriteLine(@"File creation: DONE!");
            _status = "File creation: DONE!";
        }
    }
}
