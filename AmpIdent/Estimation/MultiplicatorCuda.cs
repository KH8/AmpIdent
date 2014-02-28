using System;
using System.Security.Cryptography;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class MultiplicatorCuda
    {
        private static int _matrixSize;
        private static int _threadsPerBlock;
        static CudaKernel _multiplyTwoVectorWithCuda;
        private static CudaContext _cnContext;

        static void InitKernels()
        {
            _matrixSize = 256;
            _threadsPerBlock = 512;

            _cnContext = new CudaContext(CudaContext.GetMaxGflopsDeviceId());
            CUmodule cumodule = _cnContext.LoadModule(@"\Kernel\kernel.ptx");
            _multiplyTwoVectorWithCuda = new CudaKernel("_Z6kernel_", cumodule, _cnContext);
        }

        static void CleanUpResources()
        {
            if (_cnContext != null) _cnContext.Dispose();
        }

        public static Func<DenseMatrix, DenseMatrix, DenseMatrix> Multiply = (m1, m2) =>
        {
            // init parameters
            InitKernels();

            _matrixSize = m1.RowCount * m2.ColumnCount;
            _multiplyTwoVectorWithCuda.BlockDimensions = _threadsPerBlock;
            _multiplyTwoVectorWithCuda.GridDimensions = m1.RowCount * m2.ColumnCount / _threadsPerBlock + 1;

            var matrixM1Float = MatrixConverter.DenseMatrix2Floats(m1);
            CudaDeviceVariable<float> matrixM1 = matrixM1Float;
            matrixM1.CopyToDevice(matrixM1Float);
            var matrixM2Float =MatrixConverter.DenseMatrix2Floats(m2);
            CudaDeviceVariable<float> matrixM2 = matrixM2Float;
            matrixM2.CopyToDevice(matrixM2Float);
            var matrixM = new CudaDeviceVariable<float>(_matrixSize);

            // run cuda method
            _multiplyTwoVectorWithCuda.Run(matrixM1.DevicePointer, matrixM2.DevicePointer, matrixM.DevicePointer,
                m1.ColumnCount, m2.ColumnCount, m1.RowCount, m2.RowCount);

            // copy return to host
            var output = new float[_matrixSize];
            matrixM.CopyToHost(output);

            matrixM1.Dispose();
            matrixM2.Dispose();
            matrixM.Dispose();
            CleanUpResources();

            return MatrixConverter.Floats2DenseMatrix(output, m2.ColumnCount, m1.RowCount);
        };
    }
}