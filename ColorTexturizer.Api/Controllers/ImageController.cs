
using global::ColorTexturizer.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorTexturizer.Api.Controllers
{
    // handle API requests
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        // color data transfer object
        public class ColorInfoDto
        {
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
            public string Hex { get; set; }
            public string Name { get; set; }
            public int PixelCount { get; set; }
            public string TexturePath { get; set; }
        }

        // accepts image upload, returns extracted color list as JSON
        [HttpPost("colors")]

        public async Task<IActionResult> ExtractColors(IFormFile file)
        {
            // check if file is valid
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided.");

            // retrieve image file stream and convert to byte array
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            // decode image file stream to raw BGRA pixel array
            using Image<Bgra32> image = Image.Load<Bgra32>(fileBytes);
            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);

            // run Core engine
            ImageProcessing processor = new ImageProcessing(pixelBytes, image.Width, image.Height);
            processor.FindColors();

            Dictionary<RgbColor, int> colorMap = processor.GetColors();
            ColorIdentifier classifier = new ColorIdentifier();

            // convert color map to DTO list
            var response = colorMap.Select(kvp => new ColorInfoDto
            {
                R = kvp.Key.R,
                G = kvp.Key.G,
                B = kvp.Key.B,
                Hex = $"#{kvp.Key.R:X2}{kvp.Key.G:X2}{kvp.Key.B:X2}",
                Name = classifier.GetColorName(kvp.Key),
                PixelCount = kvp.Value,
                TexturePath = processor.GetTextureForColor(kvp.Key)
            }).ToList();

            return Ok(response);
        }

        // accepts image upload, applies textures, returns final PNG file stream
        [HttpPost("process")]
        public async Task<IActionResult> ProcessTextures(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided.");

            // retrieve image file stream and convert to byte array
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            // decode image file stream to raw BGRA pixel array
            using Image<Bgra32> image = Image.Load<Bgra32>(fileBytes);
            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);

            // run Core engine
            ImageProcessing processor = new ImageProcessing(pixelBytes, image.Width, image.Height);
            processor.FindColors();
            byte[] outputPixels = processor.ApplyTextures();

            // Encode modified BGRA bytes back to PNG format stream
            using Image<Bgra32> resultImage = Image.LoadPixelData<Bgra32>(outputPixels, image.Width, image.Height);
            using var outputStream = new MemoryStream();
            await resultImage.SaveAsPngAsync(outputStream);

            // return the PNG file stream as a response
            return File(outputStream.ToArray(), "image/png");
        }
    }
}

