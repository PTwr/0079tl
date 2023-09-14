namespace Tests;

using InMemoryBinaryFile.Helpers;

public class StringTests
{
    byte[] asciiBytesWithstring = new byte[] { 1, 1, 1, 65, 66, 67, 0, 0, 0, 2, 2, 2 };
    byte[] UTF8BytesWithstring = new byte[] { 1, 1, 1, 196, 133, 196, 153, 196, 135, 0, 0, 0, 2, 2, 2 };

    [Fact]
    public void NullTerminatedStringExtraction()
    {
        Assert.Equal("ABC", asciiBytesWithstring.AsSpan(3).FindNullTerminator().ToAsciiString());
        Assert.Equal("¹êæ", UTF8BytesWithstring.AsSpan(3).FindNullTerminator().ToUTF8String());
    }
}