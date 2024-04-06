using System.Data;

namespace PngDecoder.Chunks;

public class Chunk
{
    public int Size => 4 + 4 + Data.Length + 4;
    
    public byte[] NameBytes { get; protected set; }
    public string Name => Util.BytesToASCII(NameBytes);

    public byte[] ChunkBytes { get; protected set; }
    public byte[] Data { get; protected set; }
    public int CRC32 { get; protected set; }

    public bool IsIndependent => char.IsUpper(Name[0]);
    public bool IsOfficial => char.IsUpper(Name[1]);
    public bool IsCopyable => char.IsUpper(Name[3]);

    public static Chunk GetNextChunk(byte[] bytes, int startByte)
    {
        var dataSize = Util.CombineBytes(bytes.Skip(startByte).Take(4).ToArray());

        var chunkBytes = bytes.Skip(startByte).Take(dataSize + 12).ToArray();

        var nameBytes = chunkBytes.Skip(4).Take(4).ToArray();
        var nameAscii = Util.BytesToASCII(nameBytes);
        var data = chunkBytes.Skip(8).Take(dataSize).ToArray();
        var crc32 = CRC32Hasher.Hash(chunkBytes.Skip(4).Take(4 + dataSize).ToArray());
        var crc32FromFile = Util.CombineBytes(chunkBytes.Skip(8 + dataSize).Take(4).ToArray());

        if (crc32 != crc32FromFile)
            throw new DataException($"Incorrect CRC32 in chunk: {crc32}. Must be {crc32FromFile}");

        return new Chunk
        {
            ChunkBytes = chunkBytes,
            CRC32 = crc32,
            Data = data,
            NameBytes = nameBytes
        };
    }
}