﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmpIdent
{
    class Interpolation
    {
        public Interpolation()
        {
            //*/

            /*
            //INTERPOLATION
            // parameters
            
            double k1 = -0.2;
            double k2 = +0.7;
            double k3 = -3;
            double k4 = -0.9;
            double q = -4.53;
            int sigma2 = 9000;
            
            // experiments

            var X1 = new DenseMatrix(500, 1, 0.0);
            var Y1 = new DenseMatrix(500, 1, 0.0);

            for (int j = 1; j <= 500; j++)
            {
                int i = j - 1;

                if (i != 0) X1[i, 0] = i;
                else X1[i, 0] = random.Next(0, 10);

                Y1[i, 0] = (X1[i, 0] - 100) * (X1[i, 0] - 250) * (X1[i, 0] - 30) + random.Next(-6 * sigma2, 6 * sigma2);
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
            }
            //*/
        }
    }
}
