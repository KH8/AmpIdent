using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class Armax
    {
        //private
        //matrixes
        private DenseMatrix _theta;

        //parameters
        private int _naParameter;
        private int _nbParameter;
        private int _ndParameter;
        private int _nkParameter;
        private int _modelShift;
        private int _startingPoint;
        private int _fixedLength;

        //auxiliaries
        private readonly FiCalculator _fiCalculator;
        //status
        private string _statusString;

        //matrixes
        private DenseMatrix _matrixX;
        private DenseMatrix _matrixY;

        //public
        //constructor
        public Armax()
        {
            _naParameter = 10;
            _nbParameter = 10;
            _ndParameter = 0;
            _nkParameter = 0;
            _modelShift = 0;
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
        public DenseMatrix V0 { get; set; }

        public DenseMatrix Theta
        {
            get { return _theta; }
            set { _theta = value; }
        }

        public DenseMatrix Yk { get; set; }

        public DenseMatrix MatrixX
        {
            get { return _matrixX; }
            set { _matrixX = value; }
        }

        public DenseMatrix MatrixY
        {
            get { return _matrixY; }
            set { _matrixY = value; }
        }

        //status
        public string StatusString
        {
            get { return _statusString; }
        }

        //methods
        public DenseMatrix Model(DenseMatrix tX1)
        {
            var tV0 = new DenseMatrix(tX1.Values.Length + _fixedLength, 1, 0.0);
            var iYk = new DenseMatrix(tX1.Values.Length, 1, 0.0);
            var tYk = VectorLinker.LinkMatrix(_matrixY, iYk);
            var tXk = VectorLinker.LinkMatrix(_matrixX, tX1);

            for (var i = 0; i <= tX1.Values.Length - _startingPoint - 1; i++)
            {
                var tFiKt = _fiCalculator.CalculateFi_k(_naParameter, _nbParameter, _ndParameter, _nkParameter, i + _startingPoint, _modelShift, tX1, tYk, tV0);
                var thetaT = _theta.Transpose();
                var tThetaKy1 = thetaT*tFiKt;
                tYk[i + _startingPoint, 0] = tThetaKy1[0, 0];
            }
            _statusString = "Output Creation: DONE";
            return tYk;
        }

        private DenseMatrix CreateStartMatrix(DenseMatrix matrix)
        {
            var startMatrix = new DenseMatrix(_fixedLength, 1, 0.0);

            for (int i = 0; i < _fixedLength; i++)
            {
                startMatrix[i, 0] = matrix[i, 0];
            }

            return startMatrix;
        }

        public void CreateStartMatrixX(DenseMatrix matrix)
        {
            _fixedLength = 10000 + _startingPoint;

            _matrixX = new DenseMatrix(_fixedLength, 1, 0.0);
            _matrixX = CreateStartMatrix(matrix);
        }

        public void CreateStartMatrixY(DenseMatrix matrix)
        {
            _fixedLength = 10000 + _startingPoint;

            _matrixY = new DenseMatrix(_fixedLength, 1, 0.0);
            _matrixY = CreateStartMatrix(matrix);
        }
    }
}