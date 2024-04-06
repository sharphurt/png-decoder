namespace PngDecoder;

public static class CRC32Hasher
{
    public static int Hash(byte[] bytes)
    {
        var crc = 0xFFFFFFFF;

        for (var i = 0; i < bytes.Length; i++)
        {
            var ch = bytes[i];
            for (var j = 0; j < 8; j++)
            {
                var b = (ch ^ crc) & 1;
                crc >>= 1;
                if (b != 0)
                    crc ^= 0xEDB88320;
                ch >>= 1;
            }
        }

        return (int)~crc;
    }
}