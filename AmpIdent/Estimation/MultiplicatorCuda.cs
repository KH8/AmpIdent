using System;
using System.Security.Cryptography;
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
        private static CudaContext _cnContext;
        private static Boolean _initialized;

        static void InitKernels()
        {
            _matrixSize = 256;
            _threadsPerBlock = 256;

            _cnContext = new CudaContext();
            CUmodule cumodule = _cnContext.LoadModule(@"\Kernel\kernel.ptx");
            _multiplyTwoVectorWithCuda = new CudaKernel("_Z6kernel_", cumodule, _cnContext);

            _initialized = true;
        }

        public static Func<DenseMatrix, DenseMatrix, DenseMatrix> Multiply = (m1, m2) =>
        {
            // init parameters
            if (!_initialized) InitKernels();

            _matrixSize = m1.RowCount * m2.ColumnCount;
            _multiplyTwoVectorWithCuda.BlockDimensions = _threadsPerBlock;
            _multiplyTwoVectorWithCuda.GridDimensions = m1.RowCount / _threadsPerBlock + 1;

            CudaDeviceVariable<float> matrixM1 = MatrixConverter.DenseMatrix2Floats(m1);
            CudaDeviceVariable<float> matrixM2 = MatrixConverter.DenseMatrix2Floats(m2);
            var matrixM = new CudaDeviceVariable<float>(_matrixSize);
            // run cuda method
            _multiplyTwoVectorWithCuda.Run(matrixM1.DevicePointer, matrixM2.DevicePointer, matrixM.DevicePointer,
                m1.ColumnCount, m2.ColumnCount, m1.RowCount, m2.RowCount);
            // copy return to host
            var output = new float[_matrixSize];
            matrixM.CopyToHost(output);

            return MatrixConverter.Floats2DenseMatrix(output, m2.ColumnCount, m1.RowCount);
        };
    }
}