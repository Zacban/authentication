using System.ComponentModel.DataAnnotations;

namespace IdentityNetCore.Models;

public record LoginWith2faViewModel
{
    [Required]
    [Display(Name = "Authenticator code")]
    public string? TwoFactorCode { get; init; }

    public bool RememberMe { get; init; }
}
