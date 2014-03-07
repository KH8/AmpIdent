using System;
using AmpIdent.Auxiliaries;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class LeastSquares
    {
        //private
        //estimation parameters
        private int _numberOfIterations;
        private double _acceptableError;
        private int _estimationLength;

        //status
        private int _estimationStatusPercentage;
        internal Boolean _estimationDone;
        private double _estimationError;
        private double _estimationDifference;
        internal string _statusString;

        //matrixes
        private readonly Armax _modelArmax;
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
        public LeastSquares(Armax armax)
        {
            _estimationStatusPercentage = 0;
            _estimationDone = false;
            _statusString = "Iterative LS: Initialized";

            _modelArmax = armax;
            _numberOfIterations = 20;
            _acceptableError = 1.0E-12;
            _estimationLength = 500;

            _fiCalculator = new FiCalculator();
        }

        public void Compute(DenseMatrix x1, DenseMatrix y1, int estimationLength)
        {
            var random = new Random();
            //this._y1 = Y1;

            //Setting preparation.....................................................................................................
            _estimationError = 0.0;
            _estimationStatusPercentage = 0;
            if (estimationLength == 0) _estimationLength = x1.Values.Length - _modelArmax.StartingPoint;
            else _estimationLength = estimationLength;

            //I Step: Y(L)............................................................................................................

            var yl = new DenseMatrix(_estimationLength, 1, 0.0);
            for (var i = 0; i <= _estimationLength - 1; i++) { yl[i, 0] = y1[i + _modelArmax.StartingPoint, 0]; }
            _statusString = "Iterative LS: Step I: DONE";

            //II Step: V0.............................................................................................................

            _modelArmax.V0 = new DenseMatrix(x1.Values.Length, 1, 1.0); //
            for (var i = 0; i <= x1.Values.Length - 1; i++) {
                _modelArmax.V0[i, 0] = random.Next(-100, 100);
            }
            _statusString = "Iterative LS: Step II: DONE";

            //Definition of temporary matrixes
            var thetaK1 = new DenseMatrix(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter, 1, 0.0);
            var fiKl = new DenseMatrix(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter, _estimationLength, 0.0);
            var vkT = new DenseMatrix(_estimationLength + (_numberOfIterations + 1) * _modelArmax.StartingPoint, 1, 1.0);
            var thetaDiff = new DenseMatrix(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter, 1, 0.0);

            //Predefinition of results
            _modelArmax.Theta = new DenseMatrix(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter, 1, 0.0);
            _modelArmax.Yk = new DenseMatrix(x1.Values.Length, 1, 0.0);

            for (var k = 1; k <= _numberOfIterations; k++)
            {
                //III Step: Fi_k(_estimationLength)...................................................................................

                for (var t = _modelArmax.StartingPoint; t <= _modelArmax.StartingPoint + _estimationLength - 1; t++)
                {
                    var fik = _fiCalculator.CalculateFi_k(_modelArmax.NaParameter, _modelArmax.NbParameter, _modelArmax.NdParameter, _modelArmax.NkParameter, t, _modelArmax.ModelShift, x1, y1, _modelArmax.V0);
                    for (var i = 0; i <= _modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter - 1; i++)
                    {
                        fiKl[i, t - _modelArmax.StartingPoint] = fik[i, 0];
                    }
                }
                _statusString = "Iteration " + k + " LS: Step III: DONE";

                //IV Step: Theta(k)...................................................................................................

                var fiKLt = fiKl.Transpose();
                _statusString = "Iteration " + k +" LS: Step IV: 1/5 DONE";
                var fiFiT = MultiplicatorCuda.Multiply(fiKl, (DenseMatrix)fiKLt);
                _statusString = "Iteration " + k + " LS: Step IV: 2/5 DONE";
                var fiFiTi = fiFiT.Inverse();
                _statusString = "Iteration " + k + " LS: Step IV: 3/5 DONE";
                var fiFiTiFiKl = MultiplicatorCuda.Multiply((DenseMatrix)fiFiTi, fiKl);
                _statusString = "Iteration " + k + " LS: Step IV: 4/5 DONE";
                var thetaK = fiFiTiFiKl * yl;
                _statusString = "Iteration " + k + " LS: Step IV: 5/5 DONE";

                //V Step: Vk_t........................................................................................................

                for (var i = 0; i <=_estimationLength - 1; i++)
                {
                    var fikt = _fiCalculator.CalculateFi_k(_modelArmax.NaParameter, _modelArmax.NbParameter, _modelArmax.NdParameter, _modelArmax.NkParameter, i + _modelArmax.StartingPoint, _modelArmax.ModelShift, x1, y1, _modelArmax.V0);
                    var thetaKy1 = thetaK.Transpose() * fikt;
                    vkT[i, 0] = y1[i + _modelArmax.StartingPoint, 0] - thetaKy1[0, 0];
                }
                _statusString = "Iteration " + k + " LS: Step V: DONE";

                //VI Step: Average Error..............................................................................................

                for (var i = 0; i <= _modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter - 1; i++)
                {
                    thetaDiff[i, 0] = Math.Abs(thetaK[i, 0] - thetaK1[i, 0]);
                }
                _estimationError = 0.0;
                for (var i = 0; i <= _modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter - 1; i++)
                {
                    _estimationError = _estimationError + thetaDiff[i, 0];
                }
                _estimationDifference = 0.0;
                for (var i = 0; i <= _estimationLength - 1; i++)
                {
                    _estimationDifference += vkT[i, 0];
                }
                _estimationDifference = _estimationDifference / _estimationLength;
                _estimationError = _estimationError / (_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter);
                _statusString = "Iteration " + k + " LS: Step VI: Error: " + _estimationError +
                                @"; Diff: " + _estimationDifference;

                //VII Step: Decision..................................................................................................

                if (_estimationError != _estimationError) k = _numberOfIterations;
                else
                {
                    _modelArmax.V0 = vkT;
                    for (var i = 0; i <= _modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter - 1; i++)
                    {
                        thetaK1[i, 0] = thetaK[i, 0];
                        _estimationStatusPercentage = k * 100 / _numberOfIterations;
                    }
                    if (_estimationError < _acceptableError) k = _numberOfIterations;
                }
                _statusString = "Iteration " + k + " LS: Step VII: DONE";
            }

            //END: Vk.................................................................................................................
            if (_estimationLength - _modelArmax.StartingPoint < 10000)
            {
                _modelArmax.Offset = _estimationLength - _modelArmax.StartingPoint;
            }
            _modelArmax.CreateStartMatrixX(x1);
            _modelArmax.CreateStartMatrixY(y1);
            
            _modelArmax.Theta = thetaK1;
            _modelArmax.Yk = _modelArmax.Model(x1, true);
            _statusString = "Iterative LS: Estimation: DONE";
            
            _estimationDone = true;
        }
    }
}
