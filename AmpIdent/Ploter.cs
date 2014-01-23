using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AmpIdent
{
    class Ploter
    {
        //Fields
        //Private
        private MainViewModel _mainViewModel;
        private int _plottingResolution;
        private int _precentageOfPlotting;

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
            _mainViewModel = new MainViewModel();
            _plottingResolution = 1;
        }

        public void Plot(DenseMatrix vector, int layer)
        {
            for (int i = 0; i < vector.Values.Length - 1 / _plottingResolution; i++)
            {
                _mainViewModel.AddPoint(layer, new DataPoint(i, vector[i, 0]));
                i += _plottingResolution - 1;
            }

            _mainViewModel.Model.IsLegendVisible = false;
            _mainViewModel.Model.TitleFontSize = 0;
            _mainViewModel.Model.Title = "";
            _mainViewModel.Model.SubtitleFontSize = 0;

        }
    }
}
