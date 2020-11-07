using System;
//using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common.Tar.Headers;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

public static class BinaryPrimitives
{
    public static void WriteInt32LittleEndian(byte [] output,int offset, Int32 i)
    {
        byte[] bf = BitConverter.GetBytes(i);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bf);
        }
        Array.Copy(bf, 0, output, offset, 4);
    }
    public static void WriteInt64BigEndian(byte[] output, int offset, Int64 i)
    {
        byte[] bf = BitConverter.GetBytes(i);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bf);
        }
        Array.Copy(bf, 0, output, offset, 8);
    }

    public static int ReadInt32LittleEndian(byte [] buff,int offset=0)
    {
        if (buff.Length - offset < 4) return 0;
        if(BitConverter.IsLittleEndian)
        {
            return BitConverter.ToInt32(buff, offset);
        }
        else
        {
            byte[] bf = new byte[4];
            Array.Copy(buff,offset,bf,0,4);
            Array.Reverse(bf);
            return BitConverter.ToInt32(bf, 0);
        }
    }
    public static System.UInt32 ReadUInt32LittleEndian(byte[] buff, int offset = 0)
    {
        if (buff.Length - offset < 4) return 0;
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.ToUInt32(buff, offset);
        }
        else
        {
            byte[] bf = new byte[4];
            Array.Copy(buff, offset, bf, 0, 4);
            Array.Reverse(bf);
            return BitConverter.ToUInt32(bf, 0);
        }
    }
    public static System.Int16 ReadInt16LittleEndian(byte[] buff, int offset=0)
    {
        if (buff.Length - offset < 2) return 0;
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.ToInt16(buff, offset);
        }
        else
        {
            byte[] bf = new byte[2];
            Array.Copy(buff, offset, bf, 0, 2);
            Array.Reverse(bf);
            return BitConverter.ToInt16(bf, 0);
        }
    }
    public static System.UInt16 ReadUInt16LittleEndian(byte[] buff, int offset = 0)
    {
        if (buff.Length - offset < 2) return 0;
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.ToUInt16(buff, offset);
        }
        else
        {
            byte[] bf = new byte[2];
            Array.Copy(buff, offset, bf, 0, 2);
            Array.Reverse(bf);
            return BitConverter.ToUInt16(bf, 0);
        }
    }
    public static System.Int64 ReadInt64LittleEndian(byte[] buff, int offset=0)
    {
        if (buff.Length - offset < 8) return 0;
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.ToInt64(buff, offset);
        }
        else
        {
            byte[] bf = new byte[8];
            Array.Copy(buff, offset, bf, 0, 8);
            Array.Reverse(bf);
            return BitConverter.ToInt64(bf, 0);
        }
    }
    public static System.Int64 ReadInt64BigEndian(byte[] buff, int offset = 0)
    {
        if (buff.Length - offset < 8) return 0;
        if (!BitConverter.IsLittleEndian)
        {
            return BitConverter.ToInt64(buff, offset);
        }
        else
        {
            byte[] bf = new byte[8];
            Array.Copy(buff, offset, bf, 0, 8);
            Array.Reverse(bf);
            return BitConverter.ToInt64(bf, 0);
        }
    }
    public static System.UInt64 ReadUInt64LittleEndian(byte[] buff, int offset = 0)
    {
        if (buff.Length - offset < 8) return 0;
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.ToUInt64(buff, offset);
        }
        else
        {
            byte[] bf = new byte[8];
            Array.Copy(buff, offset, bf, 0, 8);
            Array.Reverse(bf);
            return BitConverter.ToUInt64(bf, 0);
        }
    }

}
/*
public static partial class BinaryPrimitives
{
    /// <summary>
    /// Reads a <see cref="double" /> from the beginning of a read-only span of bytes, as little endian.
    /// </summary>
    /// <param name="source">The read-only span to read.</param>
    /// <returns>The little endian value.</returns>
    /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="source"/> is too small to contain a <see cref="double" />.
    /// </exception>
    public static double ReadDoubleLittleEndian(byte[]source)
    {
        return !BitConverter.IsLittleEndian ?
            BitConverter.Int64BitsToDouble(ReverseEndianness(MemoryMarshal.Read<long>(source))) :
            MemoryMarshal.Read<double>(source);
    }

    /// <summary>
    /// Reads an Int16 out of a read-only span of bytes as little endian.
    /// </summary>
 
    public static short ReadInt16LittleEndian(byte[]source)
    {
        short result = MemoryMarshal.Read<short>(source);
        if (!BitConverter.IsLittleEndian)
        {
            result = ReverseEndianness(result);
        }
        return result;
    }

    /// <summary>
    /// Reads an Int32 out of a read-only span of bytes as little endian.
    /// </summary>

    public static int ReadInt32LittleEndian(byte[]source)
    {
        int result = MemoryMarshal.Read<int>(source);
        if (!BitConverter.IsLittleEndian)
        {
            result = ReverseEndianness(result);
        }
        return result;
    }

    /// <summary>
    /// Reads an Int64 out of a read-only span of bytes as little endian.
    /// </summary>

    public static long ReadInt64LittleEndian(byte[]source)
    {
        long result = MemoryMarshal.Read<long>(source);
        if (!BitConverter.IsLittleEndian)
        {
            result = ReverseEndianness(result);
        }
        return result;
    }

    /// <summary>
    /// Reads a <see cref="float" /> from the beginning of a read-only span of bytes, as little endian.
    /// </summary>
    /// <param name="source">The read-only span to read.</param>
    /// <returns>The little endian value.</returns>
    /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="source"/> is too small to contain a <see cref="float" />.
    /// </exception>

    public static float ReadSingleLittleEndian(byte[]source)
    {
        return !BitConverter.IsLittleEndian ?
            BitConverter.Int32BitsToSingle(ReverseEndianness(MemoryMarshal.Read<int>(source))) :
            MemoryMarshal.Read<float>(source);
    }

    /// <summary>
    /// Reads a UInt16 out of a read-only span of bytes as little endian.
    /// </summary>

    public static ushort ReadUInt16LittleEndian(byte[]source)
    {
        ushort result = MemoryMarshal.Read<ushort>(source);
        if (!BitConverter.IsLittleEndian)
        {
            result = ReverseEndianness(result);
        }
        return result;
    }

    /// <summary>
    /// Reads a UInt32 out of a read-only span of bytes as little endian.
    /// </summary>
 
    public static uint ReadUInt32LittleEndian(byte[]source)
    {
        uint result = MemoryMarshal.Read<uint>(source);
        if (!BitConverter.IsLittleEndian)
        {
            result = ReverseEndianness(result);
        }
        return result;
    }

    /// <summary>
    /// Reads a UInt64 out of a read-only span of bytes as little endian.
    /// </summary>
  
    public static ulong ReadUInt64LittleEndian(byte[]source)
    {
        ulong result = MemoryMarshal.Read<ulong>(source);
        if (!BitConverter.IsLittleEndian)
        {
            result = ReverseEndianness(result);
        }
        return result;
    }

    /// <summary>
    /// Reads a <see cref="double" /> from the beginning of a read-only span of bytes, as little endian.
    /// </summary>
    /// <param name="source">The read-only span of bytes to read.</param>
    /// <param name="value">When this method returns, the value read out of the read-only span of bytes, as little endian.</param>
    /// <returns>
    /// <see langword="true" /> if the span is large enough to contain a <see cref="double" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>

    public static bool TryReadDoubleLittleEndian(byte[]source, out double value)
    {
        if (!BitConverter.IsLittleEndian)
        {
            bool success = MemoryMarshal.TryRead(source, out long tmp);
            value = BitConverter.Int64BitsToDouble(ReverseEndianness(tmp));
            return success;
        }

        return MemoryMarshal.TryRead(source, out value);
    }

    /// <summary>
    /// Reads an Int16 out of a read-only span of bytes as little endian.
    /// </summary>
    /// <returns>If the span is too small to contain an Int16, return false.</returns>
 
    public static bool TryReadInt16LittleEndian(byte[]source, out short value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return MemoryMarshal.TryRead(source, out value);
        }

        bool success = MemoryMarshal.TryRead(source, out short tmp);
        value = ReverseEndianness(tmp);
        return success;
    }

    /// <summary>
    /// Reads an Int32 out of a read-only span of bytes as little endian.
    /// </summary>
    /// <returns>If the span is too small to contain an Int32, return false.</returns>

    public static bool TryReadInt32LittleEndian(byte[]source, out int value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return MemoryMarshal.TryRead(source, out value);
        }

        bool success = MemoryMarshal.TryRead(source, out int tmp);
        value = ReverseEndianness(tmp);
        return success;
    }

    /// <summary>
    /// Reads an Int64 out of a read-only span of bytes as little endian.
    /// </summary>
    /// <returns>If the span is too small to contain an Int64, return false.</returns>

    public static bool TryReadInt64LittleEndian(byte[]source, out long value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return MemoryMarshal.TryRead(source, out value);
        }

        bool success = MemoryMarshal.TryRead(source, out long tmp);
        value = ReverseEndianness(tmp);
        return success;
    }

    /// <summary>
    /// Reads a <see cref="float" /> from the beginning of a read-only span of bytes, as little endian.
    /// </summary>
    /// <param name="source">The read-only span of bytes to read.</param>
    /// <param name="value">When this method returns, the value read out of the read-only span of bytes, as little endian.</param>
    /// <returns>
    /// <see langword="true" /> if the span is large enough to contain a <see cref="float" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
    public static bool TryReadSingleLittleEndian(byte[]source, out float value)
    {
        if (!BitConverter.IsLittleEndian)
        {
            bool success = MemoryMarshal.TryRead(source, out int tmp);
            value = BitConverter.Int32BitsToSingle(ReverseEndianness(tmp));
            return success;
        }

        return MemoryMarshal.TryRead(source, out value);
    }

    /// <summary>
    /// Reads a UInt16 out of a read-only span of bytes as little endian.
    /// </summary>
    /// <returns>If the span is too small to contain a UInt16, return false.</returns>

    public static bool TryReadUInt16LittleEndian(byte[]source, out ushort value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return MemoryMarshal.TryRead(source, out value);
        }

        bool success = MemoryMarshal.TryRead(source, out ushort tmp);
        value = ReverseEndianness(tmp);
        return success;
    }

    /// <summary>
    /// Reads a UInt32 out of a read-only span of bytes as little endian.
    /// </summary>
    /// <returns>If the span is too small to contain a UInt32, return false.</returns>

    public static bool TryReadUInt32LittleEndian(byte[]source, out uint value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return MemoryMarshal.TryRead(source, out value);
        }

        bool success = MemoryMarshal.TryRead(source, out uint tmp);
        value = ReverseEndianness(tmp);
        return success;
    }

    /// <summary>
    /// Reads a UInt64 out of a read-only span of bytes as little endian.
    /// </summary>
    /// <returns>If the span is too small to contain a UInt64, return false.</returns>

    public static bool TryReadUInt64LittleEndian(byte[]source, out ulong value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return MemoryMarshal.TryRead(source, out value);
        }

        bool success = MemoryMarshal.TryRead(source, out ulong tmp);
        value = ReverseEndianness(tmp);
        return success;
    }
}*/

