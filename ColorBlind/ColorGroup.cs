using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorBlind
{
    // hold color groups
    public class ColorGroup
    {
        // color of the group
        public Color Color { get; set; }
        // pixel list of the group
        public List<Point> Pixels { get; set; } = new List<Point>();

    }

}
