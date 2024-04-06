using System.Data;

namespace PngDecoder.Chunks;

public class IHDR : Chunk
{
    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }

    public int ColorDepth { get; private set; }
    public ColorType ColorType { get; private set; }

    public bool Interlacing { get; private set; }

    public IHDR(byte[] bytes)
    {
        Parse(bytes);
    }

    private void Parse(byte[] bytes)
    {
        var chunkBytes = bytes.Skip(8).ToArray();
        var dataSize = Util.CombineBytes(chunkBytes.Take(4).ToArray());

        ChunkBytes = chunkBytes.Take(dataSize + 12).ToArray();
        
        // name - INDR
        NameBytes = ChunkBytes.Skip(4).Take(4).ToArray();
        Util.CompareBytesASCII("IHDR name", "IHDR", NameBytes);

        Data = ChunkBytes.Skip(8).Take(dataSize).ToArray();
        
        ImageWidth = Util.CombineBytes(Data.Take(4).ToArray());
        ImageHeight = Util.CombineBytes(Data.Skip(4).Take(4).ToArray());
        ColorDepth = Data[8];

        if (!Enum.IsDefined(typeof(ColorType), (int)Data[9]))
            throw new DataException($"Incorrect color type in IHDR chunk: {Data[9]}");

        ColorType = (ColorType)Data[9];

        // compress type - Deflate
        if (Data[10] != 0x00)
            throw new DataException($"Incorrect compress type in IHDR chunk: {Data[10]}");

        if (Data[11] != 0x00)
            throw new DataException($"Incorrect filtering type in IHDR chunk: {Data[11]}");

        Interlacing = Convert.ToBoolean(Data[12]);

        var crc32 = CRC32Hasher.Hash(ChunkBytes.Skip(4).Take(4 + dataSize).ToArray());
        var crc32FromFile = Util.CombineBytes(ChunkBytes.Skip(21).ToArray());
        if (crc32 != crc32FromFile)
            throw new DataException($"Incorrect CRC32 in IHDR chunk: {crc32}. Must be {crc32FromFile}");

        CRC32 = crc32;
    }
}