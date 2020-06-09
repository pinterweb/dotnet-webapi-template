﻿namespace BusinessApp.Data
{
    using System.Linq;

    public static class StringExtensions
    {
        public static string ConvertToPascalCase(this string str)
        {
            return string.Join(
                ".",
                str.Split('.')
                    .Select(s => char.ToUpper(s.ToCharArray()[0]) + s.Substring(1))
            );
        }
    }
}
