using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Reporting;

namespace AmpIdent.ViewModels
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private PlotModel plotModel;
        public PlotModel MyPlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("MyPlotModel"); }
        }

        public MainWindowModel()
        {
            var temp = new PlotModel("Square wave");
            var ls = new LineSeries("sin(x)+sin(3x)/3+sin(5x)/5+...");
            int n = 10;
            for (double x = -10; x < 10; x += 0.0001)
            {
                double y = 0;
                for (int i = 0; i < n; i++)
                {
                    int j = i * 2 + 1;
                    y += Math.Sin(j * x) / j;
                }
                ls.Points.Add(new DataPoint(x, y));
            }
            temp.Series.Add(ls);
            temp.Axes.Add(new LinearAxis(AxisPosition.Left, -4, 4));
            temp.Axes.Add(new LinearAxis(AxisPosition.Bottom));
            plotModel = temp; 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
