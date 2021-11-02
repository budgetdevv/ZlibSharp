// Copyright (c) 2021, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

namespace ZlibSharp;

public unsafe struct ZStream
{
    public byte* next_in;
    public uint avail_in;
    public CULong total_in;

    public byte* next_out;
    public uint avail_out;
    public CULong total_out;
    public byte* msg;

    internal readonly internal_state* state; // not visible by applications.

    public delegate* unmanaged[Cdecl] <void*, uint, uint, void*> zalloc;
    public delegate* unmanaged[Cdecl] <void*, void*> zfree;
    public void* opaque;

    public int data_type;

    public CULong adler;
    public CULong reserved; // reserved for future use in zlib.

    //Helpers
    public uint TotalBytesWritten
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint) total_out.Value;
    }
    
    public uint TotalBytesUnprocessed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => avail_in;
    }
    
    public uint Adler32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint) (adler.Value & 0xffff);
    }
}
