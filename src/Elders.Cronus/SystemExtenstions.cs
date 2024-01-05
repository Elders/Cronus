namespace System;

internal static class SystemExtenstions
{
    public static DateTimeOffset ToDateTimeOffsetUtc(this long fileTimeUtc)
    {
        DateTime crutch = DateTime.FromFileTimeUtc(fileTimeUtc);
        return new DateTimeOffset(crutch, TimeSpan.Zero);
    }
}
