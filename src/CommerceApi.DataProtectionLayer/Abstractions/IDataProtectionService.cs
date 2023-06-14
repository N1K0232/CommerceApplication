﻿namespace CommerceApi.DataProtectionLayer.Abstractions;

public interface IDataProtectionService
{
    string? Protect(string input);

    string? Unprotect(string input);
}