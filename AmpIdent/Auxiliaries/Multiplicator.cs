using System;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Auxiliaries
{
    public class Multiplicator
    {
        private static Boolean _compute;
        private static DenseMatrix _m1;
        private static DenseMatrix _m2;
        private static DenseMatrix _m;

        private readonly Thread _thread;

        public Multiplicator()
        {
            _thread = new Thread(Compute);

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = false;
            _thread.Start();
        }

        public static DenseMatrix Multiply(DenseMatrix m1, DenseMatrix m2)
        {
            _m1 = m1;
            _m2 = m2;
            _compute = true;

            while (_compute)
            {
                Thread.Sleep(2000);
            }

            return _m;
        }

        public void Compute()
        {
            while (_thread.IsAlive)
            {
                if (_compute)
                {
                    _m = new DenseMatrix(_m1.RowCount, _m2.ColumnCount, 0.0);

                    Parallel.For(0, _m1.RowCount, i =>
                    {
                        for (var j = 0; j < _m2.ColumnCount; ++j) // each col of B
                            for (var k = 0; k < _m1.ColumnCount; ++k) // could use k < bRows
                                _m[i,j] += _m1[i,k] * _m2[k,j];
                    });
                    _compute = false;
                }
                Thread.Sleep(100);
            }
        }
    }
}