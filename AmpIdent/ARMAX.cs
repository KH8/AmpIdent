using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent
{
    class ARMAX
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

        //matrixes
        private DenseMatrix _Y1;
        private DenseMatrix _V0;
        private DenseMatrix _Theta;
        private DenseMatrix _YK;

        //public
        //model size parameters
        public int NAParameter
        {
            get { return _naParameter; }
            set { _naParameter = value; }
        }
        public int NBParameter
        {
            get { return _nbParameter; }
            set { _nbParameter = value; }
        }
        public int NDParameter
        {
            get { return _ndParameter; }
            set { _ndParameter = value; }
        }
        public int NKParameter
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
                    return _V0;
                }
                else return null;
            }
        }
        public DenseMatrix Theta
        {
            get
            {
                if (_estimationDone)
                {
                    return _Theta;
                }
                else return null;
            }
        }
        public DenseMatrix YK
        {
            get
            {
                if (_estimationDone)
                {
                    return _YK;
                }
                else return null;
            }
        }
        ///

        //Methods
        //Constructor
        public ARMAX()
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
        }

        public void Compute(DenseMatrix X1, DenseMatrix Y1, int estimationLength)
        {
            Random random = new Random();
            _Y1 = Y1;

            //Setting preparation.....................................................................................................
            _estimationError = 0.0;
            _estimationStatusPercentage = 0;
            if (estimationLength == 0) _estimationLength = X1.Values.Length - _startingPoint;
            else _estimationLength = estimationLength;

            //I Step: Y(L)............................................................................................................

            var YL = new DenseMatrix(_estimationLength, 1, 0.0);
            for (int i = 0; i <= _estimationLength - 1; i++) { YL[i, 0] = Y1[i + _startingPoint, 0]; }
            Console.WriteLine("Step I: DONE");

            //II Step: V0.............................................................................................................

            _V0 = new DenseMatrix(X1.Values.Length, 1, 1.0); //
            for (int i = 0; i <= X1.Values.Length - 1; i++) { _V0[i, 0] = random.Next(-100, 100); }
            Console.WriteLine("Step II: DONE");

            //Definition of temporary matrixes
            var Theta_k_1 = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            for (int i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++) { Theta_k_1[i, 0] = 0.0; }
            var Fi_k = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            var Fi_k_L = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, _estimationLength, 0.0);
            var Fi_k_t = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            var Vk_t = new DenseMatrix(_estimationLength + (_numberOfIterations + 1) * _startingPoint, 1, 1.0);
            var ThetaDiff = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);

            //Predefinition of results
            _Theta = new DenseMatrix(_naParameter + _nbParameter + _ndParameter, 1, 0.0);
            _YK = new DenseMatrix(X1.Values.Length, 1, 0.0);

            for (int k = 1; k <= _numberOfIterations; k++)
            {
                //III Step: Fi_k(_estimationLength)...................................................................................

                for (int t = _startingPoint; t <= _startingPoint + _estimationLength - 1; t++)
                {
                    Fi_k = CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, t, X1, Y1, _V0);
                    for (int i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                    {
                        Fi_k_L[i, t - _startingPoint] = Fi_k[i, 0];
                    }
                }
                Console.WriteLine("Step III: DONE");

                //IV Step: Theta(k)...................................................................................................

                var Fi_k_LT = Fi_k_L.Transpose();
                Console.WriteLine("Step IV: 1/5 DONE");
                var FiFiT = Fi_k_L.Multiply(Fi_k_LT);
                Console.WriteLine("Step IV: 2/5 DONE");
                var FiFiTI = FiFiT.Inverse();
                Console.WriteLine("Step IV: 3/5 DONE");
                var FiFiTIFi_k_L = FiFiTI.Multiply(Fi_k_L);
                Console.WriteLine("Step IV: 4/5 DONE");
                var Theta_k = FiFiTIFi_k_L * YL;
                Console.WriteLine("Step IV: 5/5 DONE");

                //V Step: Vk_t........................................................................................................

                for (int i = 0; i <=_estimationLength - 1; i++)
                {
                    Fi_k_t = CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, i + _startingPoint, X1, Y1, _V0);
                    var Theta_k_Y_1 = Theta_k.Transpose() * Fi_k_t;
                    Vk_t[i, 0] = Y1[i + _startingPoint, 0] - Theta_k_Y_1[0, 0];
                }
                Console.WriteLine("Step V: DONE");

                //VI Step: Average Error..............................................................................................

                for (int i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                {
                    ThetaDiff[i, 0] = Math.Abs(Theta_k[i, 0] - Theta_k_1[i, 0]);
                }
                _estimationError = 0.0;
                for (int i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                {
                    _estimationError = _estimationError + ThetaDiff[i, 0];
                }
                _estimationDifference = 0.0;
                for (int i = 0; i <= _estimationLength - 1; i++)
                {
                    _estimationDifference += Vk_t[i, 0];
                }
                _estimationDifference = _estimationDifference / _estimationLength;
                _estimationError = _estimationError / (_naParameter + _nbParameter + _ndParameter);
                Console.WriteLine(k.ToString() + " iteration: VI Step: Estimation Error " + " : " + _estimationError.ToString() + "; Estimation Difference : " + _estimationDifference.ToString());
                Console.WriteLine("Step VI: DONE");

                //VII Step: Decision..................................................................................................

                if (_estimationError != _estimationError) k = _numberOfIterations;
                else
                {
                    _V0 = Vk_t;
                    for (int i = 0; i <= _naParameter + _nbParameter + _ndParameter - 1; i++)
                    {
                        Theta_k_1[i, 0] = Theta_k[i, 0];
                        _estimationStatusPercentage = k * 100 / _numberOfIterations;
                    }
                    if (_estimationError < _acceptableError) k = _numberOfIterations;
                }
                Console.WriteLine("Step VII: DONE");
            }

            //END: Vk.................................................................................................................
            _Theta = Theta_k_1;
            _YK = Model(X1);
            Console.WriteLine("Estimation: DONE");
            
            _estimationDone = true;
        }

        public DenseMatrix CalculateFi_k(int t_na, int t_nb, int t_nd, int t_nk, int t_t, DenseMatrix t_X, DenseMatrix t_Y, DenseMatrix t_V)
        {
            var t_Fi_k = new DenseMatrix(t_na + t_nb + t_nd, 1, 0.0);
            for (int i = 1; i <= t_na + t_nb + t_nd; i++)
            {
                if (i <= t_na)
                {
                    t_Fi_k[i - 1, 0] = -1 * t_Y[t_t - i, 0];
                }
                if (i > t_na && i <= t_na + t_nb)
                {
                    t_Fi_k[i - 1, 0] = t_X[t_t - t_nk - i + t_na + _modelShift, 0];
                }
                if (i > t_na + t_nb && i <= t_na + t_nb + t_nd)
                {
                    t_Fi_k[i - 1, 0] = t_V[t_t - i + t_na + t_nb, 0];
                }
            }
            return t_Fi_k;
        }

        public DenseMatrix Model(DenseMatrix t_X1)
        {
            var t_V0 = new DenseMatrix(t_X1.Values.Length, 1, 0.0);
            var t_YK = new DenseMatrix(t_X1.Values.Length, 1, 0.0);

            for (int i = 0; i <= t_X1.Values.Length - _startingPoint - 1; i++)
            {
                var t_Fi_k_t = CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, i + _startingPoint, t_X1, t_YK, t_V0);
                var t_Theta_k_Y_1 = _Theta.Transpose() * t_Fi_k_t;
                t_YK[i + _startingPoint, 0] = t_Theta_k_Y_1[0, 0];
            }

            return t_YK;
        }
    }
}
