using System.Data;
using System.Drawing;

namespace PngDecoder;

public static class Util
{
    public static Int32 CombineBytes(params byte[] bytes)
    {
        var result = 0;

        for (int i = 0; i < bytes.Length; i++) result |= bytes[i] << (bytes.Length - 1 - i) * 8;

        return result;
    }

    public static void CompareBytes(string name, Int32 value, params byte[] bytes)
    {
        var combined = CombineBytes(bytes);

        if (combined != value)
            throw new DataException($"{name} bytes incorrect: {combined:X}. Must be: {value:X}");
    }

    public static void CompareBytesASCII(string name, string value, params byte[] bytes)
    {
        var combined = CombineBytes(bytes);
        var asciiString = BytesToASCII(bytes);
        if (asciiString != value)
            throw new DataException(
                $"{name} bytes incorrect: {combined:X} (in ASCII: {asciiString}). Must be: {CharactersToBytes(value):X} (in ASCII: {value})");
    }

    public static Int32 CharactersToBytes(string s) => CombineBytes(s.Select(c => (byte)c).ToArray());

    public static string BytesToASCII(byte[] bytes) => String.Join("", bytes.Select(Convert.ToChar));

    public static int ColorTypeToChannelsCount(ColorType type) =>
        type switch
        {
            ColorType.HalfTone => 2,
            ColorType.HalfToneAlpha => 3,
            ColorType.RGB => 3,
            ColorType.RGBA => 4,
            _ => 0
        };

    public static Color[] ByteArrayToPixelArray(byte[] bytes, int colorDepth, ColorType colorType)
    {
        var bytesPerChannel = colorDepth / 8;
        var channels = Util.ColorTypeToChannelsCount(colorType);
        var bytesPerPixel = bytesPerChannel * channels;

        var pixels = new List<Color>();

        for (int i = 0; i < bytes.Length; i += bytesPerPixel)
        {
            var pixelBytes = bytes.Skip(i).Take(bytesPerChannel * channels).ToArray();
            var rBytes = pixelBytes.Take(bytesPerChannel).ToArray();
            var gBytes = pixelBytes.Skip(bytesPerChannel).Take(bytesPerChannel).ToArray();
            var bBytes = pixelBytes.Skip(bytesPerChannel * 2).Take(bytesPerChannel).ToArray();
            
            var aBytes = pixelBytes.Skip(bytesPerChannel * 3).Take(bytesPerChannel).ToArray();

            var r = CombineBytes(rBytes);
            var g = CombineBytes(gBytes);
            var b = CombineBytes(bBytes);

            var a = CombineBytes(aBytes);

            pixels.Add(Color.FromArgb(a, r, g, b));
        }

        return pixels.ToArray();
    }
}