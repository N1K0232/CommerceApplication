﻿using CommerceApi.DataProtectionLayer.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CommerceApi.DataProtectionLayer.Services;

internal class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _dataProtector;

    public DataProtectionService(IDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public string? Protect(string input)
    {
        if (input is null)
        {
            return null;
        }

        var protectedString = _dataProtector.Protect(input);
        return protectedString;
    }

    public string? Unprotect(string input)
    {
        if (input is null)
        {
            return null;
        }

        var unprotectedString = _dataProtector.Unprotect(input);
        return unprotectedString;
    }
}