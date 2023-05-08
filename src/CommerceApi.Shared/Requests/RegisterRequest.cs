﻿namespace CommerceApi.Shared.Requests;

public class RegisterRequest
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string ConfirmPhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string ConfirmEmail { get; set; } = null!;

    public string? UserName { get; set; }

    public string Password { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;
}