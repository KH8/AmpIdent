using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace AmpIdent.Estimation
{
    public class MultiplicatorCuda
    {
        private const int VECTOR_SIZE = 5120;
        const int THREADS_PER_BLOCK = 256;

        static CudaKernel addTwoVectorWithCuda;

        public int VectorSize
        {
            get { return VECTOR_SIZE; }
        }

        public void InitKernels()
        {
             CudaContext cntxt = new CudaContext();
             CUmodule cumodule = cntxt.LoadModule(@"\Kernel\kernel.ptx");
             addTwoVectorWithCuda = new CudaKernel("_Z6kernelPiS_S_i", cumodule, cntxt);
             addTwoVectorWithCuda.BlockDimensions = THREADS_PER_BLOCK;
             addTwoVectorWithCuda.GridDimensions = VECTOR_SIZE / THREADS_PER_BLOCK + 1;
        }

        public Func<int[], int[], int, int[]> AddVectors = (a, b, size) =>
        {
            // init parameters
            CudaDeviceVariable<int> vector_hostA = a;
            CudaDeviceVariable<int> vector_hostB = b;
            CudaDeviceVariable<int> vector_hostOut = new CudaDeviceVariable<int>(size);
            // run cuda method
            addTwoVectorWithCuda.Run(vector_hostA.DevicePointer, vector_hostB.DevicePointer, vector_hostOut.DevicePointer, size);
            // copy return to host
            int[] output = new int[size];
            vector_hostOut.CopyToHost(output);
            return output;
        };
    }
}