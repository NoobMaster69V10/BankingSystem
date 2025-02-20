﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingSystem.Core.DTO;

public class PersonRegisterDto
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Lastname is required.")]
    public string Lastname { get; set; }

    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")]
    public string IdNumber { get; set; }

    [Required(ErrorMessage = "Birth date is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format.")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; }

    [AllowedValues("Operator", "Person", ErrorMessage = "Role must be either 'Operator' or 'Person'.")]
    [JsonIgnore]
    public string Role { get; set; } = "Person";
}