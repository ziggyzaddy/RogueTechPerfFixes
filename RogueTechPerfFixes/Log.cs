using HBS.Logging;
using ModTek.Public;

namespace RogueTechPerfFixes;

public static class Log
{
    public static readonly NullableLogger Main = NullableLogger.GetLogger(nameof(RogueTechPerfFixes), LogLevel.Error);
}
