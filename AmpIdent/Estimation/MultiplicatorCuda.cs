using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace AmpIdent.Estimation
{
    public class MultiplicatorCuda
    {
        static CudaKernel _addWithCuda;

        public void InitKernels()
        {
            var cntxt = new CudaContext();
            CUmodule cumodule = cntxt.LoadModule(@"Kernel\kernel.ptx");
            _addWithCuda = new CudaKernel("_Z6kerneliiPi", cumodule, cntxt);
        }

        public Func<int, int, int> CudaAdd = (a, b) =>
        {
            // init output parameters
            CudaDeviceVariable<int> resultDev = 0;
            int resultHost = 0;
            // run CUDA method
            _addWithCuda.Run(a, b, resultDev.DevicePointer);
            // copy return to host
            resultDev.CopyToHost(ref resultHost);
            return resultHost;
        };
    }
}