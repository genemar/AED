namespace AEDFinder.Web.Models;

public class TenantBrandingOptions
{
    public TenantTheme Default { get; set; } = new();

    public List<TenantTheme> Tenants { get; set; } = new();
}

public class TenantTheme
{
    public string? Host { get; set; }

    public string Name { get; set; } = "AED Vinden";

    public string PrimaryColor { get; set; } = "#006837";

    public string AccentColor { get; set; } = "#24A148";

    public string? LogoPath { get; set; }
}
