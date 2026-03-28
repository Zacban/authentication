namespace IdentityNetCore.Models;

public record TwoFactorViewModel
{
    public string? Token { get; init; }
    public string? Code { get; set; }
    public string? QrCodeUri { get; set; }
}