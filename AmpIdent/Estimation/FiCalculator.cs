using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Estimation
{
    public class FiCalculator
    {
        public DenseMatrix CalculateFi_k(int tNa, int tNb, int tNd, int tNk, int tT, int modelShift, DenseMatrix tX, DenseMatrix tY, DenseMatrix tV)
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
                    tFiK[i - 1, 0] = tX[tT - tNk - i + tNa + modelShift, 0];
                }
                if (i > tNa + tNb && i <= tNa + tNb + tNd)
                {
                    tFiK[i - 1, 0] = tV[tT - i + tNa + tNb, 0];
                }
            }
            return tFiK;
        }
    }
}