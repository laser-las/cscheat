public sealed class Settings
{
    public bool ShowBox { get; set; } = true;
    public bool ShowSkeleton { get; set; } = true;
    public bool ShowNames { get; set; } = true;
    public bool ShowHealth { get; set; } = true;
    public bool ShowWeapon { get; set; } = true;
    public bool ShowDistance { get; set; } = true;
    public float MaxDistanceMeters { get; set; } = 250f;
}
