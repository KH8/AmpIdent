using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel mainViewModel = new MainViewModel();
            string newLine = Environment.NewLine;

            ///*
            //first Czech example
            // parameters
            
            double k = 0.2; 
            double q = 1;
            double sigma2 = 0.5;

            Random random = new Random();
            
            // experiments

            var X = new DenseMatrix(10, 1, 0);
            var Y = new DenseMatrix(10, 1, 0);

            for (int i = 0; i <= 9; i++)
            {
                X[i, 0] = i;
                Y[i, 0] = k * X[i, 0] + q + sigma2 * random.Next(0, 10);
                mainViewModel.AddPoint(1, new DataPoint(X[i, 0], Y[i, 0]));
                mainViewModel.AddPoint(2, new DataPoint(X[i, 0], k * X[i, 0] + q));
            }
            ///*/

            DataContext = mainViewModel;
        }
    }
}
