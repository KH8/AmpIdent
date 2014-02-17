using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class RecursiveLeastSquares : LeastSquares
    {
        //private
        //estimation parameters
        private int _recurenceLength;
        private double _lambda;
        private double _delta;

        //status
        private int _estimationStatusPercentage;

        //matrixes
        private readonly Armax _modelArmax;
        private readonly FiCalculator _fiCalculator;

        //public
        //model estimation parameters
        public int RecurenceLength
        {
            get { return _recurenceLength; }
            set { _recurenceLength = value; }
        }

        public double Lambda
        {
            get { return _lambda; }
            set { _lambda = value; }
        }

        public double Delta
        {
            get { return _delta; }
            set { _delta = value; }
        }

        //Methods
        //Constructor
        public RecursiveLeastSquares(Armax armax) : base(armax)
        {
            _estimationStatusPercentage = 0;

            _lambda = 0.99;
            _delta = 1.0E+100;
            _modelArmax = armax;
            RecurenceLength = 1000;

            _fiCalculator = new FiCalculator();
        }

        public void ComputeRls(DenseMatrix x1, DenseMatrix y1, int estimationLength, int recurenceLength)
        {
            Compute(x1, y1, estimationLength);
            _estimationStatusPercentage = 0;
            if (recurenceLength == 0) _recurenceLength = _modelArmax.V0.Values.Length - _modelArmax.StartingPoint;
            else _recurenceLength = recurenceLength;

            //V Step: RLS preparation.................................................................................................

            DenseMatrix pN1 = DenseMatrix.Identity(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter);
            pN1 = _delta*pN1;
            var thetaN1 = _modelArmax.Theta;
            _statusString = "RLS: Step V: DONE";

            //VI Step: computation....................................................................................................

            for (var r = _modelArmax.StartingPoint; r <= _recurenceLength - 1; r++)
            {
                var fiN = _fiCalculator.CalculateFi_k(_modelArmax.NaParameter, _modelArmax.NbParameter, _modelArmax.NdParameter, _modelArmax.NkParameter, r, _modelArmax.ModelShift, x1, y1, _modelArmax.V0);
                var yn = fiN.Transpose() * thetaN1;
                double alphaN = y1[r, 0] - yn[0, 0];

                //calculation of K(n-1)
                var pN1FiN = pN1*fiN;
                var fiNt = fiN.Transpose();
                var fiNtpN1FiN = fiNt*pN1FiN;
                double lFiNtpN1FiN = 10 * (_lambda + fiNtpN1FiN[0, 0]);
                var kN1 = pN1FiN.Divide(lFiNtpN1FiN);

                //calculation of P(n)
                var kN1FiNt = kN1*fiNt;
                DenseMatrix iMatrix = DenseMatrix.Identity(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter);
                var ikN1FiNt = iMatrix - kN1FiNt;
                var pNlambda = pN1.Divide(_lambda);
                var pN = ikN1FiNt*pNlambda;

                //calculation of thetaN
                var alphaNkN = alphaN * kN1;
                var thetaN = thetaN1 + alphaNkN;
                 
                //memory
                pN1 = (DenseMatrix) pN;
                thetaN1 = (DenseMatrix)thetaN;

                _estimationStatusPercentage = r*100/_recurenceLength;
                _statusString = "RLS: Step VI: " + _estimationStatusPercentage +"%";
            }
            _statusString = "RLS: Step VI: DONE";

            //END: Vk.................................................................................................................
            
            _modelArmax.Theta = thetaN1;
            _modelArmax.Yk = _modelArmax.Model(x1);
            _statusString = "RLS: Estimation: DONE";
            
            _estimationDone = true;
        }
    }
}
