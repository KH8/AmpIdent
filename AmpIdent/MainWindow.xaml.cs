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
            string newLine = Environment.NewLine;

            // parameters
            
            //double k = 0.2; 
            //double q = 1;
            //double sigma2 = 0.5;
            
            // experiments

            var vectorX = new DenseMatrix(10,1,1);
            var vectorY = new DenseMatrix(10, 1, 1);

            for (int i = 0; i <= 9; i++)
            {
                vectorX[i,0] = i;
            }
            Console.WriteLine(newLine + "Vector X:");
            Console.WriteLine(vectorX.ToString());

            for (int i = 0; i <= 9; i++)
            {
                vectorY[i,0] = 1.234F * vectorX[i,0] + 4.523F;
            }
            Console.WriteLine(newLine + "Vector Y:");
            Console.WriteLine(vectorY.ToString());

            var matrixZ = new DenseMatrix(4, 4, 1);

            int k = 5;

            matrixZ[0, 0] = vectorY[k - 1,0];
            matrixZ[0, 1] = vectorY[k - 2,0];
            matrixZ[0, 2] = vectorX[k - 1,0];
            matrixZ[0, 3] = vectorX[k - 2,0];

            matrixZ[1, 0] = vectorY[k,0];
            matrixZ[1, 1] = vectorY[k - 1,0];
            matrixZ[1, 2] = vectorX[k,0];
            matrixZ[1, 3] = vectorX[k - 1,0];
            
            matrixZ[2, 0] = vectorY[k + 1,0];
            matrixZ[2, 1] = vectorY[k,0];
            matrixZ[2, 2] = vectorX[k + 1,0];
            matrixZ[2, 3] = vectorX[k,0];
            
            matrixZ[3, 0] = vectorY[k + 1,0];
            matrixZ[3, 1] = vectorY[k + 2,0];
            matrixZ[3, 2] = vectorX[k + 1,0];
            matrixZ[3, 3] = vectorX[k + 2,0];

            var vectorYk = new DenseMatrix(4, 1, 1);
            vectorYk[0, 0] = vectorY[k, 0];
            vectorYk[1, 0] = vectorY[k+1, 0];
            vectorYk[2, 0] = vectorY[k+2, 0];
            vectorYk[3, 0] = vectorY[k+3, 0];

            Console.WriteLine(newLine + "Matrix Z:");
            Console.WriteLine(matrixZ.ToString());

            var matrixZT = matrixZ.Transpose();
            Console.WriteLine(newLine + "Matrix ZT:");
            Console.WriteLine(matrixZT.ToString());

            var matrixZTZ = matrixZT*matrixZ;
            var matrixTheta = matrixZTZ.Inverse() * matrixZT * vectorYk;
            Console.WriteLine(newLine + "Matrix Theta:");
            Console.WriteLine(matrixTheta.ToString());

            var tempX = new DenseMatrix(20, 1, 1);
            var tempY = new DenseMatrix(20, 1, 1);

            for (int i = 0; i <= 19; i++)
            {
                tempX[i, 0] = i;
            }

            for (int i = 2; i <= 19; i++)
            {
                tempY[i, 0] = -1 * matrixTheta[0, 0] * tempY[i - 1, 0] - matrixTheta[1, 0] * tempY[i - 2, 0] + matrixTheta[2, 0] * tempX[i - 1, 0] + matrixTheta[3, 0] * tempX[i - 2, 0];
            }

            Console.WriteLine(newLine + "Solved:");
            Console.WriteLine(tempX.ToString());
            Console.WriteLine(tempY.ToString());

            while (true)
            {
            }
        }
    }
}
