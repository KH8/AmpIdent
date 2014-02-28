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

        static void InitKernels()
        {
            _matrixSize = 256;
            _threadsPerBlock = 512;

            var cntxt = new CudaContext();
            CUmodule cumodule = cntxt.LoadModule(@"\Kernel\kernel.ptx");
            _multiplyTwoVectorWithCuda = new CudaKernel("_Z6kernel_", cumodule, cntxt);
        }

        public static Func<DenseMatrix, DenseMatrix, DenseMatrix> Multiply = (m1, m2) =>
        {
            _matrixSize = m1.RowCount*m2.ColumnCount;

            // init parameters
            InitKernels();
            _multiplyTwoVectorWithCuda.BlockDimensions = _threadsPerBlock;
            _multiplyTwoVectorWithCuda.GridDimensions = m1.RowCount * m2.ColumnCount / _threadsPerBlock + 1;

            CudaDeviceVariable<float> matrixM1 = MatrixConverter.DenseMatrix2Floats(m1);
            CudaDeviceVariable<float> matrixM2 = MatrixConverter.DenseMatrix2Floats(m2);
            var matrixM = new CudaDeviceVariable<float>(_matrixSize);
            // run cuda method
            _multiplyTwoVectorWithCuda.Run(matrixM1.DevicePointer, matrixM2.DevicePointer, matrixM.DevicePointer,
                m1.ColumnCount, m2.ColumnCount, m1.RowCount, m2.RowCount);
            // copy return to host
            var output = new float[_matrixSize];
            matrixM.CopyToHost(output);

            var outputMatrix = MatrixConverter.Floats2DenseMatrix(output, m1.RowCount, m2.ColumnCount);
            return outputMatrix;
        };
    }
}