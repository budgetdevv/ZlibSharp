// Copyright (c) 2021, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

namespace ZlibSharp;

/// <summary>
/// Zlib Memory Compression and Decompression Helper Class.
/// </summary>
public static unsafe class MemoryZlib
{
    private struct GetAdler32DataPostProcessor: IZPostProcessor
    {
        public uint Adler32;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ZStream* StreamPtr, ZlibResult Result)
        {
            Adler32 = StreamPtr->Adler32;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Compress(byte[] Source, byte[] Dest, out uint Adler32, ZlibCompressionLevel CompressionLevel = ZlibCompressionLevel.DefaultCompression)
    {
        Compress(Source.AsSpan(), Dest.AsSpan(), out Adler32, CompressionLevel);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Compress(string SourcePath, byte[] Dest, out uint Adler32, ZlibCompressionLevel CompressionLevel = ZlibCompressionLevel.DefaultCompression)
    {
        var Source = File.ReadAllBytes(SourcePath);
        
        Compress(Source.AsSpan(), Dest.AsSpan(), out Adler32, CompressionLevel);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Compress(Span<byte> Source, Span<byte> Dest, out uint Adler32, ZlibCompressionLevel CompressionLevel = ZlibCompressionLevel.DefaultCompression)
    {
        var Processor = new GetAdler32DataPostProcessor();
        
        Compress(Source, Dest, ref Processor, CompressionLevel);

        Adler32 = Processor.Adler32;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Compress<PostProcessorT>(Span<byte> Source, Span<byte> Dest, ref PostProcessorT Processor, ZlibCompressionLevel CompressionLevel = ZlibCompressionLevel.DefaultCompression)
        where PostProcessorT: IZPostProcessor
    {
        ZlibHelper.Compress(Source, Dest, CompressionLevel, ref Processor);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decompress(string SourcePath, byte[] Dest, out uint Adler32)
    {
        var Source = File.ReadAllBytes(SourcePath);
        
        Decompress(Source, Dest, out Adler32);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decompress(byte[] Source, byte[] Dest, out uint Adler32)
    {
        Decompress(Source.AsSpan(), Dest.AsSpan(), out Adler32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decompress(Span<byte> Source, Span<byte> Dest, out uint Adler32)
    {
        var Processor = new GetAdler32DataPostProcessor();
        
        ZlibHelper.Decompress(Source, Dest, ref Processor);

        Adler32 = Processor.Adler32;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decompress<PostProcessorT>(Span<byte> Source, Span<byte> Dest, ref PostProcessorT Processor)
        where PostProcessorT : IZPostProcessor
    {
        ZlibHelper.Decompress(Source, Dest, ref Processor);
    }

    /// <summary>
    /// Check data for compression by zlib.
    /// </summary>
    /// <param name="Source">Input stream.</param>
    /// <returns>Returns <see langword="true" /> if data is compressed by zlib, else <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="Source"/> is <see langword="null" />.</exception>
    public static bool IsCompressedByZlib(Span<byte> Source)
    {
        if (Source.Length >= 2)
        {
            ref var SourceRef = ref MemoryMarshal.GetReference(Source);

            var byte1 = SourceRef;

            var byte2 = Unsafe.Add(ref SourceRef, 1);
        
            return byte1 is 0x78 && byte2 is 0x01 or 0x5E or 0x9C or 0xDA;
        }

        throw new ArgumentNullException(nameof(Source));
    }

    /// <summary>
    /// Check data for compression by zlib.
    /// </summary>
    /// <param name="Path">The file to check on if it is compressed by zlib.</param>
    /// <returns>Returns <see langword="true" /> if data is compressed by zlib, else <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="Path"/> is <see langword="null" /> or <see cref="string.Empty"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCompressedByZlib(string Path)
        => IsCompressedByZlib(File.ReadAllBytes(Path));

    /// <summary>
    /// Check data for compression by zlib.
    /// </summary>
    /// <param name="Data">Input array.</param>
    /// <returns>Returns <see langword="true" /> if data is compressed by zlib, else <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="Data"/> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCompressedByZlib(byte[] Data)
        => IsCompressedByZlib(Data.AsSpan());

    // NEW: Zlib version check.

    /// <summary>
    /// Gets the version to ZlibSharp.
    /// </summary>
    /// <returns>The version string to this version of ZlibSharp.</returns>
    public static string ZlibVersion()
        => typeof(MemoryZlib).Assembly.GetName().Version!.ToString(3);

    // NEW: Adler32 hasher.

    /// <summary>
    /// Gets the Adler32 checksum of the input data at the specified index and length.
    /// </summary>
    /// <param name="Data">The data to checksum.</param>
    /// <returns>The Adler32 hash of the input data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ZlibGetAdler32(byte[] Data)
    {
        return ZlibGetAdler32(Data.AsSpan());
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ZlibGetAdler32(Span<byte> Data)
    {
        fixed (byte* DataPtr = Data)
        {
            return UnsafeNativeMethods.adler32(
                UnsafeNativeMethods.adler32(0L, null, 0),
                DataPtr,
                (uint) Data.Length);
        }
    }
}
