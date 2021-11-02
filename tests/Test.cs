using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ZlibSharp.Tests;

public class Test
{
    private readonly ITestOutputHelper FakeConsole;
    
    private readonly byte[] SourceString, SourceStringCompressed, SourceBuffer;

    private readonly int LengthOfCompressed;

    private struct GetStreamCopyPostProcessor: IZPostProcessor
    {
        public ZStream StreamCopy;
        
        public unsafe void Execute(ZStream* StreamPtr, ZlibResult Result)
        {
            StreamCopy = *StreamPtr;
        }
    }
    
    public Test(ITestOutputHelper fakeConsole)
    {
        FakeConsole = fakeConsole;
        
        SourceString = File.ReadAllBytes("SourceText.txt");

        var DestBuffer = new byte[SourceString.Length];

        var Processor = new GetStreamCopyPostProcessor();
        
        MemoryZlib.Compress(SourceString, DestBuffer, ref Processor);

        LengthOfCompressed = (int) Processor.StreamCopy.TotalBytesWritten;

        SourceStringCompressed = new byte[LengthOfCompressed];
        
        DestBuffer.AsSpan(0, LengthOfCompressed).CopyTo(SourceStringCompressed);

        SourceBuffer = new byte[SourceString.Length];
    }
    
    [Fact]
    public void DecompressionWorks()
    {
        var Processor = new GetStreamCopyPostProcessor();

        MemoryZlib.Decompress(SourceStringCompressed, SourceBuffer, ref Processor);
            
        Processor.StreamCopy.TotalBytesUnprocessed.Should().Be(0);

        SourceBuffer.Should().Equal(SourceString);
    }
    
    [Fact]
    public void DecompressionToUnderAllocatedBufferReturnsNonZeroValue()
    {
        const int UndersizedBufferLength = 69;

        UndersizedBufferLength.Should().BeLessThan(SourceBuffer.Length);
        
        var UndersizedDestBuffer = new byte[UndersizedBufferLength];

        var Processor = new GetStreamCopyPostProcessor();
        
        MemoryZlib.Decompress(SourceStringCompressed, UndersizedDestBuffer, ref Processor);
            
        Processor.StreamCopy.TotalBytesUnprocessed.Should().NotBe(0);

        Processor.StreamCopy.TotalBytesWritten.Should().Be(UndersizedBufferLength);
    }
    
    [Fact]
    public void CompressionToUnderAllocatedBufferTotalBytesUnprocessedIsNonZeroValue()
    {
        const int UndersizedBufferLength = 69;

        UndersizedBufferLength.Should().BeLessThan(LengthOfCompressed);
        
        var UndersizedDestBuffer = new byte[UndersizedBufferLength];
        
        var Processor = new GetStreamCopyPostProcessor();

        var Span = UndersizedDestBuffer.AsSpan();
        
        MemoryZlib.Compress(SourceString, Span, ref Processor);

        Processor.StreamCopy.TotalBytesUnprocessed.Should().NotBe(0);
    }
    
    [Fact]
    public void DecompressionToOverAllocatedBufferShouldHaveBytesWrittenEqualToSourceStringLength()
    {
        const uint OversizeBy = 69;

        var SourceLength = (uint) SourceString.Length;
        
        var OversizedDestBuffer = new byte[SourceLength + OversizeBy];
        
        var Processor = new GetStreamCopyPostProcessor();
        
        MemoryZlib.Decompress(SourceStringCompressed, OversizedDestBuffer, ref Processor);
            
        Processor.StreamCopy.TotalBytesUnprocessed.Should().Be(0);

        Processor.StreamCopy.TotalBytesWritten.Should().Be(SourceLength);
    }
}