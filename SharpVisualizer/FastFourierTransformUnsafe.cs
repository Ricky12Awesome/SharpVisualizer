using System;
using NAudio.Dsp;

namespace SharpVisualizer
{
    public static unsafe class FastFourierTransformUnsafe
    {
        public static void FFT(bool forward, int m, Complex* data)
        {
            var num1 = 1;
            for (var index = 0; index < m; ++index)
                num1 *= 2;
            var num2 = num1 >> 1;
            var index1 = 0;
            for (var index2 = 0; index2 < num1 - 1; ++index2)
            {
                if (index2 < index1)
                {
                    var x = data[index2].X;
                    var y = data[index2].Y;
                    data[index2].X = data[index1].X;
                    data[index2].Y = data[index1].Y;
                    data[index1].X = x;
                    data[index1].Y = y;
                }

                int num3;
                for (num3 = num2; num3 <= index1; num3 >>= 1)
                    index1 -= num3;
                index1 += num3;
            }

            var num4 = -1f;
            var num5 = 0.0f;
            var num6 = 1;
            for (var index2 = 0; index2 < m; ++index2)
            {
                var num3 = num6;
                num6 <<= 1;
                var num7 = 1f;
                var num8 = 0.0f;
                for (var index3 = 0; index3 < num3; ++index3)
                {
                    for (var index4 = index3; index4 < num1; index4 += num6)
                    {
                        var index5 = index4 + num3;
                        var num9 = (float) (num7 * (double) data[index5].X -
                                            num8 * (double) data[index5].Y);
                        var num10 = (float) (num7 * (double) data[index5].Y +
                                             num8 * (double) data[index5].X);
                        data[index5].X = data[index4].X - num9;
                        data[index5].Y = data[index4].Y - num10;
                        data[index4].X += num9;
                        data[index4].Y += num10;
                    }

                    var num11 = (float) (num7 * (double) num4 - num8 * (double) num5);
                    num8 = (float) (num7 * (double) num5 + num8 * (double) num4);
                    num7 = num11;
                }

                num5 = (float) Math.Sqrt((1.0 - num4) / 2.0);
                if (forward)
                    num5 = -num5;
                num4 = (float) Math.Sqrt((1.0 + num4) / 2.0);
            }

            if (!forward)
                return;
            for (var index2 = 0; index2 < num1; ++index2)
            {
                data[index2].X /= num1;
                data[index2].Y /= num1;
            }
        }
    }
}