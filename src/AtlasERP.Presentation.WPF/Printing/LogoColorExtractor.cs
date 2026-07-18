using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AtlasERP.Presentation.WPF.Printing;

/// <summary>
/// Best-effort dominant-color extraction from a logo image using only WPF's built-in
/// imaging types — no external image-processing package, to avoid another
/// dependency-install failure point.
/// </summary>
public static class LogoColorExtractor
{
    public static string? TryExtractDominantColorHex(string imagePath)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);

            var width = converted.PixelWidth;
            var height = converted.PixelHeight;
            if (width == 0 || height == 0)
            {
                return null;
            }

            var stride = width * 4;
            var pixels = new byte[height * stride];
            converted.CopyPixels(pixels, stride, 0);

            var totalPixels = width * height;
            var step = Math.Max(1, totalPixels / 20000);

            long vibrantR = 0, vibrantG = 0, vibrantB = 0, vibrantCount = 0;
            long fallbackR = 0, fallbackG = 0, fallbackB = 0, fallbackCount = 0;

            for (var i = 0; i < totalPixels; i += step)
            {
                var offset = i * 4;
                var b = pixels[offset];
                var g = pixels[offset + 1];
                var r = pixels[offset + 2];
                var a = pixels[offset + 3];

                if (a < 64)
                {
                    continue; // mostly transparent
                }

                fallbackR += r; fallbackG += g; fallbackB += b; fallbackCount++;

                var max = Math.Max(r, Math.Max(g, b));
                var min = Math.Min(r, Math.Min(g, b));
                var saturation = max == 0 ? 0 : (double)(max - min) / max;
                var value = max / 255.0;

                // Skip near-white, near-black, and low-saturation (gray) pixels — almost
                // always background, not the actual brand color.
                if (value > 0.92 || value < 0.12 || saturation < 0.18)
                {
                    continue;
                }

                vibrantR += r; vibrantG += g; vibrantB += b; vibrantCount++;
            }

            if (vibrantCount > 0)
            {
                return ToHex((byte)(vibrantR / vibrantCount), (byte)(vibrantG / vibrantCount), (byte)(vibrantB / vibrantCount));
            }

            if (fallbackCount > 0)
            {
                return ToHex((byte)(fallbackR / fallbackCount), (byte)(fallbackG / fallbackCount), (byte)(fallbackB / fallbackCount));
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string ToHex(byte r, byte g, byte b) => $"#{r:X2}{g:X2}{b:X2}";
}
