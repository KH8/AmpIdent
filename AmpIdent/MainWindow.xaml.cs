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

            //TEST MODEL
            var X1 = new DenseMatrix(500, 1, 0.0);
            var Y1 = new DenseMatrix(500, 1, 0.0);

            for (int i = 0; i <= 499; i++)
            {
                X1[i, 0] = -54 * Math.Sin(Math.Sqrt(i));
                mainViewModel.AddPoint(1, new DataPoint(i, X1[i, 0]));

                Y1[i, 0] = X1[i, 0] * X1[i, 0] / 100;
                mainViewModel.AddPoint(2, new DataPoint(i, Y1[i, 0]));
            }

            int na = 5;
            int nb = 5;

            //I STEP: Y(L)
            int L = 100;
            var YL = new DenseMatrix(L, 1, 0.0);

            for (int i = 1; i <= L - 1; i++)
            {
                YL[i, 0] = Y1[i, 0];
            }
            Console.WriteLine("I step: YL");
            Console.WriteLine(YL.ToString());

            //II STEP:
            int k = 1;
            var V0 = new DenseMatrix(L, 1, 0.0);
            Random random = new Random();
            for (int i = 1; i <= L - 1; i++)
            {
                V0[i, 0] = random.Next(-10, 10);
            }
            Console.WriteLine("II step: V0");
            Console.WriteLine(V0.ToString());



            /*
            // parameters
            
            double k1 = -0.2;
            double k2 = +0.7;
            double k3 = -3;
            double k4 = -0.9;
            double q = -4.53;
            int sigma2 = 9000;

            Random random = new Random();
            
            // experiments

            var X1 = new DenseMatrix(500, 1, 0.0);
            var Y1 = new DenseMatrix(500, 1, 0.0);

            for (int j = 1; j <= 500; j++)
            {
                int i = j - 1;

                if (i != 0) X1[i, 0] = random.Next((int)X1[i - 1, 0], (int)X1[i - 1, 0] + 10);
                else X1[i, 0] = random.Next(0, 10);

                Y1[i, 0] = k4 * Math.Pow(X1[i, 0], -2) *  k3 * Math.Pow(X1[i, 0], -3) + k2 * Math.Pow(X1[i, 0], 2) + k1 * X1[i, 0] +  q + random.Next(-6 * sigma2, 6* sigma2);
                mainViewModel.AddPoint(1, new DataPoint(X1[i, 0], Y1[i, 0]));
                mainViewModel.AddPoint(2, new DataPoint(X1[i, 0], k4 * Math.Pow(X1[i, 0], -2) * k3 * Math.Pow(X1[i, 0], -3) + k2 * Math.Pow(X1[i, 0], 2) + k1 * X1[i, 0] + q));
            }

            var Z = new DenseMatrix(500, 4, 1.0);
            for (int i = 0; i <= 498; i++)
            {
                Z[i, 0] = 1;
                Z[i, 1] = X1[i, 0];
                Z[i, 2] = X1[i, 0] * X1[i, 0];
                Z[i, 3] = X1[i, 0] * X1[i, 0] * X1[i, 0];
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

            for (int i = 0; i <= 499; i++)
            {
                Y[i, 0] = Theta[0, 0] + Theta[1, 0] * X1[i, 0] + Theta[2, 0] * X1[i, 0] * X1[i, 0] + Theta[3, 0] * X1[i, 0] * X1[i, 0] * X1[i, 0];
                mainViewModel.AddPoint(9, new DataPoint(X1[i, 0], Y[i, 0]));
            }*/

            DataContext = mainViewModel;
        }
    }
}
