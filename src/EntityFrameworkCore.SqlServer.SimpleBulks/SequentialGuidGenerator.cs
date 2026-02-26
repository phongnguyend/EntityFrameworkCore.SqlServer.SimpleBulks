using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Threading;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

/// <summary>
/// https://github.com/dotnet/efcore/blob/main/src/EFCore/ValueGeneration/SequentialGuidValueGenerator.cs
/// https://github.com/dotnet/efcore/issues/33579
/// https://github.com/dotnet/efcore/issues/30753
/// </summary>
public static class SequentialGuidGenerator
{
    public static long GetTicks()
    {
        return DateTime.UtcNow.Ticks;
    }

    public static Guid Next(ref long start)
    {
        var guid = Guid.NewGuid();

        var counter = BitConverter.IsLittleEndian
            ? Interlocked.Increment(ref start)
            : BinaryPrimitives.ReverseEndianness(Interlocked.Increment(ref start));

        var counterBytes = MemoryMarshal.AsBytes(
            new ReadOnlySpan<long>(in counter));

        // Guid uses a sequential layout where the first 8 bytes (_a, _b, _c)
        // are subject to byte-swapping on big-endian systems when reading from
        // or writing to a byte array (e.g., via MemoryMarshal or Guid constructors).
        // The remaining 8 bytes (_d through _k) are interpreted as-is,
        // regardless of endianness.
        //
        // Since we only modify the last 8 bytes of the Guid (bytes 8–15),
        // byte order does not affect the result.
        //
        // This allows us to safely use MemoryMarshal.AsBytes to directly access
        // and modify the Guid's underlying bytes without any extra conversions,
        // which also slightly improves performance on big-endian architectures.
        var guidBytes = MemoryMarshal.AsBytes(
            new Span<Guid>(ref guid));

        guidBytes[08] = counterBytes[1];
        guidBytes[09] = counterBytes[0];
        guidBytes[10] = counterBytes[7];
        guidBytes[11] = counterBytes[6];
        guidBytes[12] = counterBytes[5];
        guidBytes[13] = counterBytes[4];
        guidBytes[14] = counterBytes[3];
        guidBytes[15] = counterBytes[2];

        return guid;
    }
}