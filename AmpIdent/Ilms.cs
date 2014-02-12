using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent
{
    public class Ilms
    {
        //private
        //estimation parameters
        private int _numberOfIterations;
        private double _acceptableError;
        private int _estimationLength;

        //status
        private int _estimationStatusPercentage;
        private Boolean _estimationDone;
        private double _estimationError;
        private double _estimationDifference;
        private string _statusString;

        //matrixes
        private readonly Multiplicator _multiplicator;
        private readonly Armax _armax;
        private readonly FiCalculator _fiCalculator;

        //public
        //model estimation parameters
        public int NumberOfIterations
        {
            get { return _numberOfIterations; }
            set { _numberOfIterations = value; }
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
        public string StatusString
        {
            get { return _statusString; }
        }
        

        //Methods
        //Constructor
        public Ilms(Armax armax)
        {
            _estimationStatusPercentage = 0;
            _estimationDone = false;
            _statusString = "Initialized";

            _armax = armax;
            _numberOfIterations = 20;
            _acceptableError = 1.0E-12;
            _estimationLength = 500;
            _armax.ModelShift = 0;

            _multiplicator = new Multiplicator();
            _fiCalculator = new FiCalculator();
        }

        public void Compute(DenseMatrix x1, DenseMatrix y1, int estimationLength)
        {
            var random = new Random();
            //this._y1 = Y1;

            //Setting preparation.....................................................................................................
            _estimationError = 0.0;
            _estimationStatusPercentage = 0;
            if (estimationLength == 0) _estimationLength = x1.Values.Length - _armax.StartingPoint;
            else _estimationLength = estimationLength;

            //I Step: Y(L)............................................................................................................

            var yl = new DenseMatrix(_estimationLength, 1, 0.0);
            for (var i = 0; i <= _estimationLength - 1; i++) { yl[i, 0] = y1[i + _armax.StartingPoint, 0]; }
            _statusString = "Step I: DONE";

            //II Step: V0.............................................................................................................

            _armax.V0 = new DenseMatrix(x1.Values.Length, 1, 1.0); //
            for (var i = 0; i <= x1.Values.Length - 1; i++) {
                _armax.V0[i, 0] = random.Next(-100, 100);
            }
            _statusString = "Step II: DONE";

            //Definition of temporary matrixes
            var thetaK1 = new DenseMatrix(_armax.NaParameter + _armax.NbParameter + _armax.NdParameter, 1, 0.0);
            for (var i = 0; i <= _armax.NaParameter + _armax.NbParameter + _armax.NdParameter - 1; i++) { thetaK1[i, 0] = 0.0; }
            var Fi_k = new DenseMatrix(_armax.NaParameter + _armax.NbParameter + _armax.NdParameter, 1, 0.0);
            var Fi_k_L = new DenseMatrix(_armax.NaParameter + _armax.NbParameter + _armax.NdParameter, _estimationLength, 0.0);
            var Fi_k_t = new DenseMatrix(_armax.NaParameter + _armax.NbParameter + _armax.NdParameter, 1, 0.0);
            var Vk_t = new DenseMatrix(_estimationLength + (_numberOfIterations + 1) * _armax.StartingPoint, 1, 1.0);
            var ThetaDiff = new DenseMatrix(_armax.NaParameter + _armax.NbParameter + _armax.NdParameter, 1, 0.0);

            //Predefinition of results
            _armax.Theta = new DenseMatrix(_armax.NaParameter + _armax.NbParameter + _armax.NdParameter, 1, 0.0);
            _armax.Yk = new DenseMatrix(x1.Values.Length, 1, 0.0);

            for (var k = 1; k <= _numberOfIterations; k++)
            {
                //III Step: Fi_k(_estimationLength)...................................................................................

                for (var t = _armax.StartingPoint; t <= _armax.StartingPoint + _estimationLength - 1; t++)
                {
                    Fi_k = _fiCalculator.CalculateFi_k(_armax.NaParameter, _armax.NbParameter, _armax.NdParameter, _armax.NkParameter, t, _armax.ModelShift, x1, y1, _armax.V0);
                    for (var i = 0; i <= _armax.NaParameter + _armax.NbParameter + _armax.NdParameter - 1; i++)
                    {
                        Fi_k_L[i, t - _armax.StartingPoint] = Fi_k[i, 0];
                    }
                }
                _statusString = "Step III: DONE";

                //IV Step: Theta(k)...................................................................................................

                var fiKLt = Fi_k_L.Transpose();
                _statusString = "Step IV: 1/5 DONE";
                var fiFiT = _multiplicator.Multiply(Fi_k_L, (DenseMatrix)fiKLt);
                _statusString = "Step IV: 2/5 DONE";
                var fiFiTi = fiFiT.Inverse();
                _statusString = "Step IV: 3/5 DONE";
                var fiFiTiFiKl = _multiplicator.Multiply((DenseMatrix)fiFiTi, Fi_k_L);
                _statusString = "Step IV: 4/5 DONE";
                var thetaK = fiFiTiFiKl * yl;
                _statusString = "Step IV: 5/5 DONE";

                //V Step: Vk_t........................................................................................................

                for (var i = 0; i <=_estimationLength - 1; i++)
                {
                    Fi_k_t = _fiCalculator.CalculateFi_k(_armax.NaParameter, _armax.NbParameter, _armax.NdParameter, _armax.NkParameter, i + _armax.StartingPoint, _armax.ModelShift, x1, y1, _armax.V0);
                    var thetaKy1 = thetaK.Transpose() * Fi_k_t;
                    Vk_t[i, 0] = y1[i + _armax.StartingPoint, 0] - thetaKy1[0, 0];
                }
                _statusString = "Step V: DONE";

                //VI Step: Average Error..............................................................................................

                for (var i = 0; i <= _armax.NaParameter + _armax.NbParameter + _armax.NdParameter - 1; i++)
                {
                    ThetaDiff[i, 0] = Math.Abs(thetaK[i, 0] - thetaK1[i, 0]);
                }
                _estimationError = 0.0;
                for (var i = 0; i <= _armax.NaParameter + _armax.NbParameter + _armax.NdParameter - 1; i++)
                {
                    _estimationError = _estimationError + ThetaDiff[i, 0];
                }
                _estimationDifference = 0.0;
                for (var i = 0; i <= _estimationLength - 1; i++)
                {
                    _estimationDifference += Vk_t[i, 0];
                }
                _estimationDifference = _estimationDifference / _estimationLength;
                _estimationError = _estimationError / (_armax.NaParameter + _armax.NbParameter + _armax.NdParameter);
                _statusString = k + @" iteration: VI Step: Estimation Error " + @" : " + _estimationError +
                                @"; Estimation Difference : " + _estimationDifference;
                _statusString = "Step VI: DONE";

                //VII Step: Decision..................................................................................................

                if (_estimationError != _estimationError) k = _numberOfIterations;
                else
                {
                    _armax.V0 = Vk_t;
                    for (var i = 0; i <= _armax.NaParameter + _armax.NbParameter + _armax.NdParameter - 1; i++)
                    {
                        thetaK1[i, 0] = thetaK[i, 0];
                        _estimationStatusPercentage = k * 100 / _numberOfIterations;
                    }
                    if (_estimationError < _acceptableError) k = _numberOfIterations;
                }
                _statusString = "Step VII: DONE";
            }

            //END: Vk.................................................................................................................
            _armax.Theta = thetaK1;
            _armax.Yk = _armax.Model(x1);
            _statusString = "Estimation: DONE";
            
            _estimationDone = true;
        }
    }
}
