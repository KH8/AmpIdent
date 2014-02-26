using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace AmpIdent.Estimation
{
    public class MultiplicatorCuda
    {
        private const int VECTOR_SIZE = 5120;
        const int THREADS_PER_BLOCK = 256;
 
        static CudaKernel fillVectorWithCuda;

        public int VectorSize
        {
            get { return VECTOR_SIZE; }
        }

        public void InitKernels()
        {
             CudaContext cntxt = new CudaContext();
             CUmodule cumodule = cntxt.LoadModule(@"\Kernel\kernel.ptx");
             fillVectorWithCuda = new CudaKernel("_Z6kernelPii", cumodule, cntxt);
             fillVectorWithCuda.BlockDimensions = THREADS_PER_BLOCK;
             fillVectorWithCuda.GridDimensions = VectorSize / THREADS_PER_BLOCK + 1;
        }
 
        public Func<int[], int, int[]> FillVector = (m, value) =>
        {
            // init parameters
            CudaDeviceVariable<int> vector_host = m;
            // run cuda method
            fillVectorWithCuda.Run(vector_host.DevicePointer, value);
            // copy return to host
            int[] output = new int[m.Length];
            vector_host.CopyToHost(output);
            return output;
        };
    }
}