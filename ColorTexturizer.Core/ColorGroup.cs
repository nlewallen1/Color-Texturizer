namespace ColorTexturizer.Core
{
    // hold color groups
    public class ColorGroup
    {
        // color of the group
        public RgbColor RgbColor { get; set; }
        // pixel list of the group
        public List<PixelPoint> Pixels { get; set; } = new List<PixelPoint>();

    }

}
