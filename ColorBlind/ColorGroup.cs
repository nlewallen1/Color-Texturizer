using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace ColorBlind
{
    public class ColorGroup
    {
        public Color Color { get; set; }
        public List<Point> Pixels { get; set; } = new List<Point>();

    }
}
