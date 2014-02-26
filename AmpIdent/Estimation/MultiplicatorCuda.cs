using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class MultiplicatorCuda
    {
        private static int _matrixSize;
        private static int _threadsPerBlock;
        static CudaKernel _multiplyTwoVectorWithCuda;

        public MultiplicatorCuda()
        {
            _matrixSize = 256;
            _threadsPerBlock = 1;
            InitKernels();
        }

        static void InitKernels()
        {
             var cntxt = new CudaContext();
             CUmodule cumodule = cntxt.LoadModule(@"\Kernel\kernel.ptx");
             _multiplyTwoVectorWithCuda = new CudaKernel("_Z6kernelPdS_S_iiii", cumodule, cntxt);
        }

        static double[] DenseMatrix2Doubles(DenseMatrix m)
        {
            var mDoubles = new double[m.RowCount * m.ColumnCount];

            for (int i = 0; i < m.RowCount; i++)
            {
                for (int j = 0; j < m.ColumnCount; j++)
                {
                    mDoubles[m.ColumnCount * i + j] = m[i, j];
                }
            }

            return mDoubles;
        }

        public Func<DenseMatrix, DenseMatrix, Double[]> Multiply = (m1, m2) =>
        {
            _matrixSize = m1.RowCount*m2.ColumnCount;

            _multiplyTwoVectorWithCuda.BlockDimensions = _threadsPerBlock;
            _multiplyTwoVectorWithCuda.GridDimensions = _matrixSize/_threadsPerBlock + 1;

            // init parameters
            CudaDeviceVariable<double> matrixM1 = DenseMatrix2Doubles(m1);
            CudaDeviceVariable<double> matrixM2 = DenseMatrix2Doubles(m2);
            var matrixM = new CudaDeviceVariable<double>(_matrixSize);
            // run cuda method
            _multiplyTwoVectorWithCuda.Run(matrixM1.DevicePointer, matrixM2.DevicePointer, matrixM.DevicePointer,
                m1.ColumnCount, m2.ColumnCount, m1.RowCount, m2.RowCount);
            // copy return to host
            var output = new double[_matrixSize];
            matrixM.CopyToHost(output);

            return output;
        };
    }
}