using System;
using System.IO;
using System.Text;
using Jither.IO.Types;

namespace Jither.IO;

public class BinWriter : IDisposable
{
    private bool disposed;
    protected internal Stream stream;
    private readonly bool ownStream;

    // Constant buffer for write operations:
    protected readonly byte[] buffer = new byte[32];

    public string Name
    {
        get; protected set;
    }

    public uint Position
    {
        get
        {
            return (uint)stream.Position;
        }
        set
        {
            stream.Position = value;
        }
    }

    public uint Size
    {
        get
        {
            return (uint)stream.Length;
        }
    }

    public BinWriter(Stream stream, bool ownStream = false)
    {
        this.stream = stream;
        this.ownStream = ownStream;
    }

    public BinWriter(string path)
        : this(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        this.Name = Path.GetFileName(path);
    }

    // BYTES:

    public void WriteU8(byte value)
    {
        buffer[0] = value;
        InternalWrite(buffer, 1);
    }

    public void WriteS8(sbyte value)
    {
        buffer[0] = (byte)value;
        InternalWrite(buffer, 1);
    }

    // LITTLE ENDIAN:

    public void WriteU16LE(ushort value)
    {
        buffer[0] = (byte)value;
        buffer[1] = (byte)(value >> 8);
        InternalWrite(buffer, 2);
    }

    public void WriteU24LE(uint value)
    {
        buffer[0] = (byte)value;
        buffer[1] = (byte)(value >> 8);
        buffer[2] = (byte)(value >> 16);
        InternalWrite(buffer, 3);
    }

    public void WriteU32LE(uint value)
    {
        buffer[0] = (byte)value;
        buffer[1] = (byte)(value >> 8);
        buffer[2] = (byte)(value >> 16);
        buffer[3] = (byte)(value >> 24);
        InternalWrite(buffer, 4);
    }

    public void WriteS16LE(short value)
    {
        buffer[0] = (byte)value;
        buffer[1] = (byte)(value >> 8);
        InternalWrite(buffer, 2);
    }

    public void WriteS32LE(int value)
    {
        buffer[0] = (byte)value;
        buffer[1] = (byte)(value >> 8);
        buffer[2] = (byte)(value >> 16);
        buffer[3] = (byte)(value >> 24);
        InternalWrite(buffer, 4);
    }

    // BIG ENDIAN:

    public void WriteU16BE(ushort value)
    {
        buffer[0] = (byte)(value >> 8);
        buffer[1] = (byte)(value);
        InternalWrite(buffer, 2);
    }

    public void WriteU24BE(uint value)
    {
        buffer[0] = (byte)(value >> 16);
        buffer[1] = (byte)(value >> 8);
        buffer[2] = (byte)(value);
        InternalWrite(buffer, 3);
    }

    public void WriteU32BE(uint value)
    {
        buffer[0] = (byte)(value >> 24);
        buffer[1] = (byte)(value >> 16);
        buffer[2] = (byte)(value >> 8);
        buffer[3] = (byte)(value);
        InternalWrite(buffer, 4);
    }

    public void WriteS16BE(short value)
    {
        buffer[0] = (byte)(value >> 8);
        buffer[1] = (byte)(value);
        InternalWrite(buffer, 2);
    }

    public void ReadS32BE(int value)
    {
        buffer[0] = (byte)(value >> 24);
        buffer[1] = (byte)(value >> 16);
        buffer[2] = (byte)(value >> 8);
        buffer[3] = (byte)(value);
        InternalWrite(buffer, 4);
    }

    public void WriteVariableLength(uint value)
    {
        int count = 1;
        if ((value & 0xF0000000) > 0)
        {
            count = 5;
        }
        else if ((value & 0x0FE00000) > 0)
        {
            count = 4;
        }
        else if ((value & 0x001FC000) > 0)
        {
            count = 3;
        }
        else if ((value & 0x00003F80) > 0)
        {
            count = 2;
        }

        for (int i = 1; i <= count; i++)
        {
            byte b = (byte)((value >> ((count - i) * 7)) & 0x7F);
            if (i < count) b |= 0x80;

            WriteU8(b);
        }
    }

    // STRINGS:

    // TODO: Optimize
    public void WriteStringZ(string value)
    {
        byte[] strBuffer = Encoding.ASCII.GetBytes(value);
        InternalWrite(strBuffer, value.Length);
        WriteU8(0);
    }

    public void WriteString(string value)
    {
        byte[] strBuffer = Encoding.ASCII.GetBytes(value);
        InternalWrite(strBuffer, value.Length);
    }

    public void WriteFourCC(FourCC value)
    {
        WriteU32LE(value.NumericValue);
    }

    public void WriteTwoCC(TwoCC value)
    {
        WriteU16LE(value.NumericValue);
    }

    [Obsolete("Bad argument order - use (byte[], int) instead")]
    public void Write(int count, byte[] value)
    {
        InternalWrite(value, count);
    }

    public void Write(byte[] value, int count)
    {
        InternalWrite(value, count);
    }

    protected virtual void InternalWrite(byte[] aBuffer, int count)
    {
        stream.Write(aBuffer, 0, count);
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            if (ownStream)
            {
                stream.Close();
                stream = null;
            }
            disposed = true;
        }
    }

}
