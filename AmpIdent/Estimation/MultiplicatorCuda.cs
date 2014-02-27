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
             _multiplyTwoVectorWithCuda = new CudaKernel("_Z6kernel_", cumodule, cntxt);
        }

        static float[] DenseMatrix2Floats(DenseMatrix m)
        {
            var mFloats = new float[m.RowCount * m.ColumnCount];

            for (int i = 0; i < m.RowCount; i++)
            {
                for (int j = 0; j < m.ColumnCount; j++)
                {
                    mFloats[m.ColumnCount * i + j] = (float)m[i, j];
                }
            }

            return mFloats;
        }

        public Func<DenseMatrix, DenseMatrix, float[]> Multiply = (m1, m2) =>
        {
            _matrixSize = m1.RowCount*m2.ColumnCount;

            _multiplyTwoVectorWithCuda.BlockDimensions = _threadsPerBlock;
            _multiplyTwoVectorWithCuda.GridDimensions = m1.RowCount * m1.ColumnCount * m2.ColumnCount / _threadsPerBlock + 1;

            // init parameters
            CudaDeviceVariable<float> matrixM1 = DenseMatrix2Floats(m1);
            CudaDeviceVariable<float> matrixM2 = DenseMatrix2Floats(m2);
            var matrixM = new CudaDeviceVariable<float>(_matrixSize);
            // run cuda method
            _multiplyTwoVectorWithCuda.Run(matrixM1.DevicePointer, matrixM2.DevicePointer, matrixM.DevicePointer,
                m1.ColumnCount, m2.ColumnCount, m1.RowCount, m2.RowCount);
            // copy return to host
            var output = new float[_matrixSize];
            matrixM.CopyToHost(output);

            return output;
        };
    }
}