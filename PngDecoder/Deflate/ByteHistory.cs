namespace PngDecoder.Deflate;

public class ByteHistory
{
    private byte[] _data;

    private int _index;

    public ByteHistory(int size)
    {
        if (size < 1)
            throw new ArgumentException("Size must be positive");

        _data = new byte[size];
        _index = 0;
    }

    public void Append(byte b)
    {
        if (_index < 0 || _index >= _data.Length)
            throw new InvalidOperationException("Byte history is full");

        _data[_index] = b;
        _index = (_index + 1) % _data.Length;
    }

    public void Copy(int dist, int len, StreamWriter streamWriter)
    {
        if (len < 0 || dist < 1 || dist > _data.Length)
            throw new InvalidOperationException();

        int readIndex = (_index - dist + _data.Length) % _data.Length;
        if (readIndex < 0 || readIndex >= _data.Length)
            throw new InvalidOperationException();

        for (int i = 0; i < len; i++)
        {
            byte b = _data[readIndex];
            readIndex = (readIndex + 1) % _data.Length;
                streamWriter.Write(b);
            Append(b);
        }
    }
}