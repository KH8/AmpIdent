﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
        private string _path;
        private int _loadingPercentage;
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel();

            /*
            _path = "";
            _loadingPercentage = 0;

            MainViewModel mainViewModel = new MainViewModel();
            string newLine = Environment.NewLine;
            Random random = new Random();

            Thread _thread = new Thread(new ThreadStart(Update));
            _thread.IsBackground = false;
            _thread.Start();
            //*/

            ///*
            //TEST MODEL
            var X1 = new DenseMatrix(500, 1, 0.0);
            var Y1 = new DenseMatrix(500, 1, 0.0);

            for (int i = 0; i <= 499; i++)
            {
                X1[i, 0] = -54 * Math.Sin(i / 10);// +random.Next(-10, 100);
                //X1[i, 0] = random.Next(-10, 100);
                //mainViewModel.AddPoint(1, new DataPoint(i, X1[i, 0]));

                if (i <= 10)
                {
                    Y1[i, 0] = X1[i, 0];
                    //mainViewModel.AddPoint(2, new DataPoint(i, Y1[i, 0]));
                }

                if (i > 10)
                {
                    Y1[i, 0] = 30 * Math.Sin(X1[i -10, 0]) - X1[i, 0];
                    //mainViewModel.AddPoint(2, new DataPoint(i, Y1[i, 0]));
                }
            }

            int na = 6;
            int nb = 6;
            int nd = 6;
            int nk = 5;
            int L = 450;
            int iterations = 1000;
            int tStart = 15;
            double averageLimit = 1.0E-12;
            double averageDiff = 0.0;

            //I Step: Y(L)
            var YL = new DenseMatrix(L, 1, 0.0);

            for (int i = 0; i <= L - 1; i++)
            {
                YL[i, 0] = Y1[i + tStart, 0];
            }
            //Console.WriteLine("I step: YL");
            //Console.WriteLine(YL.ToString());

            //II Step:
            var V0 = new DenseMatrix(L + (iterations + 1) * tStart , 1, 1.0);
            for (int i = 0; i <= L + 2 * tStart - 1; i++)
            {
                V0[i, 0] = random.Next(-10, 10);
            }
            //Console.WriteLine("II step: V0");
            //Console.WriteLine(V0.ToString());

            var Theta_k_1 = new DenseMatrix(na + nb + nd, 1, 0.0);
            for (int i = 0; i <= na + nb + nd - 1; i++)
            {
                Theta_k_1[i, 0] = 0.0;
            }

            var Fi_k = new DenseMatrix(na + nb + nd, 1, 0.0);
            var Fi_k_L = new DenseMatrix(na + nb + nd, L, 0.0);

            var Fi_k_t = new DenseMatrix(na + nb + nd, 1, 0.0);
            var Vk_t = new DenseMatrix(L + (iterations + 1) * tStart, 1, 1.0);

            var ThetaDiff = new DenseMatrix(na + nb + nd, 1, 0.0);

            for (int k = 1; k <= iterations; k++)
            {
                //III Step: Fi_k(L)

                for (int t = tStart; t <= tStart + L - 1; t++)
                {
                    Fi_k = CalculateFi_k(na, nb, nd, nk, t, X1, Y1, V0);
                    for (int i = 0; i <= na + nb + nd - 1; i++)
                    {
                        Fi_k_L[i, t - tStart] = Fi_k[i, 0];
                    }
                }
                //Console.WriteLine("III step: Fi_k :" + k.ToString());
                //Console.WriteLine(Fi_k.ToString());
                //Console.WriteLine("III step: Fi_k_L :" + k.ToString());
                //Console.WriteLine(Fi_k_L.ToString());

                //IV Step: Theta(k)
                var FiFiT = Fi_k_L * Fi_k_L.Transpose();
                var Theta_k = FiFiT.Inverse() * Fi_k_L * YL;
                //Console.WriteLine("IV step: Theta_k :" + k.ToString());
                //Console.WriteLine(Theta_k.ToString());

                //V Step: Vk_t
                for (int i = 0; i <= L + tStart - 1; i++)
                {
                    Fi_k_t = CalculateFi_k(na, nb, nd, nk, i + tStart, X1, Y1, V0);

                    var Theta_k_Y_1 = Theta_k.Transpose() * Fi_k_t;
                    Vk_t[i, 0] = Y1[i + tStart, 0] - Theta_k_Y_1[0, 0];
                }
                //Console.WriteLine("V Step: Vk_t :" + k.ToString());
                //Console.WriteLine(Vk_t.ToString());

                //VI Step Average Error;
                for (int i = 0; i <= na + nb + nd - 1; i++)
                {
                    ThetaDiff[i, 0] = Math.Abs(Theta_k[i, 0] - Theta_k_1[i, 0]);
                }
                //Console.WriteLine("VI Step: Theta Difference :" + k.ToString());
                //Console.WriteLine(ThetaDiff.ToString());
                averageDiff = 0.0;
                for (int i = 0; i <= na + nb + nd - 1; i++)
                {
                    averageDiff = averageDiff + ThetaDiff[i, 0];
                }
                averageDiff = averageDiff / (na + nb + nd);
                Console.WriteLine("VI Step: Average Difference :" + k.ToString());
                Console.WriteLine(averageDiff.ToString());

                //Decision
                V0 = Vk_t;
                for (int i = 0; i <= na + nb + nd - 1; i++)
                {
                    Theta_k_1[i, 0] = Theta_k[i, 0];
                }

                if (averageDiff < averageLimit) k = iterations;
            }

            //FINAL: Vk
            for (int i = 0; i <= L + tStart - 1; i++)
            {
                Fi_k_t = CalculateFi_k(na, nb, nd, nk, i + tStart, X1, Y1, V0);

                var Theta_k_Y_1 = Theta_k_1.Transpose() * Fi_k_t;
                //mainViewModel.AddPoint(3, new DataPoint(i + tStart, Theta_k_Y_1[0, 0]));
            }

            for (int i = 0; i <= 499; i++)
            {
                X1[i, 0] = 100*(i-3)*(i-400)*(i-230);
                //X1[i, 0] = random.Next(-10, 100);
                mainViewModel.AddPoint(4, new DataPoint(i, X1[i, 0]));

                if (i <= 10)
                {
                    Y1[i, 0] = X1[i, 0];
                    mainViewModel.AddPoint(5, new DataPoint(i, Y1[i, 0]));
                }

                if (i > 10)
                {
                    Y1[i, 0] = 30 * Math.Sin(X1[i - 10, 0]) - X1[i, 0];
                    mainViewModel.AddPoint(5, new DataPoint(i, Y1[i, 0]));
                }
            }
            for (int i = 0; i <= L + tStart - 1; i++)
            {
                Fi_k_t = CalculateFi_k(na, nb, nd, nk, i + tStart, X1, Y1, V0);

                var Theta_k_Y_1 = Theta_k_1.Transpose() * Fi_k_t;
                mainViewModel.AddPoint(6, new DataPoint(i + tStart, Theta_k_Y_1[0, 0]));
            }
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

            DataContext = mainViewModel;
        }

        private DenseMatrix CalculateFi_k(int t_na, int t_nb, int t_nd, int t_nk, int t_t, DenseMatrix t_X, DenseMatrix t_Y, DenseMatrix t_V)
        {
            var t_Fi_k = new DenseMatrix(t_na + t_nb + t_nd, 1, 0.0);
            for (int i = 1; i <= t_na + t_nb + t_nd; i++)
            {
                if (i <= t_na)
                {
                    t_Fi_k[i - 1, 0] = -1 * t_Y[t_t - i, 0];
                }
                if (i > t_na && i <= t_na + t_nb)
                {
                    t_Fi_k[i - 1, 0] = t_X[t_t - t_nk - i + t_na, 0];
                }
                if (i > t_na + t_nb && i <= t_na + t_nb + t_nd)
                {
                    t_Fi_k[i - 1, 0] = t_V[t_t - i + t_na + t_nb, 0];
                }
            }

            return t_Fi_k;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = new MainViewModel();

            // first we need to read our wav file, so we can get our info:
            byte[] wav = File.ReadAllBytes(_path);

            // then we are going to get our file's info
            var NumChannnels = wav[22];
            var SampleRate = wav[24] + 256 * wav[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            int samples = (wav.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (NumChannnels == 2) samples /= 2;

            int pos = 44; // start of data chunk
            _loadingPercentage = 0;

            for (int i = 0; i < samples; i++)
            {
                int number = wav[pos] + 256 * wav[pos + 1];
                if (number > 32767) number -= 65534;
                mainViewModel.AddPoint(1, new DataPoint(i, number));

                _loadingPercentage = i * 100 / samples;
                pos += 2;
                Console.WriteLine(_loadingPercentage.ToString());
            }

            DataContext = mainViewModel;
        }

        private void PathBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            TextBox _pathBox = (TextBox)sender;
            _path = _pathBox.Text.ToString();
        }

        private void Update()
        {
        }
    }
}
