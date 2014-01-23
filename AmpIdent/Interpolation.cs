using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                else return null;
            }
        }

        //Methods
        //Constructor
        public Interpolation()
        {
            _estimationLength = 0;
            _estimationDone = false;
        }

        public void Compute(DenseMatrix X1, int size)
        {
            //Setting Preparation.....................................................................................................
            _estimationLength = X1.Values.Length;
            
            //Creation of a Z matrix..................................................................................................
            var Z = new DenseMatrix(_estimationLength, size, 1.0);
            for (int i = 0; i <= _estimationLength - 1; i++)
            {
                for (int j = 0; j <= size - 1; j++)
                {
                    Z[i, j] = Math.Pow(X1[i, 0],j);
                }
            }

            //Computation of Theta matrix.............................................................................................
            var ZT = Z.Transpose();
            var ZTZ = ZT * Z;
            var determinant = ZTZ.Determinant();
            var Theta = ZTZ.Inverse() * ZT * X1;

            _output = new DenseMatrix(_estimationLength, 1, 1.0);

            //Output creation.........................................................................................................
            for (int i = 0; i <= _estimationLength - 1; i++)
            {
               for (int j = 0; j <= size - 1; j++)
               {
                   _output[i, 0] += Theta[j,0]*Math.Pow(i,j);
               }

            }

            _estimationDone = true;
        }
    }
}
