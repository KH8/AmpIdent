using System;
using System.Threading;
using MathNet.Numerics.LinearAlgebra.Double;
using OxyPlot;

namespace AmpIdent.Visual
{
    class Ploter
    {
        //Fields
        //Private
        private readonly MainViewModel _mainViewModel;
        private int _plottingResolution;
        private readonly int _precentageOfPlotting;
        private Thread _thread;
        private Boolean _plotCmd;

        //Public
        public MainViewModel MainViewModel { get { return _mainViewModel; } }
        public int PlottingResolution
        {
            get { return _plottingResolution; }
            set { _plottingResolution = value; }
        }
        public int PercentageOfPlotting { get { return _precentageOfPlotting; } }
        
        //Methods
        //Constructor
        public Ploter()
        {
            _precentageOfPlotting = 0;
            _mainViewModel = new MainViewModel();
            _plottingResolution = 1;

            _mainViewModel.AddPoint(9, new DataPoint(0, 0));

            _mainViewModel.Model.IsLegendVisible = false;
            _mainViewModel.Model.TitleFontSize = 0;
            _mainViewModel.Model.Title = "";
            _mainViewModel.Model.SubtitleFontSize = 0;

            _thread = new Thread(Plotting);

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = false;
            _thread.Start();
        }

        private void Plotting()
        {
            while (_thread.IsAlive)
            {
                if (_plotCmd)
                {
                    if (_plottingResolution < 1) _plottingResolution = 1;
                    for (var i = 0; i < vector.Values.Length - 1 / _plottingResolution; i++)
                    {
                        _mainViewModel.AddPoint(layer, new DataPoint(i, vector[i, 0]));
                        i += _plottingResolution - 1;
                    }
                    _mainViewModel.Model.RefreshPlot(true);
                    _plotCmd = false;
                }
            }
        }

        public void Plot(DenseMatrix vector, int layer)
        {
            _
        }

        public void Clear()
        {
            _mainViewModel.Clear();
        }
    }
}
