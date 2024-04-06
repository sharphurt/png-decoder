using System.IO.Compression;
using zlib;

namespace PngDecoder.Deflate;


// https://github.com/nayuki/Simple-DEFLATE-decompressor

public class ZLibDecompressor
{
    public static byte[] Decompress(byte[] bytes)
    {
        using FileStream outputFileStream = File.Create("decompressed.txt");
        using var decompressor = new ZLibStream(new MemoryStream(bytes), CompressionMode.Decompress);

        using var memoryStream = new MemoryStream();
        decompressor.CopyTo(memoryStream);
        decompressor.CopyTo(outputFileStream);

        return memoryStream.ToArray();
    } 
    
    public static byte[] ZLibDotnetDecompress(byte[] data, int size)
    {
        MemoryStream compressed = new MemoryStream(data);
        ZInputStream inputStream = new ZInputStream(compressed);
        byte[] result = new byte[size];   //  Since zinputstream inherits binaryreader instead of stream, it can only prepare the output buffer in advance and use read to obtain fixed length data.
        inputStream. read(result, 0, result.Length); //  Note that the initial of read here is lowercase
        return result;
    }
}