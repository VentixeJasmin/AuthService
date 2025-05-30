﻿using System.ComponentModel.DataAnnotations;

namespace AuthService.Models; 

public class UserDto
{
    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must contain at least 8 characters, one uppercase, one lowercase, one digit and one special character")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;

    
    [Range(typeof(bool), "true", "true", ErrorMessage = "Required")]
    public bool AcceptTerms { get; set; }
}

