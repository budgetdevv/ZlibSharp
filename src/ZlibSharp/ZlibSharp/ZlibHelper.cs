// Copyright (c) 2021, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

namespace ZlibSharp;

internal static unsafe class ZlibHelper
{
    private static void InitializeInflate(ZStream* StreamPtr)
    {
        var Result = UnsafeNativeMethods.inflateInit_(StreamPtr, UnsafeNativeMethods.zlibVersion(), sizeof(ZStream));
        
        if (Result != ZlibResult.Ok)
        {
            throw new NotPackableException($"{nameof(InitializeInflate)} failed - ({Result}) {Marshal.PtrToStringUTF8((nint) StreamPtr->msg)}");        
        }
    }

    private static void InitializeDeflate(ZStream* StreamPtr, ZlibCompressionLevel CompressionLevel)
    {
        var Result = UnsafeNativeMethods.deflateInit_(StreamPtr, CompressionLevel, UnsafeNativeMethods.zlibVersion(), sizeof(ZStream));
        
        if (Result != ZlibResult.Ok)
        {
            throw new NotPackableException($"{nameof(InitializeDeflate)} failed - ({Result}) {Marshal.PtrToStringUTF8((nint) StreamPtr->msg)}");        
        }
    }

    internal static void InflateEnd(ZStream* StreamPtr)
    {
        var Result = UnsafeNativeMethods.inflateEnd(StreamPtr);
        
        if (Result != ZlibResult.Ok)
        {
            throw new NotPackableException($"{nameof(InflateEnd)} failed - ({Result}) {Marshal.PtrToStringUTF8((nint) StreamPtr->msg)}");
        }
    }

    private static void DeflateEnd(ZStream* StreamPtr)
    {
        var Result = UnsafeNativeMethods.deflateEnd(StreamPtr);
        
        if (Result != ZlibResult.Ok)
        {
            throw new NotPackableException($"{nameof(DeflateEnd)} failed - ({Result}) {Marshal.PtrToStringUTF8((nint) StreamPtr->msg)}");
        }
    }
    internal static void Compress<PostProcessorT>(Span<byte> Source, Span<byte> Dest, ZlibCompressionLevel CompressionLevel, ref PostProcessorT Processor)
        where PostProcessorT: IZPostProcessor
    {
        ZStream Stream;

        var StreamPtr = &Stream;
        
        //We skipped initialization
        StreamPtr->zalloc = null;
        StreamPtr->zfree = null;
        
        fixed (byte* SourcePtr = Source)
        fixed (byte* DestPtr = Dest)
        {
            InitializeDeflate(StreamPtr, CompressionLevel);
            
            StreamPtr->next_in = SourcePtr;
            StreamPtr->avail_in = (uint) Source.Length;
            StreamPtr->next_out = DestPtr;
            StreamPtr->avail_out = (uint) Dest.Length;

            // while ((Result = UnsafeNativeMethods.deflate(StreamPtr, ZlibFlushStrategy.NoFlush)) == ZlibResult.Ok)
            // {
            //     if (StreamPtr->avail_in == 0)
            //     {
            //         UnsafeNativeMethods.deflate(StreamPtr, ZlibFlushStrategy.Finish);
            //     }
            // }

            //TODO: Find out why UnsafeNativeMethods.deflate(StreamPtr, ZlibFlushStrategy.Finish); sets avail_in to 0, even on dest buffer under-allocation
            //Expected behavior: avail_in should be number of unprocessed bytes
            
            var OriginalIn = StreamPtr->avail_in;
            
            var Result = UnsafeNativeMethods.deflate(StreamPtr, ZlibFlushStrategy.Finish);

            StreamPtr->avail_in = OriginalIn;
            
            Processor.Execute(StreamPtr, Result);

            DeflateEnd(StreamPtr);
        }
    }

    //Decompress returns avail_in, allowing users to reallocate and continue decompressing remaining data
    //should Dest buffer be under-allocated

    internal static void Decompress<PostProcessorT>(Span<byte> Source, Span<byte> Dest, ref PostProcessorT Processor)
        where PostProcessorT: IZPostProcessor
    {
        ZStream Stream;

        var StreamPtr = &Stream;

        //We skipped initialization
        StreamPtr->zalloc = null;
        StreamPtr->zfree = null;
        
        fixed (byte* SourcePtr = Source)
        fixed (byte* DestPtr = Dest)
        {
            InitializeInflate(StreamPtr);
            
            StreamPtr->next_in = SourcePtr;
            StreamPtr->avail_in = (uint) Source.Length;
            StreamPtr->next_out = DestPtr;
            StreamPtr->avail_out = (uint) Dest.Length;

            // ZlibResult Result;
            //
            // while ((Result = UnsafeNativeMethods.inflate(StreamPtr, ZlibFlushStrategy.NoFlush)) == ZlibResult.Ok)
            // {
            //     if (StreamPtr->avail_in == 0)
            //     {
            //         UnsafeNativeMethods.inflate(StreamPtr, ZlibFlushStrategy.Finish);
            //     }
            // }
            
            var Result = UnsafeNativeMethods.inflate(StreamPtr, ZlibFlushStrategy.Finish);
            
            Processor.Execute(StreamPtr, Result);

            InflateEnd(StreamPtr);
        }
    }

    internal static uint GetAdler32(ZStream* StreamPtr)
    {
        return (uint) (StreamPtr->adler.Value & 0xffff);
    }
}
