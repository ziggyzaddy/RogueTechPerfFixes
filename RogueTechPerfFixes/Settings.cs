namespace RogueTechPerfFixes;

public class Settings
{
    public bool LogError = true;

    public bool LogDebug = false;

    public bool LogWarning = false;

    public PatchOption Patch = new();

    public class PatchOption
    {
        public bool Vanilla = true;

        public bool LowVisibility = true;
    }
}