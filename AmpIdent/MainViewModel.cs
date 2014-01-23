// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OxyPlot">
//   http://oxyplot.codeplex.com, license: Ms-PL
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OxyPlot;
using OxyPlot.Series;

namespace AmpIdent
{
    public class MainViewModel : Observable
    {
        private PlotModel model;
        LineSeries[] series;
        PlotModel tmp;

        public MainViewModel()
        {
            // Create the plot model
            tmp = new PlotModel("Simple example", "using OxyPlot");
            // Create two line series (markers are hidden by default)

            series = new LineSeries[10];

            for (int i = 1; i <= 9; i++)
            {
                series[i] = new LineSeries("Series " +i.ToString()) { MarkerType = MarkerType.Circle };
                series[i].MarkerSize = 1;

                // Add the series to the plot model
                tmp.Series.Add(series[i]);
            }
        }

        public PlotModel Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                    RaisePropertyChanged(() => Model);
                }
            }
        }

        public void AddPoint(int i, DataPoint dataPoint)
        {
            series[i].Points.Add(dataPoint);

            // Axes are created automatically if they are not defined
            // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
            Model = tmp;
        }
    }
}