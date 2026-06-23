namespace SpiceNet.UWP.Models;

public enum FitMode
{
    Center, ScaleToFit
}

public enum AppTheme
{
    System, Dark, Light
}

public class EmulatedKey
{
    public VirtualKey Modifier { get; set; }
    public VirtualKey Extra { get; set; }
    public VirtualKey Key { get; set; }
}