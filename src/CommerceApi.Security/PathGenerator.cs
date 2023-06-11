using System;
using System.IO;
using CommerceApi.Security.Abstractions;

namespace CommerceApi.Security
{
    public class PathGenerator : IPathGenerator
    {
        public string? Generate(string? fileName)
        {
            if (fileName is null)
            {
                return null;
            }

            var year = DateTime.UtcNow.Year.ToString("0000");
            var month = DateTime.UtcNow.Month.ToString("00");
            var day = DateTime.UtcNow.Day.ToString("00");

            return Path.Combine(year, month, day, fileName);
        }

        public string? Generate(string? path, string? input, string extension)
        {
            if (path is null || input is null)
            {
                return null;
            }

            return $@"{path}\{input}.{extension}";
        }
    }
}