using UnityEngine;

namespace PEC2.Utilities
{
    /// <summary>
    /// Class <c>ColorStrings</c> is used to convert a color to a string and vice versa.
    /// </summary>
    public static class ColorStrings
    {

        /// <summary>
        /// Method <c>ColorToString</c> is used to convert a color to a string.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The color as a string.</returns>
        public static string ColorToString(Color color)
        {
            return $"{color.r * 255},{color.g * 255},{color.b * 255}";
        }
        
        /// <summary>
        /// Method <c>ColorFromString</c> is used to convert a string to a color.
        /// </summary>
        /// <param name="color">The string to convert.</param>
        /// <returns>The string as a color.</returns>
        public static Color ColorFromString(string color)
        {
            var colorRGBArray = color.Split(',');
            return new Color(
                float.Parse(colorRGBArray[0]) / 255f, 
                float.Parse(colorRGBArray[1]) / 255f, 
                float.Parse(colorRGBArray[2]) / 255f);
        }
    }
}
