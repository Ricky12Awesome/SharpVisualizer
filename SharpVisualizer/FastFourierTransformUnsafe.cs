using System;
using NAudio.Dsp;

namespace Raygui
{
    public static unsafe class FastFourierTransformUnsafe
    {
        /// <summary>
        ///     This computes an in-place complex-to-complex FFT
        ///     x and y are the real and imaginary arrays of 2^m points.
        ///     <para />
        ///     Copied from
        ///     <a href="https://github.com/naudio/NAudio/blob/fb35ce8367f30b8bc5ea84e7d2529e172cf4c381/NAudio.Core/Dsp/FastFourierTransform.csNAudio.Core/Dsp/FastFourierTransform.cs#L10-L89">
        ///       github.com/naudio/NAudio/NAudio.Core/Dsp/FastFourierTransform.cs
        ///     </a>
        ///     <para />
        ///     with the change of using <c>Complex*</c> for data
        /// </summary>
        public static void FFT(bool forward, int m, Complex* data)
        {
            int n, i, i1, j, k, i2, l, l1, l2;
            float c1, c2, tx, ty, t1, t2, u1, u2, z;

            // Calculate the number of points
            n = 1;
            for (i = 0; i < m; i++)
                n *= 2;

            // Do the bit reversal
            i2 = n >> 1;
            j = 0;
            for (i = 0; i < n - 1; i++)
            {
                if (i < j)
                {
                    tx = data[i].X;
                    ty = data[i].Y;
                    data[i].X = data[j].X;
                    data[i].Y = data[j].Y;
                    data[j].X = tx;
                    data[j].Y = ty;
                }

                k = i2;

                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }

                j += k;
            }

            // Compute the FFT 
            c1 = -1.0f;
            c2 = 0.0f;
            l2 = 1;
            for (l = 0; l < m; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0f;
                u2 = 0.0f;
                for (j = 0; j < l1; j++)
                {
                    for (i = j; i < n; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * data[i1].X - u2 * data[i1].Y;
                        t2 = u1 * data[i1].Y + u2 * data[i1].X;
                        data[i1].X = data[i].X - t1;
                        data[i1].Y = data[i].Y - t2;
                        data[i].X += t1;
                        data[i].Y += t2;
                    }

                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }

                c2 = (float) Math.Sqrt((1.0f - c1) / 2.0f);
                if (forward)
                    c2 = -c2;
                c1 = (float) Math.Sqrt((1.0f + c1) / 2.0f);
            }

            // Scaling for forward transform 
            if (!forward) return;
            
            for (i = 0; i < n; i++)
            {
                data[i].X /= n;
                data[i].Y /= n;
            }
        }
    }
}