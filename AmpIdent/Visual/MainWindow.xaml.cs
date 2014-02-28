using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using AmpIdent.Estimation;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Visual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _path1;
        private string _path2;
        private string _outputPath;

        private int _na;
        private int _nb;
        private int _nd;
        private int _nk;

        private int _startPoint;
        private int _iterations;
        private int _estimationLength;
        private int _recurenceLength;

        private int _loadingPercentage1;
        private int _loadingPercentage2;
        private Boolean _playing1;
        private Boolean _playing2;
        private Boolean _1Loaded;
        private string _status;

        private readonly Thread _thread1;
        private readonly Thread _thread2;

        private Boolean _compute;
        private readonly SoundPlayer _sp;
        private readonly RecursiveLeastSquares _rls;
        private readonly Armax _armax;
        private DenseMatrix _leftChannel1;
        private DenseMatrix _rightChannel1;
        private DenseMatrix _leftChannel2;
        private DenseMatrix _rightChannel2;
        private DenseMatrix _outputChannel;
        private readonly Ploter _ploter;
        private byte[] _wav1;
        private byte[] _wav2;
        private int _samples;

        public MainWindow()
        {
            InitializeComponent();
            _sp = new SoundPlayer();
            _ploter = new Ploter();

            //Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";

            _path1 = "c:\\avril_clean.wav";
            _path2 = "c:\\avril_scolpture.wav";
            _outputPath = "c:\\Output\\output.wav";

            _na = 5;
            _nb = 5;
            _nd = 1;
            _nk = 0;

            _startPoint = 200;
            _iterations = 1;
            _estimationLength = 250000;
            _recurenceLength = 5;

            _loadingPercentage1 = 0;
            _loadingPercentage2 = 0;
            
            _armax = new Armax();
            _rls = new RecursiveLeastSquares(_armax);

            _thread1 = new Thread(Update);
            _thread2 = new Thread(Compute);

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
            //optional
            //var sampleRate = _wav1[24] + 256 * _wav1[25];

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

            _loadingPercentage1 = 100;
            _1Loaded = true;

            _ploter.PlottingResolution = _samples / 10000;
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
            //optional
            //var sampleRate = _wav2[24] + 256 * _wav2[25];

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

            _loadingPercentage2 = 100;

            _ploter.PlottingResolution = _samples / 10000;
            _ploter.Clear();
            _ploter.Plot(_rightChannel2, 1);
            _ploter.Plot(_leftChannel2, 2);
            _status = "File2 Loaded";

            if (Graph.IsChecked == false && _1Loaded)
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

        private void Update()
        {
            while (_thread1.IsAlive)
            {
                Loading1.Dispatcher.BeginInvoke((new Action(delegate {
                    Loading1.Content = "File Loaded: " +_loadingPercentage1 + "%";
                })));
                Loading2.Dispatcher.BeginInvoke((new Action(delegate {
                    Loading2.Content = "File Loaded: " + _loadingPercentage2 + "%";
                })));
                OutputBox.Dispatcher.BeginInvoke((new Action(delegate
                {
                    if (_compute)
                    {
                        _status = _rls.StatusString;
                    }
                    OutputBox.Text = _status;
                })));
                Thread.Sleep(1000);
            }
        }

        private void Compute()
        {
            while (_thread2.IsAlive)
            {
                if (_compute)
                {
                    _armax.NaParameter = _na;
                    _armax.NbParameter = _nb;
                    _armax.NdParameter = _nd;
                    _armax.NkParameter = _nk;
                    _armax.StartingPoint = _startPoint;

                    _armax.ModelShift = 0;
                    ModelShifted.Dispatcher.BeginInvoke((new Action(delegate
                    {
                        if (ModelShifted.IsChecked == true)
                        {
                            _armax.ModelShift = 1;
                        }
                    })));

                    _rls.NumberOfIterations = _iterations;
                    _rls.ComputeRls(_leftChannel1, _leftChannel2, _estimationLength, _recurenceLength);

                    _ploter.PlottingResolution = 100;
                    _ploter.Clear();
                    _ploter.Plot(_armax.Yk, 3);

                    _compute = false;
                    _status = "Model computation: DONE!";
                    UpdateModel(_armax);
                }
                Thread.Sleep(1000);
            }
        }

        private void Output(object sender, RoutedEventArgs e)
        {
            _outputChannel = new DenseMatrix(_samples, 1, 0.0);
            _outputChannel = _armax.Model(_leftChannel1);
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
            File.WriteAllBytes(_outputPath, wavOutput);
            _status = "File creation: DONE!";
        }

        public void UpdateModel(Armax modelArmax)
        {
            ModeListBox.Dispatcher.BeginInvoke((new Action(delegate
            {
                ModeListBox.Items.Clear();
                ModeListBox.Items.Add("\nNA parameters: ");
                for (int i = 0; i < modelArmax.NaParameter; i++)
                {
                    ModeListBox.Items.Add("\nNA parameter " + i + ": \t" + modelArmax.Theta[i,0]);
                }
                ModeListBox.Items.Add("\nNB parameters: ");
                for (int i = 0; i < modelArmax.NbParameter; i++)
                {
                    ModeListBox.Items.Add("\nNB parameter " + i + ": \t" + modelArmax.Theta[modelArmax.NaParameter + i, 0]);
                }
                ModeListBox.Items.Add("\nNC parameters: ");
                for (int i = 0; i < modelArmax.NdParameter; i++)
                {
                    ModeListBox.Items.Add("\nND parameter " + i + ": \t" + modelArmax.Theta[modelArmax.NaParameter + modelArmax.NbParameter + i, 0]);
                }
            })));
        }

        private void ClosingHanler(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PathBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _path1 = pathBox.Text;
        }

        private void PathBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _path2 = pathBox.Text;
        }
        private void OutputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _outputPath = pathBox.Text;
        }

        private void Na_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _na = Convert.ToInt32(pathBox.Text);
        }
        private void Nb_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _nb = Convert.ToInt32(pathBox.Text);
        }
        private void Nd_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _nd = Convert.ToInt32(pathBox.Text);
        }
        private void Nk_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _nk = Convert.ToInt32(pathBox.Text);
        }

        private void SpBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _startPoint = Convert.ToInt32(pathBox.Text);
        }

        private void NiBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _iterations = Convert.ToInt32(pathBox.Text);
        }

        private void RlBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _recurenceLength = Convert.ToInt32(pathBox.Text);
        }

        private void EiBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            _estimationLength = Convert.ToInt32(pathBox.Text);
        }
    }
}
