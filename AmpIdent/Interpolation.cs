using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent
{
    class Interpolation
    {
        //Private fields
        //Parameters
        private int _estimationLength;

        //Status
        private Boolean _estimationDone;

        //Outputs
        private DenseMatrix _output;

        //Public fields
        //Status
        public Boolean EstimationDone
        {
            get { return _estimationDone; }
        }

        //matrixes
        public DenseMatrix Output
        {
            get
            {
                if (_estimationDone)
                {
                    return _output;
                }
                return null;
            }
        }

        //Methods
        //Constructor
        public Interpolation()
        {
            _estimationLength = 0;
            _estimationDone = false;
        }

        public void Compute(DenseMatrix x1, int size)
        {
            //Setting Preparation.....................................................................................................
            _estimationLength = x1.Values.Length;
            
            //Creation of a Z matrix..................................................................................................
            var z = new DenseMatrix(_estimationLength, size, 1.0);
            for (var i = 0; i <= _estimationLength - 1; i++)
            {
                for (var j = 0; j <= size - 1; j++)
                {
                    z[i, j] = Math.Pow(i,j);
                }
            }

            //Computation of Theta matrix.............................................................................................
            var zt = z.Transpose();
            var ztz = zt * z;
            var theta = ztz.Inverse() * zt * x1;

            _output = new DenseMatrix(_estimationLength, 1, 1.0);

            //Output creation.........................................................................................................
            for (var i = 0; i <= _estimationLength - 1; i++)
            {
               for (var j = 0; j <= size - 1; j++)
               {
                   _output[i, 0] += theta[j,0]*Math.Pow(i,j);
               }
            }
            _estimationDone = true;
        }
    }
}
