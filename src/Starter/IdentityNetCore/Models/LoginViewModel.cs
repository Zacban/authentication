using System.ComponentModel.DataAnnotations;

namespace IdentityNetCore.Models;

public record LoginViewModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; init; }

    public bool RememberMe { get; init; }
}
