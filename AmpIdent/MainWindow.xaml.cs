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
            
            double k1 = -0.2;
            double k2 = +0.7;
            double k3 = -3;
            double k4 = -0.9;
            double q = -4.53;
            int sigma2 = 9;

            Random random = new Random();
            
            // experiments

            var X1 = new DenseMatrix(500, 1, 0.0);
            var Y1 = new DenseMatrix(500, 1, 0.0);

            for (int j = 1; j <= 500; j++)
            {
                int i = j - 1;
                X1[i, 0] = j;
                Y1[i, 0] = /*k4 * Math.Pow(X[i, 0] *  k3 * Math.Pow(X[i, 0]+9, 2),9) + k2 * Math.Pow(X[i, 0], 2) +*/ k1 * X1[i, 0] +  q; /* * random.Next(-6 * sigma2, 6* sigma2); */
                mainViewModel.AddPoint(1, new DataPoint(X1[i, 0], Y1[i, 0]));
                mainViewModel.AddPoint(2, new DataPoint(X1[i, 0], k1 * X1[i, 0] + q));
            }

            var Z = new DenseMatrix(500, 8, 1.0);
            for (int i = 10; i <= 498; i++)
            {
                Z[i, 0] = -1 * Y1[i - 1, 0];
                Z[i, 1] = -1 * Y1[i - 2, 0];
                Z[i, 2] = -1 * Y1[i - 3, 0];
                Z[i, 3] = 1;
                Z[i, 4] = X1[i, 0];
                Z[i, 5] = X1[i - 1, 0];
                Z[i, 6] = X1[i - 2, 0];
                Z[i, 7] = X1[i - 3, 0];
            }
            Console.WriteLine(Z.ToString());

            var ZT = Z.Transpose();
            Console.WriteLine(ZT.ToString());

            var ZTZ = ZT * Z;
            Console.WriteLine(ZTZ.ToString());

            var determinant = ZTZ.Determinant();
            Console.WriteLine(newLine + "Determinant: " +determinant.ToString() +newLine);

            var Theta = ZTZ.Inverse() * ZT * Y1;
            Console.WriteLine(Theta.ToString());

            var X = new DenseMatrix(500, 1, 0.0);
            var Y = new DenseMatrix(500, 1, 0.0);

            for (int i = 0; i <= 11; i++)
            {
                X[i, 0] = X1[i, 0];
                Y[i, 0] = Y1[i, 0];
            }

            for (int j = 11; j <= 500; j++)
            {
                int i = j - 1;
                X[i, 0] = j;
                Y[i, 0] = -1 * Theta[0, 0] * Y[i - 1, 0] - Theta[1, 0] * Y[i - 2, 0] - Theta[2, 0] * Y[i - 3, 0] + Theta[3, 0] + Theta[4, 0] * Y[i, 0] + Theta[5, 0] * Y[i - 1, 0] + Theta[6, 0] * Y[i - 2, 0] + Theta[7, 0] * Y[i - 3, 0];
                mainViewModel.AddPoint(9, new DataPoint(X[i, 0], Y[i, 0]));
            }
            
            DataContext = mainViewModel;
        }
    }
}
