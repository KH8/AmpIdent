﻿using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class RecursiveLeasSquares
    {
        //private
        //estimation parameters
        private int _estimationLength;
        private int _recurenceLength;
        private double _lambda;
        private double _delta;

        //status
        private int _estimationStatusPercentage;
        private Boolean _estimationDone;
        private string _statusString;

        //matrixes
        private readonly Multiplicator _multiplicator;
        private readonly Armax _modelArmax;
        private readonly FiCalculator _fiCalculator;

        //public
        //model estimation parameters
        public int EstimationLength
        {
            get { return _estimationLength; }
            set { _estimationLength = value; }
        }

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

        //status
        public int EstimationPercentage
        {
            get { return _estimationStatusPercentage; }
        }

        public Boolean EstimationDone
        {
            get { return _estimationDone; }
        }

        public string StatusString
        {
            get { return _statusString; }
        }

        

        //Methods
        //Constructor
        public RecursiveLeasSquares(Armax armax)
        {
            _estimationStatusPercentage = 0;
            _estimationDone = false;
            _statusString = "RLS: Initialized";

            _lambda = 1.0E-20;
            _delta = 1.0E+30;
            _modelArmax = armax;
            _estimationLength = 500;
            RecurenceLength = 1000;

            _multiplicator = new Multiplicator();
            _fiCalculator = new FiCalculator();
        }

        public void Compute(DenseMatrix x1, DenseMatrix y1, int estimationLength, int recurenceLength)
        {
            var random = new Random();
            
            //Setting preparation.....................................................................................................
            _estimationStatusPercentage = 0;
            if (estimationLength == 0) _estimationLength = x1.Values.Length - _modelArmax.StartingPoint;
            else _estimationLength = estimationLength;
            if (recurenceLength == 0) _recurenceLength = x1.Values.Length - _modelArmax.StartingPoint;
            else _recurenceLength = recurenceLength;

            //I Step: Y(L)............................................................................................................

            var yl = new DenseMatrix(_estimationLength, 1, 0.0);
            for (var i = 0; i <= _estimationLength - 1; i++) { yl[i, 0] = y1[i + _modelArmax.StartingPoint, 0]; }
            _statusString = "RLS: Step I: DONE";

            //II Step: V0.............................................................................................................

            _modelArmax.V0 = new DenseMatrix(x1.Values.Length, 1, 1.0); //
            for (var i = 0; i <= x1.Values.Length - 1; i++) {
                _modelArmax.V0[i, 0] = random.Next(-100, 100);
            }
            _statusString = "RLS: Step II: DONE";

            //Definition of temporary matrixes
            var fiKl = new DenseMatrix(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter, _estimationLength, 0.0);

            //Predefinition of results
            _modelArmax.Theta = new DenseMatrix(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter, 1, 0.0);
            _modelArmax.Yk = new DenseMatrix(x1.Values.Length, 1, 0.0);

            //III Step: Fi_k(_estimationLength).......................................................................................

            for (var t = _modelArmax.StartingPoint; t <= _modelArmax.StartingPoint + _estimationLength - 1; t++)
            {
                var fik = _fiCalculator.CalculateFi_k(_modelArmax.NaParameter, _modelArmax.NbParameter, _modelArmax.NdParameter, _modelArmax.NkParameter, t, _modelArmax.ModelShift, x1, y1, _modelArmax.V0);
                for (var i = 0; i <= _modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter - 1; i++)
                {
                    fiKl[i, t - _modelArmax.StartingPoint] = fik[i, 0];
                }
            }
            _statusString = "RLS: Step III: DONE";

            //IV Step: Theta(k).......................................................................................................

            var fiKLt = fiKl.Transpose();
            _statusString = "RLS: Step IV: 1/5 DONE";
            var fiFiT = _multiplicator.Multiply(fiKl, (DenseMatrix)fiKLt);
            _statusString = "RLS: Step IV: 2/5 DONE";
            var fiFiTi = fiFiT.Inverse();
            _statusString = "RLS: Step IV: 3/5 DONE";
            var fiFiTiFiKl = _multiplicator.Multiply((DenseMatrix)fiFiTi, fiKl);
            _statusString = "RLS: Step IV: 4/5 DONE";
            var thetaN1 = fiFiTiFiKl * yl;
            _statusString = "RLS: Step IV: 5/5 DONE";

            //V Step: RLS preparation.................................................................................................

            DenseMatrix pN1 = DenseMatrix.Identity(_modelArmax.NaParameter + _modelArmax.NbParameter + _modelArmax.NdParameter);
            pN1 = _delta*pN1;
            _statusString = "RLS: Step V: DONE";

            //VI Step: computation....................................................................................................

            for (var i = _modelArmax.StartingPoint; i <= _recurenceLength - 1; i++)
            {
                var fiN = _fiCalculator.CalculateFi_k(_modelArmax.NaParameter, _modelArmax.NbParameter, _modelArmax.NdParameter, _modelArmax.NkParameter, i, _modelArmax.ModelShift, x1, y1, _modelArmax.V0);
                var yn = thetaN1.Transpose() * fiN;
                double alphaN = y1[i, 0] - yn[0, 0];

                //calculation of K(n-1)
                var pN1FiN = pN1*fiN;
                var fiNt = fiN.Transpose();
                var fiNtpN1FiN = fiNt*pN1FiN;
                double lFiNtpN1FiN = _lambda * fiNtpN1FiN[0, 0];
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
                thetaN1 = (DenseMatrix) thetaN;

                _estimationStatusPercentage = i*100/_recurenceLength;
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