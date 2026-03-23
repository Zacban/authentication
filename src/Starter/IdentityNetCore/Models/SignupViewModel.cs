using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;

namespace IdentityNetCore.Models;

public record SignupViewModel
{
    [Required]
    [DataType(DataType.EmailAddress, ErrorMessage = "Invalid email address.")]
    public string? Email { get; init; }

    [Required]
    [DataType(DataType.Password, ErrorMessage = "Incorrect or missing password.")]
    public string? Password { get; init; }
}