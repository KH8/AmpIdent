using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent
{
    public class Armax
    {
        //private
        //matrixes
        private DenseMatrix _v0;
        private DenseMatrix _theta;
        private DenseMatrix _yk;

        //parameters
        private int _naParameter;
        private int _nbParameter;
        private int _ndParameter;
        private int _nkParameter;
        private int _modelShift;
        private int _startingPoint;

        //auxiliaries
        private readonly FiCalculator _fiCalculator;
        //status
        private string _statusString;

        //public
        //constructor
        public Armax()
        {
            _naParameter = 10;
            _nbParameter = 10;
            _ndParameter = 0;
            _nkParameter = 0;
            _fiCalculator = new FiCalculator();
        }

        //parameters
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

        public int ModelShift
        {
            get { return _modelShift; }
            set { _modelShift = value; }
        }

        public int StartingPoint
        {
            get { return _startingPoint; }
            set { _startingPoint = value; }
        }

        //matrixes
        public DenseMatrix V0
        {
            get { return _v0; }
            set { _v0 = value; }
        }

        public DenseMatrix Theta
        {
            get { return _theta; }
            set { _theta = value; }
        }

        public DenseMatrix Yk
        {
            get { return _yk; }
            set { _yk = value; }
        }

        //status
        public string StatusString
        {
            get { return _statusString; }
        }

        //methods
        public DenseMatrix Model(DenseMatrix tX1)
        {
            var tV0 = new DenseMatrix(tX1.Values.Length, 1, 0.0);
            var tYk = new DenseMatrix(tX1.Values.Length, 1, 0.0);

            for (var i = 0; i <= tX1.Values.Length - _startingPoint - 1; i++)
            {
                var tFiKt = _fiCalculator.CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, i + _startingPoint, _modelShift, tX1, tYk, tV0);
                var tThetaKy1 = _theta.Transpose() * tFiKt;
                tYk[i + _startingPoint, 0] = tThetaKy1[0, 0];
            }
            _statusString = "Output Creation: DONE";
            return tYk;
        }
    }
}