using System;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent
{
    public class Armax
    {
        //private
        //model size parameters
        private int _naParameter;
        private int _nbParameter;
        private int _ndParameter;
        private int _nkParameter;

        //model estimation parameters
        private int _numberOfIterations;
        private int _startingPoint;
        private double _acceptableError;
        private int _estimationLength;

        //status
        private int _estimationStatusPercentage;
        private Boolean _estimationDone;
        private double _estimationError;
        private double _estimationDifference;
        private int _modelShift;

        private Boolean _compute;
        private DenseMatrix _m1;
        private DenseMatrix _m2;
        private DenseMatrix _m;

        //matrixes
        private DenseMatrix _v0;
        private DenseMatrix _theta;
        private DenseMatrix _yk;

        //public
        //model size parameters
        public int NaParameter
        {
            get { return _naParameter; }
            set { _naParameter = value; }
        }
        public int NbParameter
        {
            get { return _nbParameter; }
            set { _nbParameter = value; }
        }
        public int NdParameter
        {
            get { return _ndParameter; }
            set { _ndParameter = value; }
        }
        public int NkParameter
        {
            get { return _nkParameter; }
            set { _nkParameter = value; }
        }

        //model estimation parameters
        public int NumberOfIterations
        {
            get { return _numberOfIterations; }
            set { _numberOfIterations = value; }
        }
        public int StartingPoint
        {
            get { return _startingPoint; }
            set { _startingPoint = value; }
        }
        public double AcceptableError
        {
            get { return _acceptableError; }
            set { _acceptableError = value; }
        }
        public int EstimationLength
        {
            get { return _estimationLength; }
            set { _estimationLength = value; }
        }
        public int ModelShift
        {
            get { return _modelShift; }
            set { _modelShift = value; }
        }

        //status
        public int EstimationPercentage
        {
            get { return _estimationStatusPercentage; }
        }
        public Boolean EstimationDone
        {
            get { return _estimationDone; }
        }
        public double EstimationError
        {
            get { return _estimationError; }
        }
        public double EstimationDifference
        {
            get { return _estimationDifference; }
        }

        //matrixes
        public DenseMatrix V0
        {
            get {
                if (_estimationDone)
                {
                    if (_v0 != null) return _v0;
                }
                return null;
            }
        }
        public DenseMatrix Theta
        {
            get
            {
                if (_estimationDone)
                {
                    return _theta;
                }
                return null;
            }
        }
        public DenseMatrix Yk
        {
            get
            {
                if (_estimationDone)
                {
                    return _yk;
                }
                return null;
            }
        }

        //Methods
        //Constructor
        public Armax()
        {
            _estimationStatusPercentage = 0;
            _estimationDone = false;

            _naParameter = 10;
            _nbParameter = 10;
            _ndParameter = 10;
            _nkParameter = 0;

            _numberOfIterations = 20;
            _startingPoint = 100;
            _acceptableError = 1.0E-12;
            _estimationLength = 500;
            _modelShift = 0;

            var thread = new Thread(Compute);

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = false;
            thread.Start();
        }

        public void Compute(DenseMatrix x1, DenseMatrix y1, int estimationLength)
        {
            var random = new Random();
            //this._y1 = Y1;

            //Setting preparation.....................................................................................................
            _estimationError = 0.0;
            _estimationStatusPercentage = 0;
            if (estimationLength == 0) _estimationLength = x1.Values.Length - _startingPoint;
            else _estimationLength = estimationLength;

            //I Step: Y(L)............................................................................................................

            var yl = new DenseMatrix(_estimationLength, 1, 0.0);
            for (var i = 0; i <= _estimationLength - 1; i++) { yl[i, 0] = y1[i + _startingPoint, 0]; }
            Console.WriteLine(@"Step I: DONE");

            //II Step: V0.............................................................................................................

            _v0 = new DenseMatrix(x1.Values.Length, 1, 1.0); //
            for (var i = 0; i <= x1.Values.Length - 1; i++) { _v0[i, 0] = random.Next(-100, 100); }
            Console.WriteLine(@"Step II: DONE");

            //Definition of temporary matrixes
            var thetaK1 = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            for (var i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++) { thetaK1[i, 0] = 0.0; }
            var Fi_k = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            var Fi_k_L = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, _estimationLength, 0.0);
            var Fi_k_t = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            var Vk_t = new DenseMatrix(_estimationLength + (_numberOfIterations + 1) * _startingPoint, 1, 1.0);
            var ThetaDiff = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);

            //Predefinition of results
            _theta = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            _yk = new DenseMatrix(x1.Values.Length, 1, 0.0);

            for (var k = 1; k <= _numberOfIterations; k++)
            {
                //III Step: Fi_k(_estimationLength)...................................................................................

                for (var t = _startingPoint; t <= _startingPoint + _estimationLength - 1; t++)
                {
                    Fi_k = CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, t, x1, y1, _v0);
                    for (var i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                    {
                        Fi_k_L[i, t - _startingPoint] = Fi_k[i, 0];
                    }
                }
                Console.WriteLine(@"Step III: DONE");

                //IV Step: Theta(k)...................................................................................................

                var fiKLt = Fi_k_L.Transpose();
                Console.WriteLine(@"Step IV: 1/5 DONE");
                var fiFiT = Multiply(Fi_k_L, (DenseMatrix)fiKLt);
                Console.WriteLine(@"Step IV: 2/5 DONE");
                var fiFiTi = fiFiT.Inverse();
                Console.WriteLine(@"Step IV: 3/5 DONE");
                var fiFiTiFiKl = Multiply((DenseMatrix)fiFiTi, Fi_k_L);
                Console.WriteLine(@"Step IV: 4/5 DONE");
                var thetaK = fiFiTiFiKl * yl;
                Console.WriteLine(@"Step IV: 5/5 DONE");

                //V Step: Vk_t........................................................................................................

                for (var i = 0; i <=_estimationLength - 1; i++)
                {
                    Fi_k_t = CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, i + _startingPoint, x1, y1, _v0);
                    var thetaKy1 = thetaK.Transpose() * Fi_k_t;
                    Vk_t[i, 0] = y1[i + _startingPoint, 0] - thetaKy1[0, 0];
                }
                Console.WriteLine(@"Step V: DONE");

                //VI Step: Average Error..............................................................................................

                for (var i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                {
                    ThetaDiff[i, 0] = Math.Abs(thetaK[i, 0] - thetaK1[i, 0]);
                }
                _estimationError = 0.0;
                for (var i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                {
                    _estimationError = _estimationError + ThetaDiff[i, 0];
                }
                _estimationDifference = 0.0;
                for (var i = 0; i <= _estimationLength - 1; i++)
                {
                    _estimationDifference += Vk_t[i, 0];
                }
                _estimationDifference = _estimationDifference / _estimationLength;
                _estimationError = _estimationError / (_naParameter + _nbParameter + _ndParameter);
                Console.WriteLine(k + @" iteration: VI Step: Estimation Error " + @" : " + _estimationError + @"; Estimation Difference : " + _estimationDifference);
                Console.WriteLine(@"Step VI: DONE");

                //VII Step: Decision..................................................................................................

                if (_estimationError != _estimationError) k = _numberOfIterations;
                else
                {
                    _v0 = Vk_t;
                    for (var i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                    {
                        thetaK1[i, 0] = thetaK[i, 0];
                        _estimationStatusPercentage = k * 100 / _numberOfIterations;
                    }
                    if (_estimationError < _acceptableError) k = _numberOfIterations;
                }
                Console.WriteLine(@"Step VII: DONE");
            }

            //END: Vk.................................................................................................................
            _theta = thetaK1;
            _yk = Model(x1);
            Console.WriteLine(@"Estimation: DONE");
            
            _estimationDone = true;
        }

        public DenseMatrix CalculateFi_k(int tNa, int tNb, int tNd, int tNk, int tT, DenseMatrix tX, DenseMatrix tY, DenseMatrix tV)
        {
            var tFiK = new DenseMatrix(tNa + tNb + tNd, 1, 0.0);
            for (var i = 1; i <= tNa + tNb + tNd; i++)
            {
                if (i <= tNa)
                {
                    tFiK[i - 1, 0] = -1 * tY[tT - i, 0];
                }
                if (i > tNa && i <= tNa + tNb)
                {
                    tFiK[i - 1, 0] = tX[tT - tNk - i + tNa + _modelShift, 0];
                }
                if (i > tNa + tNb && i <= tNa + tNb + tNd)
                {
                    tFiK[i - 1, 0] = tV[tT - i + tNa + tNb, 0];
                }
            }
            return tFiK;
        }

        public DenseMatrix Model(DenseMatrix tX1)
        {
            var tV0 = new DenseMatrix(tX1.Values.Length, 1, 0.0);
            var tYk = new DenseMatrix(tX1.Values.Length, 1, 0.0);

            for (var i = 0; i <= tX1.Values.Length - _startingPoint - 1; i++)
            {
                var tFiKt = CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, i + _startingPoint, tX1, tYk, tV0);
                var tThetaKy1 = _theta.Transpose() * tFiKt;
                tYk[i + _startingPoint, 0] = tThetaKy1[0, 0];
            }

            return tYk;
        }

        public DenseMatrix Multiply(DenseMatrix m1, DenseMatrix m2)
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
            while (true)
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
