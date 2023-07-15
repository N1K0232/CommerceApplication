﻿namespace CommerceApi.Security.Abstractions
{
    public interface IPathGenerator
    {
        string? Generate(string? fileName);

        string? Generate(string? path, string? input, string extension);
    }
}