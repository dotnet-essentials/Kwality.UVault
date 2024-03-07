namespace Kwality.UVault.QA.Common.Properties;

public static class Time
{
#pragma warning disable S6354
    public static DateTime Now => DateTime.Now;
    public static DateTime Tomorrow => Now.AddHours(24);
#pragma warning restore S6354
}
