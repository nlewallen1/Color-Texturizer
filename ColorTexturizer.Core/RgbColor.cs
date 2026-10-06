using System;
using System.Collections.Generic;
using System.Text;

namespace ColorTexturizer.Core
{
    public readonly record struct RgbColor(byte R, byte G, byte B, byte A = 255);
}