namespace SharpCompress.Common.GZip
{
    internal sealed class GZipFilePart : FilePart
    {
        private string _name;
        private readonly Stream _stream;

        internal GZipFilePart(Stream stream, ArchiveEncoding archiveEncoding)
            : base(archiveEncoding)
        {
            ReadAndValidateGzipHeader(stream);
            EntryStartPosition = stream.Position;
            _stream = stream;
        }

        internal long EntryStartPosition { get; }

        internal DateTime? DateModified { get; private set; }

        internal override string FilePartName => _name;

        internal override Stream GetCompressedStream()
        {
            return new DeflateStream(_stream, CompressionMode.Decompress, CompressionLevel.Default);
        }

        internal override Stream GetRawStream()
        {
            return _stream;
        }
        
        private void ReadAndValidateGzipHeader(Stream stream)
        {
            // read the header on the first read
            byte[] header = new byte[10];
            int n = stream.Read(header,0,10);

            // workitem 8501: handle edge case (decompress empty stream)
            if (n == 0)
            {
                return;
            }

            if (n != 10)
            {
                throw new ZlibException("Not a valid GZIP stream.");
            }

            if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
            {
                throw new ZlibException("Bad GZIP header.");
            }

            int timet = BinaryPrimitives.ReadInt32LittleEndian(header,(4));
            DateModified = TarHeader.EPOCH.AddSeconds(timet);
            if ((header[3] & 0x04) == 0x04)
            {
                // read and discard extra field
                n = stream.Read(header,0, 2); // 2-byte length field

                short extraLength = (short)(header[0] + header[1] * 256);
                byte[] extra = new byte[extraLength];

                if (!stream.ReadFully(extra))
                {
                    throw new ZlibException("Unexpected end-of-file reading GZIP header.");
                }
                n = extraLength;
            }
            if ((header[3] & 0x08) == 0x08)
            {
                _name = ReadZeroTerminatedString(stream);
            }
            if ((header[3] & 0x10) == 0x010)
            {
                ReadZeroTerminatedString(stream);
            }
            if ((header[3] & 0x02) == 0x02)
            {
                stream.ReadByte(); // CRC16, ignore
            }
        }

        private string ReadZeroTerminatedString(Stream stream)
        {
            byte[] buf1 = new byte[1];
            var list = new List<byte>();
            bool done = false;
            do
            {
                // workitem 7740
                int n = stream.Read(buf1, 0, 1);
                if (n != 1)
                {
                    throw new ZlibException("Unexpected EOF reading GZIP header.");
                }
                if (buf1[0] == 0)
                {
                    done = true;
                }
                else
                {
                    list.Add(buf1[0]);
                }
            }
            while (!done);
            byte[] buffer = list.ToArray();
            return ArchiveEncoding.Decode(buffer);
        }
    }
}
