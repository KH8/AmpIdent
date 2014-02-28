using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class MatrixConverter
    {
        public static float[] DenseMatrix2Floats(DenseMatrix m)
        {
            var mFloats = new float[m.RowCount * m.ColumnCount];

            for (int i = 0; i < m.RowCount; i++)
            {
                for (int j = 0; j < m.ColumnCount; j++)
                {
                    mFloats[m.ColumnCount * i + j] = (float)m[i, j];
                }
            }

            return mFloats;
        }

        public static DenseMatrix Floats2DenseMatrix(float[] mFloats, int columnCount, int rowCount)
        {
            var m = new DenseMatrix(rowCount, columnCount, 0.0);

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    m[i, j] = mFloats[columnCount*i + j];
                }
            }

            return m;
        }
    }
}