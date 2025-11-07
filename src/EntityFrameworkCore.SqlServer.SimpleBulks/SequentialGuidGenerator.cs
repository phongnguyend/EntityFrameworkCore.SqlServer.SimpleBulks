#if NET9_0_OR_GREATER
using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class SequentialGuidGenerator
{
    public static Guid Next()
    {
        return Next(DateTimeOffset.UtcNow);
    }

    public static Guid Next(DateTimeOffset timeNow)
    {
        return Guid.CreateVersion7(timeNow);
    }
}

#else
using System;
using System.Security.Cryptography;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class SequentialGuidGenerator
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public static Guid Next()
    {
        return Next(DateTimeOffset.UtcNow);
    }

    public static Guid Next(DateTimeOffset timeNow)
    {
        var randomBytes = new byte[7];
        _rng.GetBytes(randomBytes);
        var ticks = (ulong)timeNow.Ticks;

        var uuidVersion = (ushort)4;
        var uuidVariant = (ushort)0b1000;

        var ticksAndVersion = (ushort)((ticks << 48 >> 52) | (ushort)(uuidVersion << 12));
        var ticksAndVariant = (byte)((ticks << 60 >> 60) | (byte)(uuidVariant << 4));

        var guid = new Guid((uint)(ticks >> 32), (ushort)(ticks << 32 >> 48), ticksAndVersion,
            ticksAndVariant,
            randomBytes[0],
            randomBytes[1],
            randomBytes[2],
            randomBytes[3],
            randomBytes[4],
            randomBytes[5],
            randomBytes[6]);

        return guid;
    }
}

#endif