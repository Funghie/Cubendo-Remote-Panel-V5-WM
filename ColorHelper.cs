// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System.Drawing;
using System.Text.RegularExpressions;

namespace Cubendo_Remote_Panel
{
    public static class ColorHelper
    {
        /// <summary>
        /// Safely parses a color string (name or hex, with or without #). Returns defaultColor if invalid.
        /// </summary>
        public static Color ParseOrDefault(string colorValue, Color defaultColor)
        {
            if (string.IsNullOrWhiteSpace(colorValue))
                return defaultColor;

            // Normalize: add '#' if it's a 6-digit hex without prefix
            if (Regex.IsMatch(colorValue, @"^[0-9a-fA-F]{6}$"))
                colorValue = "#" + colorValue;

            try
            {
                Color color = ColorTranslator.FromHtml(colorValue);
                if (color.A < 255) // Ensure fully opaque
                    return defaultColor;
                return color;
            }
            catch
            {
                Color color = Color.FromName(colorValue);
                if ((!color.IsKnownColor && color.A == 0) || color.A < 255)
                    return defaultColor;
                return color;
            }
        }

        /// <summary>
        /// Converts a Color to an HTML color string (e.g. "#RRGGBB" or color name).
        /// </summary>
        public static string ToHtml(Color color)
        {
            return ColorTranslator.ToHtml(color);
        }
    }
}
