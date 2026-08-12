using System;
using System.Windows.Media;

namespace ColorBlind
{
    public class ColorIdentifier
    {
        // Converts RGB color to HSV color space
        private void RGBtoHSV(Color color, out double hue, out double saturation, out double value)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double difference = max - min;

            value = max;
            saturation = max == 0 ? 0 : difference / max;

            if (difference == 0)
                hue = 0;
            else if (max == r)
                hue = (60 * ((g - b) / difference) + 360) % 360;
            else if (max == g)
                hue = 60 * ((b - r) / difference) + 120;
            else
                hue = 60 * ((r - g) / difference) + 240;
        }

        // determine a name for the color
        public string GetColorName(Color color)
        {
            double hue, saturation, value;
            // work with HSV instead of RGB for better color categorization
            RGBtoHSV(color, out hue, out saturation, out value);

            // judge grayscales
            if (saturation < 0.12)
            {
                if (value > 0.92) return "White";
                if (value > 0.70) return "Light Gray";
                if (value > 0.30) return "Gray";
                if (value > 0.08) return "Dark Gray";
                return "Black";
            }

            // check for brown
            bool isBrownHueRange = hue >= 10 && hue < 50;
            if (isBrownHueRange && value < 0.65 && saturation > 0.25)
                return "Brown";

            // check for pink
            bool isPinkHueRange = hue >= 320 || hue < 15;
            if (isPinkHueRange && value > 0.75 && saturation < 0.55 && saturation > 0.08)
                return "Pink";

            // check for other colors based on hue
            string name;
            if (hue < 15 || hue >= 345) name = "Red";
            else if (hue < 40) name = "Orange";
            else if (hue < 65) name = "Yellow";
            else if (hue < 170) name = "Green";
            else if (hue < 195) name = "Cyan";
            else if (hue < 250) name = "Blue";
            else if (hue < 290) name = "Purple";
            else if (hue < 345) name = "Magenta";
            else name = "Red";

            // determine shade
            if (value < 0.25)
                return "Very Dark " + name;

            if (value < 0.45)
                return "Dark " + name;

            if (saturation < 0.25 && value > 0.5)
                return "Muted " + name;

            if (value > 0.85 && saturation < 0.5)
                return "Light " + name;

            return name;
        }
    }
}