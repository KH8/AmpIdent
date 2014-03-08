using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AmpIdent.Auxiliaries;
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

        private int _sampleLength;

        private int _na;
        private int _nb;
        private int _nd;
        private int _nk;

        private int _startPoint;
        private int _iterations;
        private int _estimationLength;
        private int _recurenceLength;

        private Boolean _1Loaded;
        private Boolean _2Loaded;
        private string _status;

        private readonly Thread _thread1;
        private readonly Thread _thread2;

        private int _command;

        private readonly LeastSquares _ls;
        private readonly RecursiveLeastSquares _rls;
        private readonly Armax _armax;
        private WavLoader _file1;
        private WavLoader _file2;
        private DenseMatrix _outputChannel;
        private Ploter _ploter;

        public MainWindow()
        {
            InitializeComponent();
            _ploter = new Ploter();

            //Loading.Content = "Loading: " + _loadingPercentage.ToString() + "%";

            _path1 = "c:\\avril_clean.wav";
            _path2 = "c:\\avril_scolpture.wav";
            _outputPath = "c:\\Output\\output.wav";

            _sampleLength = 0;

            _na = 5;
            _nb = 5;
            _nd = 1;
            _nk = 0;

            _startPoint = 200;
            _iterations = 1;
            _estimationLength = 250000;
            _recurenceLength = 5;
            
            _armax = new Armax();
            _rls = new RecursiveLeastSquares(_armax);
            _ls = new RecursiveLeastSquares(_armax);

            _command = 0;

            _thread1 = new Thread(Update);
            _thread2 = new Thread(Compute);

            _thread1.SetApartmentState(ApartmentState.STA);
            _thread1.IsBackground = false;
            _thread1.Start();
            _thread2.SetApartmentState(ApartmentState.STA);
            _thread2.IsBackground = false;
            _thread2.Start();

            _status = "Load files";
            
            /* //CudaMultiplicator Test
            int len1 = 1000;
            int len2 = 6;
            int len3 = 15000;

            var a = new DenseMatrix(len1, len2, 1.0);
            var b = new DenseMatrix(len2, len3, 1.0);

            var c = MultiplicatorCuda.Multiply(a, b);
            //*/

            DataContext = _ploter.MainViewModel;
        }

        private void Load1(object sender, RoutedEventArgs e)
        {
            _file1 = new WavLoader(_path1, _sampleLength);

            _ploter.PlottingResolution = _file1.SampleLength / 10000;
            _ploter.Clear();
            _ploter.Plot(_file1.RightChannel, 1);
            _ploter.Plot(_file1.LeftChannel, 2);
            _status = "File1 Loaded";

            _1Loaded = true;
        }

        private void Load2(object sender, RoutedEventArgs e)
        {
            _file2 = new WavLoader(_path2, _sampleLength);

            _ploter.PlottingResolution = _file2.SampleLength / 10000;
            _ploter.Clear();
            _ploter.Plot(_file2.RightChannel, 1);
            _ploter.Plot(_file2.LeftChannel, 2);
            _status = "File2 Loaded";

            _2Loaded = true;
        }

        private void Play1(object sender, RoutedEventArgs e)
        {
            Player.Play((Button)sender, _path1);
        }

        private void Play2(object sender, RoutedEventArgs e)
        {
            Player.Play((Button)sender, _path2);
        }

        private void Play3(object sender, RoutedEventArgs e)
        {
            Player.Play((Button)sender, _outputPath);
        }

        private void ComputeModel(object sender, RoutedEventArgs e)
        {
            RlsEnabledBox.Dispatcher.BeginInvoke((new Action(delegate
            {
                if (RlsEnabledBox.IsChecked == true & _1Loaded & _2Loaded)
                {
                    _command = 3;
                }
                else if (_1Loaded & _2Loaded)
                {
                    _command = 4;
                }
            })));
        }

        private void Output(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once CSharpWarnings::CS0618
            _outputChannel = new DenseMatrix(_file1.SampleLength, 1, 0.0);
            _armax.Model(_file1.LeftChannel, true);
            _outputChannel = _armax.Yk;
            _status = "Output computation: DONE!";

            _ploter.PlottingResolution = _outputChannel.Values.Length / 10000;
            _ploter.Clear();
            _ploter.Plot(_outputChannel, 4);
            DataContext = _ploter.MainViewModel;

            var wavOutput = _file1.Wav;

            var pos2 = 44;
            for (var i = 0; i < _file1.SampleLength - 10; i++)
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

        private void Compute()
        {
            while (_thread2.IsAlive)
            {
                switch (_command)
                {
                    case 1:
                        _command = 0;
                        break;

                    case 2:
                        _command = 0;
                        break;

                    case 3:

                        _rls.NumberOfIterations = _iterations;
                        _rls.ComputeRls(_file1.LeftChannel, _file2.LeftChannel, _estimationLength, _recurenceLength);

                        _ploter.PlottingResolution = _armax.Yk.Values.Length / 10000;
                        _ploter.Clear();
                        _ploter.Plot(_armax.Yk, 3);

                        _status = "Model computation: DONE!";
                        UpdateModel(_armax);

                        _command = 0;
                        break;

                    case 4:

                        _ls.NumberOfIterations = _iterations;
                        _ls.Compute(_file1.LeftChannel, _file2.LeftChannel, _estimationLength);

                        _ploter.PlottingResolution = _armax.Yk.Values.Length / 10000;
                        _ploter.Clear();
                        _ploter.Plot(_armax.Yk, 3);

                        _status = "Model computation: DONE!";
                        UpdateModel(_armax);

                        _command = 0;
                        break;
                }
                Thread.Sleep(10);
            }
        }

        private void Update()
        {
            while (_thread1.IsAlive)
            {
                //Display up-to-date
                Loading1.Dispatcher.BeginInvoke((new Action(delegate
                {
                    Loading1.Content = "File Loaded: " + "0" + "%";
                    if (_file1 != null)
                    {
                        Loading1.Content = "File Loaded: " + _file1.LoadingPercentage + "%";
                    }
                })));
                Loading2.Dispatcher.BeginInvoke((new Action(delegate
                {
                    Loading2.Content = "File Loaded: " + "0" + "%";
                    if (_file2 != null)
                    {
                        Loading2.Content = "File Loaded: " + _file2.LoadingPercentage + "%";
                    }
                })));
                OutputBox.Dispatcher.BeginInvoke((new Action(delegate
                {
                    if (_command == 3)
                    {
                        _status = _rls.StatusString;
                    }
                    else if (_command == 4)
                    {
                        _status = _ls.StatusString;
                    }
                    OutputBox.Text = _status;
                })));

                //Model parameters up-to-date
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

                VerifyValues();
                Thread.Sleep(1000);
            }
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
            try { _na = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _na = 0;
                pathBox.Text = "0"; 
            }
        }

        private void Nb_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _nb = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _nb = 0;
                pathBox.Text = "0";
            }
        }

        private void Nd_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _nd = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _nd = 0;
                pathBox.Text = "0";
            }
        }

        private void Nk_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _nk = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _nk = 0;
                pathBox.Text = "0";
            }
        }

        private void SpBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _startPoint = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _startPoint = 0;
                pathBox.Text = "0";
            }
        }

        private void NiBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _iterations = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _iterations = 0;
                pathBox.Text = "0";
            }
        }

        private void RlBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _recurenceLength = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _recurenceLength = 0;
                pathBox.Text = "0";
            }
        }

        private void EiBox_OnTextChanged_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _estimationLength = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _estimationLength = 0;
                pathBox.Text = "0";
            }
        }

        private void SampleLength_TextChange(object sender, TextChangedEventArgs e)
        {
            var pathBox = (TextBox)sender;
            try { _sampleLength = Convert.ToInt32(pathBox.Text); }
            catch (FormatException)
            {
                _sampleLength = 0;
                pathBox.Text = "0";
            }
            if (_estimationLength != 0)
            {
                _estimationLength = _sampleLength;
                ElBox.Dispatcher.BeginInvoke((new Action(delegate
                {
                    ElBox.Text = _estimationLength.ToString(CultureInfo.InvariantCulture);
                })));
            }
        }
        
        private void VerifyValues()
        {
            if (_startPoint < _na + _nb + _nd + _nk)
            {
                int sp = _na + _nb + _nd + _nk;

                _startPoint = sp;
                SpBox.Dispatcher.BeginInvoke((new Action(delegate { SpBox.Text = sp.ToString(CultureInfo.InvariantCulture); })));
            }
            if (_sampleLength < _estimationLength && _sampleLength != 0)
            {
                _estimationLength = _sampleLength;
                ElBox.Dispatcher.BeginInvoke((new Action(delegate { ElBox.Text = _estimationLength.ToString(CultureInfo.InvariantCulture); })));
            }
        }
    }
}
