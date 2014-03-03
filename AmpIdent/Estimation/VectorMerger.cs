using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    class VectorMerger
    {
        public static DenseMatrix LinkMatrix(DenseMatrix aMatrix, DenseMatrix bMatrix)
        {
            var outputMatrix = new DenseMatrix(aMatrix.Values.Length + bMatrix.Values.Length, 1, 0.0);

            for (int i = 0; i < aMatrix.Values.Length; i++)
            {
                outputMatrix[i, 0] = aMatrix[i, 0];
            }
            for (int i = 0; i < bMatrix.Values.Length; i++)
            {
                outputMatrix[aMatrix.Values.Length + i, 0] = bMatrix[i, 0];
            }

            return outputMatrix;
        }
    }
}
