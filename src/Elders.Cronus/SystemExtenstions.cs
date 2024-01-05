namespace System;

internal static class SystemExtenstions
{
    public static DateTimeOffset ToDateTimeOffsetUtc(this long fileTimeUtc)
    {
        DateTime local = DateTime.FromFileTimeUtc(fileTimeUtc);
        return new DateTimeOffset(local, TimeSpan.Zero);
    }
}
