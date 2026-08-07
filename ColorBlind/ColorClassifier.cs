using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace ColorBlind
{
    public class ColorClassifier
    {
        // convert RGB to HSV for color classification
        private void RGBtoHSV(Color color, out double hue, out double saturation, out double value)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            value = max;

            double difference = max - min;

            if (max == 0)
            {
                saturation = 0;
            }
            else
            {
                saturation = difference / max;
            }

            if (difference == 0)
            {
                hue = 0;
            }
            else if (max == r)
            {
                hue = (60 * ((g - b) / difference) + 360) % 360;
            }
            else if (max == g)
            {
                hue = 60 * ((b - r) / difference) + 120;
            }
            else
            {
                hue = 60 * ((r - g) / difference) + 240;
            }
        }

        public string GetColorName(Color color)
        {
            double hue;
            double saturation;
            double value;

            RGBtoHSV(color, out hue, out saturation, out value);

            // handle grays first
            if (saturation < 0.15)
            {
                if (value > 0.85)
                    return "White";

                if (value > 0.65)
                    return "Light Gray";

                if (value > 0.35)
                    return "Gray";

                return "Black";
            }


            string name;

            if (hue < 15 || hue >= 345)
                name = "Red";
            else if (hue < 45)
                name = "Orange";
            else if (hue < 70)
                name = "Yellow";
            else if (hue < 170)
                name = "Green";
            else if (hue < 260)
                name = "Blue";
            else if (hue < 290)
                name = "Purple";
            else
                name = "Pink";


            // brightness modifier
            if (value < 0.35)
                return "Dark " + name;

            if (value > 0.75 && saturation < 0.5)
                return "Light " + name;

            return name;

            return name;
        }
    }
}
