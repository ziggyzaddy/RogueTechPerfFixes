namespace RogueTechPerfFixes;

public class Settings
{
    public PatchOption Patch = new();

    public class PatchOption
    {
        public bool Vanilla = true;

        public bool LowVisibility = false;
    }
}