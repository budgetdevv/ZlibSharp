namespace ZlibSharp
{
    public unsafe interface IZPostProcessor
    {
        public void Execute(ZStream* StreamPtr, ZlibResult Result);
    }
}