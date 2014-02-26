using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class MultiplicatorCuda
    {
        private int _matrixSize;
        private readonly int _threadsPerBlock;
        static CudaKernel _multiplyTwoVectorWithCuda;

        public MultiplicatorCuda()
        {
            _matrixSize = 256;
            _threadsPerBlock = 256;
        }

        public void InitKernels()
        {
             var cntxt = new CudaContext();
             CUmodule cumodule = cntxt.LoadModule(@"\Kernel\kernel.ptx");
             _multiplyTwoVectorWithCuda = new CudaKernel("_Z6kernelPiS_S_i", cumodule, cntxt)
             {
                 BlockDimensions = _threadsPerBlock,
                 GridDimensions = _matrixSize/_threadsPerBlock + 1
             };
        }

        public double[] Multiply(DenseMatrix m1,DenseMatrix m2)
        {
            _matrixSize = m1.RowCount*m2.ColumnCount;

            // init parameters
            CudaDeviceVariable<double> matrixM1 = DenseMatrix2Doubles(m1);
            CudaDeviceVariable<double> matrixM2 = DenseMatrix2Doubles(m2);
            var matrixM = new CudaDeviceVariable<double>(_matrixSize);
            // run cuda method
            _multiplyTwoVectorWithCuda.Run(matrixM1.DevicePointer, matrixM2.DevicePointer, matrixM.DevicePointer, m1.ColumnCount, m2.ColumnCount, m1.RowCount);
            // copy return to host
            var output = new double[_matrixSize];
            matrixM.CopyToHost(output);

            return output;
        }

        static double[] DenseMatrix2Doubles(DenseMatrix m)
        {
            var mDoubles = new double[m.RowCount * m.ColumnCount];

            for (int i = 0; i < m.RowCount; i++)
            {
                for (int j = 0; j < m.ColumnCount; j++)
                {
                    mDoubles[m.RowCount * i + j] = m[i , j];
                }
            }

            return mDoubles;
        }
    }
}