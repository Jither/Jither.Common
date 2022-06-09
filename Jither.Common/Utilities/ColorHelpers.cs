using System;
using System.Collections.Generic;

namespace Jither.Utilities;

public class ColorHelpers
{
    public static IReadOnlyList<int> GetRainbowGradientList(int divisions, double saturation, double lightness)
    {
        if (divisions <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisions));
        }
        var result = new List<int>();
        for (int i = 0; i < divisions; i++)
        {
            var hue = 360d * i / divisions;
            var (r, g, b) = HslToRgb(hue, saturation, lightness);
            result.Add((r << 16) | (g << 8) | b);
        }
        return result;
    }

    public static (byte r, byte g, byte b) HslToRgb(double hue, double saturation, double lightness)
    {
        if (saturation < 0 || saturation > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(saturation));
        }
        if (lightness < 0 || lightness > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lightness));
        }

        // Yes, modulo works for doubles too
        hue %= 360;
        if (hue < 0)
        {
            hue += 360;            
        }

        double v;
        double r, g, b;

        r = lightness;
        g = lightness;
        b = lightness;

        v = (lightness <= 0.5) ? (lightness * (saturation + 1.0)) : (lightness + saturation - lightness * saturation);

        if (v > 0)
        {
            double m;
            double sv;
            int sextant;
            double fract, vsf, mid1, mid2;

            m = lightness + lightness - v;
            sv = (v - m) / v;
            hue /= 60d;

            sextant = (int)hue;
            fract = hue - sextant;
            vsf = v * sv * fract;

            mid1 = m + vsf;
            mid2 = v - vsf;

            switch (sextant)
            {
                case 0:
                    r = v;
                    g = mid1;
                    b = m;
                    break;

                case 1:
                    r = mid2;
                    g = v;
                    b = m;
                    break;

                case 2:
                    r = m;
                    g = v;
                    b = mid1;
                    break;

                case 3:
                    r = m;
                    g = mid2;
                    b = v;
                    break;

                case 4:
                    r = mid1;
                    g = m;
                    b = v;
                    break;

                case 5:
                    r = v;
                    g = m;
                    b = mid2;
                    break;
            }
        }

        return (
            Convert.ToByte(r * 255.0f),
            Convert.ToByte(g * 255.0f),
            Convert.ToByte(b * 255.0f)
        );
    }
}
