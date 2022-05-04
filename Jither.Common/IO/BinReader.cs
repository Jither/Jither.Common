using Jither.IO.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Jither.IO;

public class BinReader : IDisposable
{
    private bool disposed;

    protected Stream stream;
    private readonly bool ownStream;
    private readonly byte[] buffer = new byte[32];
    private readonly byte[] endianConversionBuffer = new byte[32];

    public long Position
    {
        get => stream.Position;
        set
        {
            stream.Position = value;
        }
    }

    public long Size => stream.Length;

    public BinReader(Stream stream, bool ownStream = false)
    {
        this.stream = stream;
        this.ownStream = ownStream;
    }

    public byte ReadU8()
    {
        InternalRead(1);
        return buffer[0];
    }

    public sbyte ReadS8()
    {
        InternalRead(1);
        return (sbyte)buffer[0];
    }

    // Little Endian
    public ushort ReadU16LE()
    {
        InternalRead(2);
        return (ushort)(buffer[0] | buffer[1] << 8);
    }

    public uint ReadU24LE()
    {
        InternalRead(3);
        return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16);
    }

    public uint ReadU32LE()
    {
        InternalRead(4);
        return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
    }

    public ulong ReadU64LE()
    {
        return ReadU32LE() | (ulong)ReadU32LE() << 32;
    }

    public short ReadS16LE()
    {
        InternalRead(2);
        return (short)(buffer[0] | buffer[1] << 8);
    }

    public int ReadS32LE()
    {
        InternalRead(4);
        return (int)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
    }

    public long ReadS64LE()
    {
        return (long)(ReadU32LE() | (ulong)ReadU32LE() << 32);
    }

    public float ReadSingleLE()
    {
        InternalRead(4);
        ReverseBytes(4);
        return BitConverter.ToSingle(endianConversionBuffer, 0);
    }

    public double ReadDoubleLE()
    {
        InternalRead(8);
        ReverseBytes(8);
        return BitConverter.ToDouble(endianConversionBuffer, 0);
    }

    // Big Endian

    public ushort ReadU16BE()
    {
        InternalRead(2);
        return (ushort)(buffer[0] << 8 | buffer[1]);
    }

    public uint ReadU24BE()
    {
        InternalRead(3);
        return (uint)(buffer[0] << 16 | buffer[1] << 8 | buffer[2]);
    }

    public uint ReadU32BE()
    {
        InternalRead(4);
        return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
    }

    public short ReadS16BE()
    {
        InternalRead(2);
        return (short)(buffer[0] << 8 | buffer[1]);
    }

    public int ReadS32BE()
    {
        InternalRead(4);
        return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
    }

    public long ReadS64BE()
    {
        return (long)((ulong)ReadU32BE() << 32 | ReadU32BE());
    }

    public ulong ReadU64BE()
    {
        return (ulong)ReadU32BE() << 32 | ReadU32BE();
    }

    public Single ReadSingleBE()
    {
        InternalRead(4);
        return BitConverter.ToSingle(buffer, 0);
    }

    public Double ReadDoubleBE()
    {
        InternalRead(4);
        return BitConverter.ToDouble(buffer, 0);
    }

    // Strings and special purpose

    /// <summary>
    /// Reads a variable length unsigned integer. Used by e.g. MIDI.
    /// Value is Big Endian - the value consists of the 7 low bits of
    /// each byte.
    /// The high bit of each byte indicates whether more bytes follow.
    /// I.e. 1_0000011b 0_1101001b => 111101001
    /// </summary>
    public uint ReadVariableLengthU32BE()
    {
        uint result = 0;
        while (true)
        {
            byte b = ReadU8();
            result <<= 7;
            result |= (uint)(b & 0x7f);
            if ((b & 0x80) == 0)
            {
                return result;
            }
        }
    }

    public string ReadStringZ()
    {
        var builder = new StringBuilder();
        byte b;
        while (true)
        {
            b = ReadU8();
            if (b == 0)
            {
                return builder.ToString();
            }
            builder.Append((char)b);
        }
    }

    public string ReadStringZ(int maxLength)
    {
        byte[] buffer = new byte[maxLength];
        if (Position + maxLength > Size)
        {
            maxLength = (int)(Size - Position);
        }
        InternalRead(buffer, maxLength);
        int length = Array.IndexOf(buffer, (byte)0);
        if (length < 0)
        {
            length = maxLength;
        }
        return Encoding.ASCII.GetString(buffer, 0, length);
    }

    public string ReadXorString(int length, byte xor = 0)
    {
        byte[] buffer = new byte[length];
        InternalRead(buffer, length);
        if (xor != 0)
        {
            for (int i = 0; i < length; i++)
            {
                buffer[i] ^= xor;
            }
        }
        // Don't ignore zero termination
        var actualLength = Array.IndexOf(buffer, (byte)0);
        if (actualLength >= 0)
        {
            length = actualLength;
        }
        return Encoding.ASCII.GetString(buffer, 0, length);
    }

    public string ReadXorStringZ(byte xor, bool zeroIsEncrypted = false)
    {
        var builder = new StringBuilder();
        // If the null terminator is encrypted, it will be 0 after decryption
        // If it's *not* encrypted, it will be the xor value
        int terminator = zeroIsEncrypted ? 0 : xor;
        int b;
        while (true)
        {
            b = ReadU8() ^ xor;
            if (b == terminator)
            {
                return builder.ToString();
            }
            builder.Append((char)b);
        }
    }

    public FourCC ReadFourCC()
    {
        return new FourCC(ReadU32LE());
    }

    public TwoCC ReadTwoCC()
    {
        return new TwoCC(ReadU16LE());
    }

    public int Read(int count, out byte[] result)
    {
        result = new byte[count];
        return InternalRead(result, count);
    }

    public int ReadToEnd(out byte[] result)
    {
        return Read((int)(Size - Position), out result);
    }

    public int Read(byte[] buffer, int bufferOffset, int count)
    {
        return InternalRead(buffer, count, bufferOffset);
    }

    private void ReverseBytes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            endianConversionBuffer[i] = buffer[count - (i + 1)];
        }
    }

    private int InternalRead(int count)
    {
        return InternalRead(buffer, count);
    }

    private int InternalRead(byte[] destinationBuffer, int count, int destinationBufferOffset = 0)
    {
        Debug.Assert(count <= destinationBuffer.Length);
        if (Position + count > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(count), $"Attempt to read beyond end of stream ({Position + count - Size} bytes past {Size})");
        }
        Debug.Assert(Position + count <= Size);
        return stream.Read(destinationBuffer, destinationBufferOffset, count);
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!disposed)
        {
            if (ownStream)
            {
                stream.Dispose();
                stream = null;
            }
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
