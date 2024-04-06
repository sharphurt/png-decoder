using System.Drawing;
using NLog;
using PngDecoder.Chunks;
using PngDecoder.Deflate;

namespace PngDecoder;

public class PngDecoder
{
    public void Decode(string path)
    {
        var bytes = File.ReadAllBytes(path);
        CheckSignature(bytes);

        var ihdr = new IHDR(bytes);
        var index = 8 + ihdr.Size;

        var pixels = new List<Color[]>();
        while (index < bytes.Length)
        {
            var c = Chunk.GetNextChunk(bytes, index);
            if (c.Name == "IDAT")
                pixels = ParseIDAT(c, ihdr);
            index += c.Size;
        }

        if (pixels.Any())
        {
            var image = new Bitmap(ihdr.ImageWidth, ihdr.ImageHeight);
            for (int i = 0; i < ihdr.ImageHeight; i++)
            {
                for (int j = 0; j < ihdr.ImageWidth; j++)
                {
                    image.SetPixel(j, i, pixels[i][j]);
                }
            }
            
            image.Save("test.bmp");
        }

        Console.WriteLine(CheckPaletteChunkExists(bytes, ihdr));
    }

    public bool CheckPaletteChunkExists(byte[] bytes, IHDR ihdr)
    {
        return Chunk.GetNextChunk(bytes, 8 + ihdr.Size).Name == "PLTE";
    }

    public List<Color[]> ParseIDAT(Chunk chunk, IHDR ihdr)
    {
        
        var bytesPerChannel = ihdr.ColorDepth / 8;
        var channels = Util.ColorTypeToChannelsCount(ihdr.ColorType);
        var rowLength = (channels * bytesPerChannel) * ihdr.ImageWidth + 1;
        
        var decompressed = ZLibDecompressor.ZLibDotnetDecompress(chunk.Data, rowLength * ihdr.ImageHeight);
        var index = 0;

        var pixelRows = new List<Color[]>();
        for (var row = 0; row < ihdr.ImageHeight; row++)
        {
            var rowWithFilter = decompressed.Skip(rowLength * row).Take(rowLength).ToArray();
            var filter = rowWithFilter[0];
            var pixels = Util.ByteArrayToPixelArray(rowWithFilter.Skip(1).ToArray(), ihdr.ColorDepth, ihdr.ColorType);
            pixelRows.Add(pixels);
        }

        return pixelRows;
    }

    public void CheckSignature(byte[] bytes)
    {
        var signature = bytes.Take(8).ToArray();

        Util.CompareBytes("0x89 independent", 0x89, signature[0]);
        Util.CompareBytes("PNG ASCII", 0x504e47, signature.Skip(1).Take(3).ToArray());
        Util.CompareBytes("CR LF", 0x0d0a, signature.Skip(4).Take(2).ToArray());
        Util.CompareBytes("DOS text input end", 0x1a, signature[6]);
        Util.CompareBytes("LF", 0x0a, signature[7]);
    }
}