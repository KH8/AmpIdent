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

            
            //first Czech example
            // parameters
            
            double k = 0.2; 
            double q = 1;
            double sigma2 = 5;

            Random random = new Random();
            
            // experiments

            var X = new DenseMatrix(10, 1, 0.0);
            var Y = new DenseMatrix(10, 1, 0.0);

            for (int j = 1; j <= 10; j++)
            {
                int i = j - 1;
                X[i, 0] = j;
                Y[i, 0] = k * X[i, 0] + q + random.Next(-5,5)/10.0;
                mainViewModel.AddPoint(1, new DataPoint(X[i, 0], Y[i, 0]));
                mainViewModel.AddPoint(2, new DataPoint(X[i, 0], k * X[i, 0] + q));
            }
            Console.WriteLine(X.ToString());
            Console.WriteLine(Y.ToString());

            var Z = new DenseMatrix(10, 2, 1.0);
            for (int i = 0; i <= 9; i++)
            {
                Z[i, 0] = X[i, 0];
            }
            Console.WriteLine(Z.ToString());

            var ZT = Z.Transpose();
            Console.WriteLine(ZT.ToString());

            var ZTZ = ZT * Z;
            Console.WriteLine(ZTZ.ToString());

            var determinant = ZTZ.Determinant();
            Console.WriteLine(newLine + "Determinant: " +determinant.ToString() +newLine);

            var Theta = ZTZ.Inverse() * ZT * Y;
            Console.WriteLine(Theta.ToString());

            for (int j = 1; j <= 10; j++)
            {
                int i = j - 1;
                X[i, 0] = j;
                Y[i, 0] = Theta[0, 0] * X[i, 0] + Theta[1, 0];
                mainViewModel.AddPoint(3, new DataPoint(X[i, 0], Y[i, 0]));
            }
            Console.WriteLine(X.ToString());
            Console.WriteLine(Y.ToString());

            DataContext = mainViewModel;
        }
    }
}
